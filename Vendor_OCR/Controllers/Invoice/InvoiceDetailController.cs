using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Vendor_OCR.Models.Invoice;
using Vendor_OCR.Repositories;

namespace Vendor_OCR.Controllers.Invoice
{
    public class InvoiceDetailController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _connectionString;
        private readonly VendorRepository _vendorRepo;
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;

        public InvoiceDetailController(IConfiguration config,IWebHostEnvironment env,VendorRepository vendorRepo, IAmazonS3 s3Client)
        {
            _env = env;
            _vendorRepo = vendorRepo;
            _connectionString = config.GetConnectionString("SqlConnectionString");
            _s3Client = s3Client;
        }

        public IActionResult InvoiceDetailList()
        {
            var invoices = _vendorRepo.GetInvoicesDetailList();
            return View("~/Views/Invoice/InvoiceDetailList.cshtml",invoices);
        }


        [HttpGet]
        public IActionResult InvoiceDetail(int Rid)
        {
            var invoice = _vendorRepo.GetInvoicesDetail(Rid);
            return View("~/Views/Invoice/InvoiceDetail.cshtml", invoice);
        }

        [HttpPost]
        public IActionResult UpdateInvoice(InvoiceDetail model, string actionType)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid data" });

            // Save or Submit
            model.actiontype = actionType == "submit" ? "submit" : "save";

            model.UpdBy = HttpContext.Session.GetString("Vendor_Code") ?? "0";
            model.UpdDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var result = _vendorRepo.UpdateInvoiceDetail(model);

            return Json(new
            {
                success = true,
                message = actionType == "submit"
                            ? "Invoice submitted successfully."
                            : "Invoice saved successfully."
            });
        }

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            string bucketName = "invoice-ocr-internal";
            string awsAccessKey = "";
            string awsSecretKey = "";
            string awsRegion = "ap-south-1";

            using var client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using var response = await client.GetObjectAsync(request);
            using var stream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream);

            return File(
                stream.ToArray(),
                response.Headers.ContentType,
                fileName
            );
        }

        public IActionResult PreviewFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest(new { message = "Invalid file name" });

                string bucketName = "invoice-ocr-internal";

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Verb = HttpVerb.GET

                };

                string url = _s3Client.GetPreSignedURL(request);

                return Json(new { previewUrl = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


    }
}
