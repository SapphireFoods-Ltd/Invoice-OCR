using System.ComponentModel.DataAnnotations;

namespace Vendor_OCR.Models
{
    public class AdminModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string VendorName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; }
    }

    public class VendorGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }

    }

    public class VendorDetails
    {
        public int VendorGroupId { get; set; }
        public int CategoryId { get; set; }
        public string Role { get; set; }
        public string PurchaseOrg { get; set; }
        public string SearchTerm { get; set; }
    }

    public class PaymentGroup
    {
        public string Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaytermStatus { get; set; }

    }

    public class VendorInput
    {
        public string VendorCode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string VendorName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string TermsOfPayment { get; set; }
        public string PaymentMethods { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public string BusinessArea { get; set; }
        public string VendorGroup { get; set; }
        public string CompanyCode { get; set; }
        public string Role { get; set; }
        public string PurOrg { get; set; }
        public string SearchTerm { get; set; }
        public string StoreName { get; set; }
        public string empcode { get; set; }
    }
}
