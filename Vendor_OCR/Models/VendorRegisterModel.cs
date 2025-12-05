using System.ComponentModel.DataAnnotations;

namespace Vendor_OCR.Models
{
    public class VendorRegisterModel
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public class VendorPassword
        {
            public string Password { get; set; }
            public string VendorCode { get; set; }
        }
        

        [Required]
        public List<BankAccount> BankAccounts { get; set; } = new()
        {
            new BankAccount() // Default one row
        };
        public class BankAccount
        {
            [Required(ErrorMessage = "IFSC is required")]
            [RegularExpression(@"^[A-Za-z]{4}[a-zA-Z0-9]{7}$", ErrorMessage = "Invalid IFSC format")]
            public string IFSC { get; set; }

            [Required(ErrorMessage = "A/C No. is required")]
            [RegularExpression(@"^[a-zA-Z0-9]{2,20}$", ErrorMessage = "Invalid Account Number")]
            public string AccountNumber { get; set; }

            [Required(ErrorMessage = "A/C Name is required")]
            [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Invalid Account Holder Name")]
            public string AccountHolderName { get; set; }

            [Required(ErrorMessage = "Bank Name is required")]
            [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Invalid Bank Name")]
            public string BankName { get; set; }
        }

        //public class VendorGroup
        //{
        //    public int Id { get; set; }
        //    public string GroupName { get; set; }
        //    public string Category { get; set; }
            
        //}

        //public class VendorDetails
        //{
        //    public int VendorGroupId { get; set; }
        //    public int CategoryId { get; set; }
        //    public string Role { get; set; }
        //    public string PurchaseOrg { get; set; }
        //    public string SearchTerm { get; set; }
        //}

        //public class PaymentGroup
        //{
        //    public string Id { get; set; }
        //    public string PaymentMethod { get; set; }
        //    public string PaytermStatus { get; set; }

        //}

        //public class VendorInput
        //{
        //    public string VendorCode { get; set; }
        //    public string Name { get; set; }
        //    public string Email { get; set; }
        //    public string Password { get; set; }
        //    public string TermsOfPayment { get; set; }
        //    public string PaymentMethods { get; set; }
        //    public string Currency { get; set; }
        //    public string Category { get; set; }
        //    public string BusinessArea { get; set; }
        //    public string VendorGroup { get; set; }
        //    public string CompanyCode { get; set; }
        //    public string Role { get; set; }
        //    public string PurOrg { get; set; }
        //    public string SearchTerm { get; set; }
        //    public string StoreName { get; set; }
        //}

    }


}
