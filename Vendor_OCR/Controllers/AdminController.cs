

using DnsClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Vendor_OCR.Repositories;
using Vendor_OCR.Filters;
using Vendor_OCR.Models;

namespace Vendor_OCR.Controllers
{
    [Authorize]
    [SessionAuthorize("1")]
    public class AdminController : Controller
    {
        private readonly VendorRepository _repo;
        private readonly ILogger<AdminController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        string empCode;
        
        public AdminController(IConfiguration config, ILogger<AdminController> logger, IHttpContextAccessor accessor)
        {
            _repo = new VendorRepository(config);
            _logger = logger;
            _httpContextAccessor = accessor;
        }
        private string VendorCode =>
        _httpContextAccessor.HttpContext.Session.GetString("Vendor_Code");

        [HttpGet]
        public IActionResult ManageVendors()
        {
            //empCode = HttpContext.Session.GetString("Vendor_Code");
            ViewBag.EmpCode = VendorCode;
            return View();
            
        }

        // POST: /Admin/AddVendor
        //[HttpPost]
        //public IActionResult AddVendor(AdminModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        _logger.LogInformation("Name or Email got empty {Time}", DateTime.Now);
        //        return View("ManageVendors", model);
        //    }


        //    try
        //    {
        //        string newVendorCode = _repo.AddVendorEmail(model);

        //        string baseUrl = $"{Request.Scheme}://{Request.Host}";
        //        string link = $"{baseUrl}/RegisterVendor/Activate?vendorCode={WebUtility.UrlEncode(newVendorCode)}";


        //        string body = $@"
        //    <html>
        //    <body style='font-family:Segoe UI, sans-serif;'>
        //        <h3>Hello {model.VendorName},</h3>
        //        <p>Welcome! Please click the link below to complete your registration or view your vendor portal:</p>
        //        <p><a href='{link}' target='_blank' 
        //            style='background-color:#007bff; color:white; padding:10px 18px; text-decoration:none; border-radius:5px;'>Open Vendor Portal</a></p>
        //        <br/>
        //        <p>Thank you,<br/><strong>Admin Team</strong></p>
        //    </body>
        //    </html>";


        //        using (var smtp = new SmtpClient("smtp.office365.com", 587))
        //        {
        //            smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
        //            smtp.EnableSsl = true;

        //            var mail = new MailMessage();
        //            mail.From = new MailAddress("sfil.supports@sapphirefoods.in", "Vendor OCR");
        //            mail.To.Add(model.Email);
        //            mail.Subject = "Welcome to Vendor Portal";
        //            mail.Body = body;
        //            mail.IsBodyHtml = true;

        //            smtp.Send(mail);
        //        }

        //        ViewBag.Message = $"✅ Mail sent successfully to {model.Email}";

        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = $"❌ Failed to send mail: {ex.Message}";
        //    }

        //    return View("ManageVendors", model);
        //}

        [HttpPost]
        public IActionResult AddVendor([FromBody] VendorInput model)
        {
            if (string.IsNullOrWhiteSpace(model.VendorName) || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Vendor Name and Email are required." });
            }

            try
            {
                // Save vendor and get generated code
                string newVendorCode = _repo.AddVendorEmail(model);

                // Prepare activation link
                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string link = $"{baseUrl}/RegisterVendor/Activate?vendorCode={WebUtility.UrlEncode(newVendorCode)}";

                string body = $@"
            <html><body style='font-family:Segoe UI;'>
                <h3>Hello {model.VendorName},</h3>
                <p>Welcome! Please click below to access your vendor portal:</p>
                <a href='{link}' target='_blank'
                    style='background:#007bff;color:white;padding:10px 18px;border-radius:5px;text-decoration:none;'>
                    Open Vendor Portal
                </a>
                <br/><br/>Thank you,<br/><strong>Admin Team</strong>
            </body></html>";

                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress("sfil.supports@sapphirefoods.in", "Vendor OCR"),
                        Subject = "Welcome to Vendor Portal",
                        Body = body,
                        IsBodyHtml = true
                    };
                    mail.To.Add(model.Email);
                    smtp.Send(mail);
                }

                return Ok(new { message = $"Mail sent successfully to {model.Email} with Vendor Code {newVendorCode}" });
            }
            catch (Exception ex)
            {
                _repo.ErrorLog(ex.Message, VendorCode);
                return StatusCode(500, new { message = $"Failed to send mail" });
            }
        }



        // ✅ AJAX endpoint for validating email
        [HttpPost]
        public JsonResult ValidateEmail([FromBody] EmailRequest request)
        {
            string status = GetMailStatus(request.Email);
            return Json(new { status });
        }


        private string GetMailStatus(string email)
        {
            bool isInvalid;

            if (!IsValidSyntax(email))
                return "Invalid1"; // syntax invalid
            else if (VerifyEmailSmtp(email, out isInvalid))
                return "Valid";
            else if (isInvalid)
                return "Invalid2"; // 550-5.1.1 mailbox unavailable
            else if (!DomainHasMx(email.Split('@')[1]))
                return "Invalid3"; // MX record not found
            else
                return "Valid"; // assume valid if network timeout etc.
        }

        private bool IsValidSyntax(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch(Exception ex)
            {
                _repo.ErrorLog(ex.Message, VendorCode);
                return false;
            }
        }

        private bool DomainHasMx(string domain)
        {
            try
            {
                var lookup = new LookupClient();
                var result = lookup.Query(domain, QueryType.MX);
                return result.Answers.MxRecords().Any();
            }
            catch(Exception ex)
            {
                _repo.ErrorLog(ex.Message, VendorCode);
                return false;
            }
        }

        private bool VerifyEmailSmtp(string email, out bool isInvalid)
        {
            isInvalid = false;
            try
            {
                string domain = email.Split('@')[1];
                var lookup = new LookupClient();
                var result = lookup.Query(domain, QueryType.MX);
                var mxRecord = result.Answers.MxRecords().FirstOrDefault();

                if (mxRecord == null)
                {
                    isInvalid = true;
                    return false;
                }

                string mxHost = mxRecord.Exchange.Value;

                using (var tcpClient = new TcpClient())
                {
                    tcpClient.ReceiveTimeout = 10000;
                    tcpClient.SendTimeout = 10000;
                    tcpClient.Connect(mxHost, 25);

                    using (var stream = tcpClient.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;
                        ReadResponse(reader);
                        writer.WriteLine("HELO yourdomain.com");
                        ReadResponse(reader);
                        writer.WriteLine("MAIL FROM:<test@yourdomain.com>");
                        ReadResponse(reader);
                        writer.WriteLine($"RCPT TO:<{email}>");
                        string response = ReadResponse(reader);
                        writer.WriteLine("QUIT");

                        if (response.StartsWith("250"))
                            return true;
                        else if (response.Contains("550-5.1.1"))
                        {
                            isInvalid = true;
                            return false;
                        }
                        else
                            return false;
                    }
                }
            }
            catch(Exception ex)
            {
                _repo.ErrorLog(ex.Message, VendorCode);
                return false;
            }
        }

        private string ReadResponse(StreamReader reader)
        {
            return reader.ReadLine() ?? "";
        }

        [HttpGet]
        public IActionResult GetVendorGroups()
        {
            var vendorGroups = _repo.GetVendorGroups();
            return Json(vendorGroups);
        }

        [HttpGet]
        public IActionResult GetCategory()
        {
            var vendorGroups = _repo.GetCategory();
            return Json(vendorGroups);
        }

        [HttpGet]
        public IActionResult GetVendorDetailsByGroup(int vendorGroupId)
        {
            var details = _repo.GetVendorDetailsByGroup(vendorGroupId);
            return Json(details);
        }

        [HttpGet]
        public IActionResult GetVendorDetailsByCategory(int categoryId)
        {
            var details = _repo.GetVendorDetailsByCategory(categoryId);
            if (details == null)
                return Json(new { success = false });
            else
                return Json(new { success = true, data = details });
        }

        [HttpGet]        
        public IActionResult GetPaymentMethod()
        {
            var paymentGroups = _repo.GetPaymentMethod();
            return Json(paymentGroups);
        }

        [HttpGet]
        public IActionResult GetPayTerm()
        {
            var paymentGroups = _repo.GetPaymentTerms();
            return Json(paymentGroups);
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
    }
}
