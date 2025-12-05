using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vendor_OCR.Models
{


    public class RegisterModel
    {
        // Business Details (Readonly)


        public string? OCR_AadharNo { get; set; }
        public string? OCR_PanNo { get; set; }
        public string? StoreName { get; set; }
        public string? Category { get; set; }
        public string? BusinessArea { get; set; }
        public string? VendorGroup { get; set; }
        public string? CompanyCode { get; set; }
        public string? Role { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? SearchTerm { get; set; }
        public string? PaymentTerm { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Currency { get; set; }

        // Vendor Details
        [RegularExpression(@"^[A-Za-z0-9'()-.& ]{4,}$", ErrorMessage = "Invalid Company Name.")]
        [Required(ErrorMessage = "Company's Name is required.")]
        public string CompanyName { get; set; }

        public string? ProprietorName { get; set; }
        public string? PartnershipName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Mobile Number.")]
        [Required(ErrorMessage = "Mobile No. is required.")]
        public string MobileNumber { get; set; }

        [RegularExpression(@"^[\w+-.%]+@[\w.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email format.")]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        // Address
        [MaxLength(225)]
        [RegularExpression(@"^[\w. ,'&~/\"":;-]*$", ErrorMessage = "Invalid characters in Building.")]
        [Required(ErrorMessage = "Building is required.")]
        public string Building { get; set; }

        [RegularExpression(@"^[\w. ,'&~/\"":;-]*$", ErrorMessage = "Invalid Street 1.")]
        [Required(ErrorMessage = "Street 1 is required.")]
        public string Street1 { get; set; }

        public string? Street2 { get; set; }

        [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Only alphabets allowed in city.")]
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid PIN format.")]
        [Required(ErrorMessage = "Postal Code is required.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; }

        // Bank Accounts
        public List<BankAccount> BankAccounts { get; set; } = new() { new BankAccount() };

        public class BankAccount
        {
            [RegularExpression(@"^[A-Za-z]{4}[A-Za-z0-9]{7}$", ErrorMessage = "Invalid IFSC format.")]
            [Required(ErrorMessage = "IFSC is required.")]
            public string IFSC { get; set; }

            [RegularExpression(@"^[0-9]{2,20}$", ErrorMessage = "Invalid Account Number.")]
            [Required(ErrorMessage = "Account No. is required.")]
            public string AccountNumber { get; set; }

            [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Invalid Account Holder Name.")]
            [Required(ErrorMessage = "Account Holder Name is required.")]
            public string AccountHolderName { get; set; }

            [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Invalid Bank Name.")]
            [Required(ErrorMessage = "Bank Name is required.")]
            public string BankName { get; set; }
        }

        // Documents Details
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", ErrorMessage = "Invalid GST format.")]
        [Required(ErrorMessage = "GST No. is required.")]
        public string GstNo { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format.")]
        [Required(ErrorMessage = "PAN No. is required.")]
        public string PanNo { get; set; }

        
        [RegularExpression(@"^[2-9][0-9]{11}$", ErrorMessage = "Invalid Aadhar Number.")]
        [Required(ErrorMessage = "Aadhar No. is required.")]
        public string AadharNo { get; set; }

        public bool IsMSME { get; set; }
        public string? MSMENo { get; set; }

        [Required(ErrorMessage = "Nature of Expense is required.")]
        public string NatureOfExpense { get; set; }

        public bool HasLowerTaxDeduction { get; set; }
        public string? LowerTaxCertNo { get; set; }

        [Required(ErrorMessage = "Reconciliation Account is required.")]
        public string ReconciliationAccount { get; set; }

        [RegularExpression(@"^([LUu]{1})([0-9]{5})([A-Za-z]{2})([0-9]{4})([A-Za-z]{3})([0-9]{6})$|^([A-Za-z]{3})-([0-9]{4})$",
            ErrorMessage = "Invalid CIN/LLPIN format.")]
        public string? CinLlpin { get; set; }

       
        // Other Details
        [MaxLength(225)]
        [Display(Name = "Remarks/Comments")]
        public string? Remarks { get; set; }


        public IFormFile? AadharFile { get; set; }
        public IFormFile? PanFile { get; set; }
        public IFormFile? BankStatementFile { get; set; }
        public IFormFile? ContractFile { get; set; }
        public IFormFile? GstCertificateFile { get; set; }
        public IFormFile? MsmeCertificateFile { get; set; }
        public IFormFile? LowerTaxCertificateFile { get; set; }
        public IFormFile? CertificateOfIncorporationFile { get; set; }

        public string? ExistAadharFile { get; set; }
        public string? ExistPanFile { get; set; }
        public string? ExistBankStatementFile { get; set; }
        public string? ExistContractFile { get; set; }
        public string? ExistGstCertificateFile { get; set; }
        public string? ExistMsmeCertificateFile { get; set; }
        public string? ExistLowerTaxCertificateFile { get; set; }
        public string? ExistCertificateOfIncorporationFile { get; set; }
    }

}

