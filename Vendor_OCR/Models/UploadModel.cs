using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vendor_OCR.Models
{
    public class UploadModel
    {
        [Required(ErrorMessage = "Please select at least one file.")]
        public List<IFormFile> Files { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        public int? InvoiceId { get; set; }   
        public string? ExistingFileName { get; set; }

        public string? RejectionRemark { get; set; }
        public string Mode { get; set; } = "insert";
    }
}
