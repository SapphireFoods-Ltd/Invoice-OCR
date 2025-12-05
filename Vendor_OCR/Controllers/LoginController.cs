using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Claims;
using Vendor_OCR.Models;

namespace Vendor_OCR.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");

            }

            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            // Validate only login fields
            ModelState.Remove(nameof(model.OldPassword));
            ModelState.Remove(nameof(model.NewPassword));
            ModelState.Remove(nameof(model.ConfirmPassword));

            if (!ModelState.IsValid)
                return View(model);

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SP_Validate_Vendor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "Validateuser");
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        string vendorCode = reader["Vendor_Code"]?.ToString() ?? "";
                        string vendorName = reader["Vendor_Name"]?.ToString() ?? "";
                        string userType = reader["usertype"]?.ToString() ?? "";
                        string flag = reader["flag"]?.ToString() ?? "";

                        // ✅ Set session values (your current logic)
                        HttpContext.Session.SetString("UserEmail", model.Email);
                        HttpContext.Session.SetString("Vendor_Code", vendorCode);
                        HttpContext.Session.SetString("Vendor_Name", vendorName);
                        HttpContext.Session.SetString("user_type", userType);
                        HttpContext.Session.SetString("Flag", flag);

                        // ✅ NEW: Add cookie authentication so [Authorize] works
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim("UserType", userType)
                };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);



                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid email or password.");
                        return View(model);
                    }
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }


        [HttpPost]
        public IActionResult ResetPassword(LoginModel model)
        {

            ModelState.Remove(nameof(model.Password));

            if (!ModelState.IsValid)
            {
                ViewBag.ShowResetModal = true;
                return View("Login", model);
            }

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SP_Validate_Vendor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "UpdatePassword");
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@OldPassword", model.OldPassword);
                cmd.Parameters.AddWithValue("@NewPassword", model.NewPassword);
                conn.Open();
                var result = cmd.ExecuteScalar();

                if (result != null && Convert.ToInt32(result) == 1)
                {
                    TempData["SuccessMessage"] = "Password updated successfully. Please log in.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("OldPassword", "Old password is incorrect.");
                    ViewBag.ShowResetModal = true;
                    return View("Login", model);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user_type = HttpContext.Session.GetString("user_type");

            // Common sign-out logic
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            if (user_type == "2")
            {
                return RedirectToAction("Login", "Login");

                
            }
            else
            {

                return View("~/Views/No Access/NoAccess.cshtml");
            }
        }

    }
}
