using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Vendor_OCR.Repositories;
using Vendor_OCR.Models;
using static Vendor_OCR.Models.VendorRegisterModel;

namespace Vendor_OCR.Controllers
{
  
    public class RegisterVendorController : Controller
    {
        private readonly ILogger<RegisterVendorController> _logger;
        private readonly VendorRepository _repo;
        string empCode;

        public RegisterVendorController(IConfiguration config, ILogger<RegisterVendorController> logger)
        {
            _repo = new VendorRepository(config);
            _logger = logger;
        }
        public IActionResult RegisterVendor(string vendorEmail, string name)
        {
            
            var model = new VendorRegisterModel
            {
                Name = name,
                Email = vendorEmail
            };
            model.BankAccounts.Add(new VendorRegisterModel.BankAccount());
            return View(model);

        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Activate(string vendorCode)
        {
            try
            {
                if (string.IsNullOrEmpty(vendorCode))
                    return View("Error", "Invalid activation link.");

                string validationResult = _repo.ValidateVendorCode(vendorCode);
                HttpContext.Session.SetString("EmpCode", vendorCode);

                switch (validationResult)
                {
                    case "Invalid":
                        return View("Error", "Invalid or expired activation link.");

                    case "AlreadyActivated":
                        return RedirectToAction("Login", "Login");

                    case "Valid":
                        var vendor = _repo.GetVendorRequestDetails(vendorCode);

                        if (vendor == null)
                            return RedirectToAction("Login", "Login");

                        var model = new VendorRegisterModel
                        {
                            Name = vendor.Name,
                            Email = vendor.Email
                        };

                        return View("~/Views/Vendor/RegisterVendor.cshtml", model);

                    default:
                        return View("~/Views/Error/Error.cshtml");
                }
            }
            catch (Exception ex)
            {
                empCode = HttpContext.Session.GetString("EmpCode");
                _repo.ErrorLog(ex.Message, empCode);

                return View("~/Views/Error/Error.cshtml");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SendOtpToEmail([FromBody] OtpRequest data)
        {
            string email = data.Email;
            string otp = new Random().Next(100000, 999999).ToString();

            try
            {
                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
                    //smtp.EnableSsl = true;

                    //var mail = new MailMessage();
                    //mail.From = new MailAddress("sfil.supports@sapphirefoods.in", " Sapphire Foods Vendor Portal");
                    //mail.To.Add(email);
                    //mail.Subject = "Your OTP Code";
                    //mail.Body = $"Your OTP for registration is: <b>{otp}</b>";
                    //mail.IsBodyHtml = true;
                    //smtp.Send(mail);

                    smtp.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress("sfil.supports@sapphirefoods.in", "Sapphire Foods Vendor Registration Portal");
                    mail.To.Add(email);
                    mail.Subject = "Vendor Registration Portal";

                    string htmlTemplate = @"
<table width='100%' cellpadding='0' cellspacing='0' role='presentation' style='font-family: Arial, Helvetica, sans-serif; background:#f3f5f7; padding:20px 0;'>
  <tr><td align='center'>
    <table width='600' cellpadding='0' cellspacing='0' role='presentation' style='background:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 6px rgba(0,0,0,0.08);'>
      <tr><td style='padding:18px 24px; background:#0d6efd; color:#ffffff;'>
        <table width='100%'><tr>
          <td style='font-size:18px; font-weight:600;'>Sapphire Foods</td>
          <td align='right' style='font-size:12px; opacity:0.95;'>Your OTP Code</td>
        </tr></table>
      </td></tr>

      <tr><td style='padding:28px 32px; color:#333333;'>
        <p style='margin:0 0 12px 0; font-size:15px;'>Hello,</p>
        <p style='margin:0 0 18px 0; font-size:14px; color:#555;'>Your OTP for registration is:</p>

        <div style='text-align:center; margin:18px 0;'>
          <div style='display:inline-block; padding:18px 26px; border-radius:6px; background:#f0f7ff; border:1px solid #dbeeff;'>
            <span style='font-size:28px; font-weight:700; letter-spacing:2px; color:#0d6efd;'>{0}</span>
          </div>
        </div>

        <p style='margin:14px 0 6px 0; font-size:13px; color:#666;'>This OTP is valid for <strong>10 minutes</strong>. Do not share this code with anyone.</p>

        <p style='margin:16px 0 0 0; font-size:13px; color:#666;'>Thanks,<br/><strong>Sapphire Foods</strong></p>
      </td></tr>

      <tr><td style='padding:14px 18px; background:#fafafa; border-top:1px solid #eee; font-size:11px; color:#777;'>
        <div style='line-height:1.35;'><strong>Confidentiality:</strong> The information contained in this email is privileged and confidential. If you are not the intended recipient, please notify the sender and delete this message.</div>
      </td></tr>
    </table>

    <div style='max-width:600px; font-size:11px; color:#9aa0a6; text-align:center; padding:10px 0;'>© Sapphire Foods India Limited</div>
  </td></tr>
</table>
";

                    // insert OTP into template
                    string bodyHtml = string.Format(htmlTemplate, otp);

                    // set body and headers
                    mail.Body = bodyHtml;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;

                    // optional: provide a plain-text alternative
                    string plainText = $"Your OTP for registration is: {otp}. This OTP is valid for 10 minutes.";
                    var plainView = AlternateView.CreateAlternateViewFromString(plainText, System.Text.Encoding.UTF8, "text/plain");
                    var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, System.Text.Encoding.UTF8, "text/html");
                    mail.AlternateViews.Add(plainView);
                    mail.AlternateViews.Add(htmlView);

                    // send
                    smtp.Send(mail);
                }

                return Json(new { status = "Success", otp = otp });
            }
            catch (Exception ex)
            {
                empCode = HttpContext.Session.GetString("EmpCode");
                _repo.ErrorLog(ex.Message, empCode);
                return Json(new { status = "Error", message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RegisterVendor(VendorRegisterModel model)
        {
            
            return Json(new { status = "Success", message = "Vendor registered successfully!" });
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveVendor([FromBody] VendorPassword vendor)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Error/Error.cshtml");

            bool isSaved = await _repo.SaveVendorAsync(vendor);
            if (isSaved)
            {
                return Ok(new
                {
                    success = true,
                    message = "Vendor saved successfully!",
                    redirect = Url.Action("Login", "Login")
                });
            }

            return View("~/Views/Error/Error.cshtml");
        }


    }

    public class OtpRequest
    {
        public string Email { get; set; }
    }
}
