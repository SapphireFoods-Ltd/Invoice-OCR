using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vendor_OCR.Repositories;  

namespace Vendor_OCR.Services
{


    public class MailService : BackgroundService   
    {
        private readonly ILogger<MailService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _smtpHost = "smtp.office365.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "sfil.supports@sapphirefoods.in";
        private readonly string _smtpPass = "khgfWcQ@42*4!&1212";
        private readonly string _sendTo = "rajan@bixware.com";


        public MailService(ILogger<MailService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddHours(21); // 21 = 9 PM (Night)

                    if (now > nextRun)
                        nextRun = nextRun.AddDays(1);

                    var delay = nextRun - now;

                    _logger.LogInformation($"Next mail will run at: {nextRun}");

                    await Task.Delay(delay, stoppingToken); // Wait until 9 PM

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    _logger.LogInformation($"Daily mail triggered at: {DateTime.Now}");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<VendorRepository>();
                        var dt = repo.GetDataFromStoredProcedure();

                        if (dt.Rows.Count > 0)
                            SendInvoiceMail(dt);
                        else
                            SendMail("Daily Invoice Summary", "<b>No invoices uploaded today.</b>");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending email");
                    // ❗ No retry wait — program continues normally to next day
                }
            }
        }


        private void SendInvoiceMail(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<h3>Daily Invoice Summary (" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ")</h3>");
            sb.Append("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");
            sb.Append("<tr style='background:#d9d9d9;font-weight:bold'><td>Category</td><td>Sub Category</td><td>Total</td></tr>");

            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + row["Category"] + "</td>");
                sb.Append("<td>" + row["Sub_category"] + "</td>");
                sb.Append("<td>" + row["TodayInvoiceCount"] + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            SendMail("Daily Invoice Summary", sb.ToString());
        }

        // SMTP Mail Sender
        private void SendMail(string subject, string body)
        {
            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_smtpUser,"Do not Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(_sendTo);
            smtp.Send(mail);
        }
    }

}
