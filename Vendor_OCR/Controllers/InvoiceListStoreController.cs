using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Vendor_OCR.Repositories;
using Amazon.S3;

namespace Vendor_OCR.Controllers
{
    public class InvoiceListStoreController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _connectionString;
        private readonly VendorRepository _vendorRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;


        private readonly IAmazonS3 _s3;
        private readonly IConfiguration _config;

        public InvoiceListStoreController(IAmazonS3 s3, IConfiguration config, IWebHostEnvironment env,
        VendorRepository vendorRepo, IHttpContextAccessor httpContextAccessor)
        {
            _s3 = s3;
            _config = config;
            _env = env;
            _vendorRepo = vendorRepo;
            _connectionString = config.GetConnectionString("SqlConnectionString");
            _httpContextAccessor = httpContextAccessor;
        }
        private string VendorCode =>
        _httpContextAccessor.HttpContext.Session.GetString("Vendor_Code");
        public IActionResult InvoiceListStore()
        {

            var type = HttpContext.Session.GetString("user_type");

            if (string.IsNullOrEmpty(type))
            {
                return View("~/Views/No Access/NoAccess.cshtml");
            }
            else
            {
                var invoices = _vendorRepo.GetInvoicesSCM("P,A,R,OR", HttpContext.Session.GetString("uid"));
                return View(invoices);
            }
        }


        public IActionResult GetInvoiceFiles(string invoiceNumber)
        {
            try
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
            catch (Exception ex) 
            {
                _vendorRepo.ErrorLog(ex.Message, VendorCode);

                return Json(new
                {
                    success = false,
                    message = "Failed to fetch files."
                });
            }
            
        }


        //public async Task<IActionResult> ApproveInvoices([FromBody] List<int> ids)
        //{
        //    try
        //    {
        //        var vendorCode = HttpContext.Session.GetString("Vendor_Code");
        //        var vendorName = HttpContext.Session.GetString("Vendor_Name");

        //        string primaryBucket = _config["AWS:PrimaryBucket"];
        //        string secondaryBucket = _config["AWS:SecondaryBucket"];

        //        var transfer = new TransferUtility(_s3);

        //        using (SqlConnection con = new SqlConnection(_connectionString))
        //        {
        //            con.Open();

        //            foreach (var id in ids)
        //            {
        //                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.AddWithValue("@mode", "UpdateApproveInvoiceStatus");
        //                    cmd.Parameters.AddWithValue("@condition1", id);
        //                    cmd.Parameters.AddWithValue("@condition2", "A");
        //                    cmd.Parameters.AddWithValue("@condition3", vendorCode);
        //                    cmd.Parameters.AddWithValue("@condition4", vendorName);
        //                    cmd.Parameters.AddWithValue("@condition5", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        //                    string fileName = cmd.ExecuteScalar()?.ToString();

        //                    if (!string.IsNullOrEmpty(fileName))
        //                    {
        //                        var tempPath = Path.Combine(_env.WebRootPath, "tempUploads", fileName);
        //                        var uploadPath = Path.Combine(_env.WebRootPath, "uploads", fileName);

        //                        if (System.IO.File.Exists(tempPath))
        //                        {
        //                            System.IO.File.Copy(tempPath, uploadPath, true);
        //                        }

        //                        if (System.IO.File.Exists(uploadPath))
        //                        {
        //                            await transfer.UploadAsync(uploadPath, primaryBucket, fileName);
        //                            await transfer.UploadAsync(uploadPath, secondaryBucket, fileName);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return Json(new { success = true, message = "Invoices approved successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        //public class RejectInvoiceRequest
        //{
        //    public List<InvoiceItem> Invoices { get; set; }
        //    public string Remark { get; set; }

        //}
        //public class InvoiceItem
        //{
        //    public int Id { get; set; }
        //    public string InvoiceNumber { get; set; }
        //}


        //[HttpPost]
        //public IActionResult RejectInvoices([FromBody] RejectInvoiceRequest request)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_connectionString))
        //        {
        //            con.Open();
        //            foreach (var invoice in request.Invoices)
        //            {
        //                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.AddWithValue("@mode", "UpdateRejectInvoiceStatus");
        //                    cmd.Parameters.AddWithValue("@condition1", invoice.Id);
        //                    cmd.Parameters.AddWithValue("@condition2", "R");
        //                    cmd.Parameters.AddWithValue("@condition3", request.Remark);
        //                    cmd.Parameters.AddWithValue("@condition4", invoice.InvoiceNumber);
        //                    cmd.Parameters.AddWithValue("@condition5", HttpContext.Session.GetString("uid"));
        //                    cmd.Parameters.AddWithValue("@condition6", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        //                    cmd.ExecuteNonQuery();
        //                }
        //            }
        //        }

        //        return Json(new { success = true, message = "Invoices rejected successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}
    }
}
