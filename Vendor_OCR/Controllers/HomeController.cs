using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Vendor_OCR.Repositories;
using Vendor_OCR.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vendor_OCR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VendorRepository _vendorRepo;
        private readonly IAmazonS3 _s3;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly string _connectionString;

        public HomeController(
            ILogger<HomeController> logger,
            IAmazonS3 s3,
            IConfiguration config,
            IWebHostEnvironment env,
            VendorRepository vendorRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _vendorRepo = vendorRepo ?? throw new ArgumentNullException(nameof(vendorRepo));
            _connectionString = _config.GetConnectionString("SqlConnectionString");
        }


        public async Task<IActionResult> Index()
        {
            var sap_code = HttpContext.Session.GetString("sap_code");
            var utype = HttpContext.Session.GetString("user_type");
            var empcode = HttpContext.Session.GetString("Vendor_Code");
            if (string.IsNullOrEmpty(utype))
                return View("~/Views/No Access/NoAccess.cshtml");

            List<HomeModel> storeWise;
            List<VendorInvoiceList> VendorInvoiceModel;

            if (utype == "2")
            {
                storeWise = await _vendorRepo.GetInvoicesProcess_Vendorwise(empcode) ?? new List<HomeModel>();
            }
            else
            {
                 storeWise = await _vendorRepo.GetInvoicesProcess_storewise(sap_code) ?? new List<HomeModel>();

            }

            var VendorCode = HttpContext.Session.GetString("Vendor_Code");

            var invoices = await _vendorRepo.GetVendorInvoicesAsync(VendorCode) ?? new List<VendorInvoiceList>();

            ViewBag.PendingCount = invoices.Count(x => x.action == null || x.action == "Pending");
            ViewBag.ApprovedCount = invoices.Count(x => x.action == "OCR Approved");
            ViewBag.RejectedCount = invoices.Count(x => x.action == "OCR Rejected");

          
            var vm = new InvoicePageViewModel
            {

               VendorInvoiceModel = invoices,
               StoreWise = storeWise
            };

            return View(vm);

        }
     
    }


}