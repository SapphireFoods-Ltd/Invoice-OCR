using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Vendor_OCR.Models;
using Vendor_OCR.Repositories;

namespace Vendor_OCR.Controllers
{
    public class VendorInvoiceListController : Controller
    {
        private readonly VendorRepository _vendorRepository;

        public VendorInvoiceListController(IConfiguration configuration)
        {
            _vendorRepository = new VendorRepository(configuration);
        }


        public async Task<IActionResult> VendorInvoiceList()
        {
            var VendorCode = HttpContext.Session.GetString("Vendor_Code");

            var invoices = await _vendorRepository.GetVendorInvoicesAsync(VendorCode);

            // Count based on action/status values
            ViewBag.PendingCount = invoices.Count(x => x.action == null || x.action == "Pending");
            ViewBag.ApprovedCount = invoices.Count(x => x.action == "OCR Approved");
            ViewBag.RejectedCount = invoices.Count(x => x.action == "OCR Rejected");

            return View(invoices);
        }



    }
}