using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Vendor_OCR.Models;
using Vendor_OCR.Repositories;

namespace Vendor_OCR.Controllers
{
    public class VendorListController : Controller
    {
        private readonly VendorRepository _vendorRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VendorListController(VendorRepository vendorRepo, IHttpContextAccessor httpContextAccessor)
        {
            _vendorRepo = vendorRepo;
            _httpContextAccessor = httpContextAccessor;
        }
        private string VendorCode =>
        _httpContextAccessor.HttpContext.Session.GetString("Vendor_Code");
        public IActionResult VendorList()
        {

            var vendors = _vendorRepo.GetVendors();
            return View(vendors);
        }


        [HttpPost]
        public IActionResult UpdateVendorEmail([FromBody] UpdateVendorEmailRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.VendorId) || string.IsNullOrWhiteSpace(request.Email))
                    return Json(new { status = "Error", message = "Invalid Input" });

                bool updated = _vendorRepo.UpdateVendorEmail(request.VendorId.Trim(), request.Email.Trim());

                return Json(new { status = updated ? "Success" : "Error" });
            }
            catch (Exception ex)
            {
                _vendorRepo.ErrorLog(ex.Message, VendorCode);
                return Json(new { status = "Error" });
            }
            
        }

        public class UpdateVendorEmailRequest
        {
            public string VendorId { get; set; }
            public string Email { get; set; }
        }


        [HttpPost]
        public IActionResult SendVendorMail([FromBody] SendMailRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request");

                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string link = $"{baseUrl}/RegisterVendor/Activate?vendorCode={WebUtility.UrlEncode(request.vendorCode)}";

                string body = $@"
                <html><body style='font-family:Segoe UI;'>
                    <h3>Hello {request.vendorName},</h3>
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
                    mail.To.Add(request.email);

                    smtp.Send(mail);
                }

                return Ok(new { status = "Success" });
            }
            catch (Exception ex) 
            {
                _vendorRepo.ErrorLog(ex.Message, VendorCode);
                return StatusCode(500, new { status = "Error"});
            }


        }



        public class SendMailRequest
        {
            public string vendorCode { get; set; }
            public string vendorName { get; set; }
            public string email { get; set; }
        }

    }
}
