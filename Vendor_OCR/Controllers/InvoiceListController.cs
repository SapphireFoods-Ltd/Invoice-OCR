using Amazon.S3;
using Amazon.S3.Transfer;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Vendor_OCR.Repositories;
using Amazon.S3;

namespace Vendor_OCR.Controllers
{
    public class InvoiceListController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _connectionString;
        private readonly VendorRepository _vendorRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;


        private readonly IAmazonS3 _s3;
        private readonly IConfiguration _config;

        public InvoiceListController(
        IAmazonS3 s3,
        IConfiguration config,
        IWebHostEnvironment env,
        VendorRepository vendorRepo,
        IHttpContextAccessor httpContextAccessor)
        {
            _s3 = s3;
            _config = config;
            _env = env;
            _vendorRepo = new VendorRepository(config); 
            _connectionString = config.GetConnectionString("SqlConnectionString");
            _httpContextAccessor = httpContextAccessor;
        }
        private string VendorCode =>
        _httpContextAccessor.HttpContext.Session.GetString("Vendor_Code");
        public IActionResult InvoiceList()
        {
            var type = HttpContext.Session.GetString("user_type");
            if (string.IsNullOrEmpty(type))
            {
                return View("~/Views/No Access/NoAccess.cshtml");
            }
            else
            {
                var invoices = _vendorRepo.GetInvoices("P,A,R,OR");
                return View(invoices);
            }
        }


        public IActionResult GetInvoiceFiles(string invoiceNumber)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "tempUploads");

            if (!Directory.Exists(folderPath))
                return Json(new List<string>());

            var files = Directory.GetFiles(folderPath)
                                 .Where(x => Path.GetFileName(x).Contains(invoiceNumber, StringComparison.OrdinalIgnoreCase))
                                 .Select(Path.GetFileName)
                                 .ToList();

            return Json(files);
        }


        public async Task<IActionResult> ApproveInvoices([FromBody] List<int> ids)
        {
            try
            {
                string vendorCode = HttpContext.Session.GetString("Vendor_Code");
                string vendorName = HttpContext.Session.GetString("Vendor_Name");
                string user_type = HttpContext.Session.GetString("user_type");
                string sap_code = "";

                if (user_type == "4" || user_type == "1")
                {
                    vendorCode = HttpContext.Session.GetString("uid");
                    vendorName = HttpContext.Session.GetString("uname");
                    sap_code = HttpContext.Session.GetString("sap_code");
                }

                string primaryBucket = _config["AWS:PrimaryBucket"];
                string secondaryBucket = _config["AWS:SecondaryBucket"];
                string OtherBucket = _config["AWS:OtherBucket"];
                string region = _config["AWS:Region"];

                var transfer = new TransferUtility(_s3);

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    foreach (var id in ids)
                    {
                        using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@mode", "UpdateApproveInvoiceStatus");
                            cmd.Parameters.AddWithValue("@condition1", id);
                            cmd.Parameters.AddWithValue("@condition2", "A");
                            cmd.Parameters.AddWithValue("@condition3", vendorCode);
                            cmd.Parameters.AddWithValue("@condition4", vendorName);
                            cmd.Parameters.AddWithValue("@condition5", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                            string fileName = cmd.ExecuteScalar()?.ToString();

                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var tempPath = Path.Combine(_env.WebRootPath, "tempUploads", fileName);
                                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", fileName);

                                if (System.IO.File.Exists(tempPath))
                                {
                                    System.IO.File.Move(tempPath, uploadPath, true);
                                }
                                await transfer.UploadAsync(uploadPath, primaryBucket, fileName);
                                await transfer.UploadAsync(uploadPath, secondaryBucket, fileName);
                                await transfer.UploadAsync(uploadPath, OtherBucket, fileName);

                                string primaryUrl = $"https://{primaryBucket}.s3.{region}.amazonaws.com/{fileName}";

                                using (SqlCommand cmdd = new SqlCommand("SP_OCR_selectquery", con))
                                {
                                    cmdd.CommandType = CommandType.StoredProcedure;
                                    cmdd.Parameters.AddWithValue("@mode", "InsertPath_S3Bucket");
                                    cmdd.Parameters.AddWithValue("@condition1", primaryUrl);
                                    cmdd.Parameters.AddWithValue("@condition2", fileName);
                                    cmdd.ExecuteNonQuery();

                                }
                                sendmailApprove(id, vendorName);
                            }
                        }
                    }
                }

                return Json(new { success = true, message = "Invoices approved successfully!" });
            }
            catch (Exception ex)
            {
                _vendorRepo.ErrorLog(ex.Message, VendorCode);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class RejectInvoiceRequest
        {
            public List<int> Ids { get; set; }
            public string Remark { get; set; }

        }
        

        [HttpPost]
        public IActionResult RejectInvoices([FromBody] RejectInvoiceRequest request)
        {
            try
            {
                string vendorCode = HttpContext.Session.GetString("Vendor_Code");
                string vendorName = HttpContext.Session.GetString("Vendor_Name");
                string user_type = HttpContext.Session.GetString("user_type");
                string sap_code = "";

                if (user_type == "4")
                {
                    vendorCode = HttpContext.Session.GetString("uid");
                    vendorName = HttpContext.Session.GetString("uname");
                    sap_code = HttpContext.Session.GetString("sap_code");
                }
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    foreach (var id in request.Ids)
                    {
                        using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@mode", "UpdateRejectInvoiceStatus");
                            cmd.Parameters.AddWithValue("@condition1", id);
                            cmd.Parameters.AddWithValue("@condition2", "R");
                            cmd.Parameters.AddWithValue("@condition3", request.Remark);
                            cmd.Parameters.AddWithValue("@condition5", vendorCode);
                            cmd.Parameters.AddWithValue("@condition6", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            cmd.ExecuteNonQuery();
                        }
                        sendmailReject(id, vendorName, request.Remark);
                    }
                }

                return Json(new { success = true, message = "Invoices rejected successfully!" });
            }
            catch (Exception ex)
            {
                _vendorRepo.ErrorLog(ex.Message, VendorCode);
                return Json(new { success = false, message = ex.Message });
            }
        }
        void sendmailReject(int invoiceid, string approvername, string remark)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@mode", "GetInvoiceById");
                        cmd.Parameters.AddWithValue("@condition1", invoiceid);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string invoicename = reader["InvoiceName"].ToString();
                                string uploadername = reader["ins_Name"].ToString();
                                string uploadedon = reader["ins_dt"].ToString();
                                string uploaderEmail = reader["emp_email"].ToString();

                                var request = HttpContext.Request;
                                string baseUrl = $"{request.Scheme}://{request.Host}";

                                string encodedFileName = Uri.EscapeDataString(invoicename);
                                string fileUrl = $"{baseUrl}/tempUploads/{encodedFileName}";

                                string body = $@"
<html>
<body style='font-family:Segoe UI; color:#333333; font-size:15px; line-height:1.6;'>

    <p>Dear <strong>{uploadername}</strong>,</p>

    <p>Your invoice 
    <a href='{fileUrl}' target='_blank' 
       style='color:#007bff; text-decoration:none; font-weight:600;'>
       {invoicename}
    </a>, uploaded on <strong>{uploadedon}</strong>, 
    has been <span style='color:#d9534f; font-weight:600;'>rejected</span> by 
    <strong>{approvername}</strong>(Level 1).</p>

    <p><strong>Reason for Rejection:</strong><br/>
    <em style='color:#d9534f;'>{remark}</em></p>

    <p>Please review the details and re-upload the corrected invoice in the portal.</p>

    <p>Regards,<br/>
    <strong>Sapphire Invoice Management System</strong></p>

</body>
</html>";

                                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                                {
                                    smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
                                    smtp.EnableSsl = true;

                                    var mail = new MailMessage
                                    {
                                        From = new MailAddress("sfil.supports@sapphirefoods.in", "Vendor OCR"),
                                        Subject = "Invoice Rejection Notification",
                                        Body = body,
                                        IsBodyHtml = true
                                    };

                                    mail.To.Add(uploaderEmail); // Replace with dynamic email later
                                    smtp.Send(mail);
                                }

                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusCode(500, new { message = $"Failed to send mail: {ex.Message}" });
            }
        }

        void sendmailApprove(int invoiceid, string approvername)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@mode", "GetInvoiceById");
                        cmd.Parameters.AddWithValue("@condition1", invoiceid);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string invoicename = reader["InvoiceName"].ToString();
                                string uploadername = reader["ins_Name"].ToString();
                                string uploadedon = reader["ins_dt"].ToString();
                                string uploaderEmail = reader["emp_email"].ToString();

                                var request = HttpContext.Request;
                                string baseUrl = $"{request.Scheme}://{request.Host}";

                                string encodedFileName = Uri.EscapeDataString(invoicename);
                                string fileUrl = $"{baseUrl}/uploads/{encodedFileName}";

                                // Email body
                                string body = $@"
<html>
<body style='font-family:Segoe UI; color:#333333; font-size:15px; line-height:1.6;'>

    <p>Dear <strong>{uploadername}</strong>,</p>

    <p>Your invoice 
    <a href='{fileUrl}' target='_blank' 
       style='color:#007bff; text-decoration:none; font-weight:600;'>
       {invoicename}
    </a>, uploaded on <strong>{uploadedon}</strong>, 
    has been <span style='color:#28a745; font-weight:600;'>Approved</span> by 
    <strong>{approvername}</strong>.</p>

    <p>Regards,<br/>
    <strong>Sapphire Invoice Management System</strong></p>

</body>
</html>";

                                // Send email
                                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                                {
                                    smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
                                    smtp.EnableSsl = true;

                                    var mail = new MailMessage
                                    {
                                        From = new MailAddress("sfil.supports@sapphirefoods.in", "Vendor OCR"),
                                        Subject = "Invoice Approval Notification",
                                        Body = body,
                                        IsBodyHtml = true
                                    };

                                    mail.To.Add(uploaderEmail); // Replace with dynamic email later
                                    smtp.Send(mail);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusCode(500, new { message = $"Failed to send mail: {ex.Message}" });
            }
        }
    }
}
