using Microsoft.AspNetCore.Mvc;
using Vendor_OCR.Services;

namespace Vendor_OCR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SapExportController : Controller
    {
        private readonly VendorExportService _exportService;
        private readonly ILogger<SapExportController> _logger;

        public SapExportController(VendorExportService exportService, ILogger<SapExportController> logger)
        {
            _exportService = exportService;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/export/run?mode=missing_sap&outputFolder=C:\Exports
        /// No secret key (ensure endpoint access is restricted or use localhost from Task Scheduler)
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromQuery] string mode = "missing_sap", [FromQuery] string? outputFolder = null)
        {
            try
            {
                var path = await _exportService.ExportHashCsvAsync(mode, outputFolder);
                _logger.LogInformation("Export completed. File: {File}", path);
                return Ok(new { success = true, path });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export failed for mode {Mode}", mode);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
