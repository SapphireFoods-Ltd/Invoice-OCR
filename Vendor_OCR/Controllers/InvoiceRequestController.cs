using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Text;
using Vendor_OCR.Repositories;
using Vendor_OCR.Services;

namespace Vendor_OCR.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceRequestController : ControllerBase
    {
        private readonly ILogger<InvoiceRequestController> _logger;
        private readonly VendorRepository _repo;
        private readonly IUserService _userService;

        public InvoiceRequestController(IConfiguration config, ILogger<InvoiceRequestController> logger, IUserService userService)
        {
            _repo = new VendorRepository(config);
            _logger = logger;
            _userService = userService;
        }
        [HttpPost("receive")]
        public async Task<IActionResult> ProcessAndInsertInvoiceAsync()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    return Unauthorized("Authorization header is required");
                }

                var authHeader = Request.Headers["Authorization"].ToString();
                if (!authHeader.StartsWith("Basic "))
                {
                    return Unauthorized("Basic authentication required");
                }

                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var credentials = decodedCredentials.Split(':', 2);

                if (credentials.Length != 2)
                {
                    return Unauthorized("Invalid authentication format");
                }

                var userId = credentials[0];
                var password = credentials[1];

                if (!await _userService.AuthenticateUserAsync(userId, password))
                {
                    return Unauthorized("Invalid user ID or password");
                }

                string rawJson;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    rawJson = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                    return BadRequest("Request body cannot be empty");

                _logger.LogDebug("Raw invoice JSON: {RawJson}", rawJson);

                InvoiceRequest request;
                try
                {
                    request = JsonConvert.DeserializeObject<InvoiceRequest>(rawJson);
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "Failed to deserialize invoice JSON");
                    _repo.ErrorLog("Failed to deserialize invoice JSON", "100100");
                    return BadRequest("Invalid JSON: " + jex.Message);
                }

                if (request == null)
                {
                    return BadRequest("Request body cannot be null");
                }

                IncomingInvoiceDto header = new IncomingInvoiceDto
                {
                    InvoiceNumber = request.InvoiceNumber,
                    InvoiceDate = request.InvoiceDate,
                    InvoiceTotal = request.InvoiceTotal,
                    InvoiceSubTotal = request.InvoiceSubTotal,
                    CustomerName = request.CustomerName,
                    CustomerBillToAddress = request.CustomerBillToAddress,
                    CustomerShipToAddress = request.CustomerShipToAddress,
                    CustomerOrderNumber = request.CustomerOrderNumber,
                    VendorName = request.VendorName,
                    VendorAddress = request.VendorAddress,
                    CustomerGSTNumber = request.CustomerGSTNumber,
                    VendorPANNumber = request.VendorPANNumber,
                    CustomerOrderDate = request.CustomerOrderDate,
                    InvoiceAcknowledgementNumber = request.InvoiceAcknowledgementNumber,
                    InvoiceIRNNumber = request.InvoiceIRNNumber,
                    InvoiceAcknowledgementDate = request.InvoiceAcknowledgementDate,
                    InvoiceTaxes = request.InvoiceTaxes,
                    VendorGSTNumber = request.VendorGSTNumber,
                    InvoiceCGSTValue = request.InvoiceCGSTValue,
                    InvoiceSGSTValue = request.InvoiceSGSTValue,
                    InvoiceIGSTValue = request.InvoiceIGSTValue,
                    ConfidenceLevelScore = request.ConfidenceLevelScore,
                    LineItems = request.LineItems?.Select(item => new IncomingLineItemDto
                    {
                        ItemDescription = item.ItemDescription,
                        ItemDiscountAmount = item.ItemDiscountAmount,
                        ItemHsnCode = item.ItemHsnCode,
                        ItemNetTotal = item.ItemNetTotal,
                        ItemQuantity = item.ItemQuantity,
                        ItemSubTotal = item.ItemSubTotal,
                        ItemUnitOfMeasure = item.ItemUnitOfMeasure,
                        ItemUnitPrice = item.ItemUnitPrice,
                        ItemCode = item.ItemCode,
                        ItemTax = item.ItemTax,
                        ItemNetPrice = item.ItemNetPrice,
                        ItemCGSTPercentage = item.ItemCGSTPercentage,
                        ItemSGSTPercentage = item.ItemSGSTPercentage,
                        ItemCGSTValue = item.ItemCGSTValue,
                        ItemSGSTValue = item.ItemSGSTValue,
                        ItemDiscountPercentage = item.ItemDiscountPercentage,
                        ItemTaxPercentage = item.ItemTaxPercentage,
                        ItemIGSTValue = item.ItemIGSTValue,
                        ItemIGSTPercentage = item.ItemIGSTPercentage
                    }).ToList() ?? new List<IncomingLineItemDto>()
                };

                _repo.SaveInvoiceFromVendor(header, rawJson);

                return Ok(new { message = "Invoice processed successfully", invoiceNumber = header.InvoiceNumber });
            }
            catch (Exception ex)
            {
                _repo.ErrorLog(ex.Message, "100100");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }


    public class InvoiceRequest
    {
        public string? InvoiceNumber { get; set; }
        public string? InvoiceDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public decimal? InvoiceSubTotal { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerBillToAddress { get; set; }
        public string? CustomerShipToAddress { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? CustomerGSTNumber { get; set; }
        public string? VendorPANNumber { get; set; }
        public string? CustomerOrderDate { get; set; }
        public string? InvoiceAcknowledgementNumber { get; set; }
        public string? InvoiceIRNNumber { get; set; }
        public string? InvoiceAcknowledgementDate { get; set; }
        public object? InvoiceTaxes { get; set; }
        public string? VendorGSTNumber { get; set; }
        public decimal? InvoiceCGSTValue { get; set; }
        public decimal? InvoiceSGSTValue { get; set; }
        public decimal? InvoiceIGSTValue { get; set; }
        public List<LineItemRequest>? LineItems { get; set; }
        public int? ConfidenceLevelScore { get; set; }
    }

    public class LineItemRequest
    {
        public string? ItemDescription { get; set; }
        public decimal? ItemDiscountAmount { get; set; }
        public string? ItemHsnCode { get; set; }
        public decimal? ItemNetTotal { get; set; }
        public decimal? ItemQuantity { get; set; }
        public decimal? ItemSubTotal { get; set; }
        public string? ItemUnitOfMeasure { get; set; }
        public decimal? ItemUnitPrice { get; set; }
        public string? ItemCode { get; set; }
        public decimal? ItemTax { get; set; }
        public decimal? ItemNetPrice { get; set; }
        public decimal? ItemCGSTPercentage { get; set; }
        public decimal? ItemSGSTPercentage { get; set; }
        public decimal? ItemCGSTValue { get; set; }
        public decimal? ItemSGSTValue { get; set; }
        public decimal? ItemDiscountPercentage { get; set; }
        public decimal? ItemTaxPercentage { get; set; }
        public decimal? ItemIGSTValue { get; set; }
        public decimal? ItemIGSTPercentage { get; set; }
    }

    public class IncomingInvoiceDto
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public decimal? InvoiceSubTotal { get; set; }
        public string CustomerName { get; set; }
        public string CustomerBillToAddress { get; set; }
        public string CustomerShipToAddress { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string VendorName { get; set; }
        public string VendorAddress { get; set; }
        public string CustomerGSTNumber { get; set; }
        public string VendorPANNumber { get; set; }
        public string CustomerOrderDate { get; set; }
        public string InvoiceAcknowledgementNumber { get; set; }
        public string InvoiceIRNNumber { get; set; }
        public string InvoiceAcknowledgementDate { get; set; }
        public object InvoiceTaxes { get; set; }
        public string VendorGSTNumber { get; set; }
        public decimal? InvoiceCGSTValue { get; set; }
        public decimal? InvoiceSGSTValue { get; set; }
        public decimal? InvoiceIGSTValue { get; set; }
        public List<IncomingLineItemDto> LineItems { get; set; }
        public int? ConfidenceLevelScore { get; set; }
    }

    public class IncomingLineItemDto
    {
        public string ItemDescription { get; set; }
        public decimal? ItemDiscountAmount { get; set; }
        public string ItemHsnCode { get; set; }
        public decimal? ItemNetTotal { get; set; }
        public decimal? ItemQuantity { get; set; }
        public decimal? ItemSubTotal { get; set; }
        public string ItemUnitOfMeasure { get; set; }
        public decimal? ItemUnitPrice { get; set; }
        public string ItemCode { get; set; }
        public decimal? ItemTax { get; set; }
        public decimal? ItemNetPrice { get; set; }
        public decimal? ItemCGSTPercentage { get; set; }
        public decimal? ItemSGSTPercentage { get; set; }
        public decimal? ItemCGSTValue { get; set; }
        public decimal? ItemSGSTValue { get; set; }
        public decimal? ItemDiscountPercentage { get; set; }
        public decimal? ItemTaxPercentage { get; set; }
        public decimal? ItemIGSTValue { get; set; }
        public decimal? ItemIGSTPercentage { get; set; }
    }
}