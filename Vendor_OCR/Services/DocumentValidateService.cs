
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using Vendor_OCR.Repositories;

namespace Vendor_OCR.Services
{
    public class DocumentValidateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DocumentValidateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ValidateDocuments();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // runs every 5 minutes
            }
        }

        private async Task ValidateDocuments()
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<VendorRepository>();

            var docs = await repo.GetDocumentsToValidate();

            foreach (var doc in docs)
            {
                List<string> failures = new List<string>();

                string name = (doc.CustomerName ?? "").ToUpper();

                if (!name.Contains("SAPPHIRE FOODS INDIA"))
                {
                    failures.Add($"Invalid Customer Name: {doc.CustomerName}");
                }

                if (string.IsNullOrEmpty(doc.PurchaseOrder))
                {
                    failures.Add("Purchase Order is missing");
                }
                
                // Send email only if one or more validations failed
                if (failures.Count > 0)
                {
                    string failedList = string.Join("\n", failures.Select(f => $"• {f}"));

                    await repo.rejectOCRInvoice(doc.uploadInvoiceId,failedList);

                    string body = $@"
<html>
<body style='font-family: Segoe UI; font-size:14px; color:#333;'>

<p>Dear {doc.uploaderName},</p>

<p>Your invoice <strong>(ID: {doc.InvoiceId})</strong>, uploaded on <strong>{doc.uploadedon}</strong>, has failed system validation and has been rejected</p>
<p>Reason(s) for Validation Failure:</P>

<ul>
    {string.Join("", failures.Select(f => $"<li>{f}</li>"))}
</ul>

<p>Please review the validation failure(s), correct the invoice, and re-upload it through the Sapphire Invoice Portal for reprocessing.</p>

<p>Regards,<br>
Sapphire Invoice Management System</p>

</body>
</html>";

                    try
                    {
                        using (var smtp = new SmtpClient("smtp.office365.com", 587))
                        {
                            smtp.Credentials = new NetworkCredential("sfil.supports@sapphirefoods.in", "khgfWcQ@42*4!&1212");
                            smtp.EnableSsl = true;

                            var mail = new MailMessage
                            {
                                From = new MailAddress("sfil.supports@sapphirefoods.in", "Vendor OCR"),
                                Subject = $"Invoice Validation Failure Notification  - {doc.InvoiceId}",
                                Body = body,
                                IsBodyHtml = true
                            };

                            mail.To.Add(doc.emailid);

                            smtp.Send(mail);
                        }
                    }
                    catch (Exception ex)
                    {
                    }


                }


                await repo.MarkAsValidated(doc.Id);
            }
        }

    }

}
