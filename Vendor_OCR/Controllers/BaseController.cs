using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Vendor_OCR.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        protected void LogErrorToDB(Exception ex, string pageName)
        {
            try
            {
                string connStr = _configuration.GetConnectionString("SqlConnectionString");
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SP_InsertErrorLog", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string errMsg = ex.Message;
                        if (ex.InnerException != null)
                            errMsg += " | Inner: " + ex.InnerException.Message;

                        string vendorCode = HttpContext.Session.GetString("Vendor_Code") ?? "System";
                        string userName = HttpContext.Session.GetString("Vendor_Name") ?? "Vendor";
                        string userMail = HttpContext.Session.GetString("UserEmail") ?? "Unknown";
                        string ins_dt = DateTime.Now.ToString("dd-MM-yyyy h:mm tt");

                        cmd.Parameters.AddWithValue("@err_log", errMsg);
                        cmd.Parameters.AddWithValue("@err_to", userName);
                        cmd.Parameters.AddWithValue("@user_type", '2');
                        cmd.Parameters.AddWithValue("@user_mail", userMail);
                        cmd.Parameters.AddWithValue("@pagename", pageName);
                        cmd.Parameters.AddWithValue("@ins_by", vendorCode);     
                        cmd.Parameters.AddWithValue("@ins_dt", ins_dt);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                
            }
        }


    }
}
