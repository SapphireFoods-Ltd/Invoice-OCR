using System;

namespace Vendor_OCR.Models
{
    public class HomeModel
    {
        // made nullable where DB might contain NULLs
        public int? OrderNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string ins_by { get; set; }
        public DateTime? ins_dt { get; set; }
        public string PrevStatus { get; set; }
        public string StatusText { get; set; }
        public string StatusClass { get; set; }
        public bool? IsSynthetic { get; set; }
        public string InvoiceName { get; set; }
        public string raw_Name { get; set; }
        public string S3_BucketPath { get; set; }

        public string Uploaded { get; set; }
        public string Approved { get; set; }
        public string OCR { get; set; }
        public string SAPPush { get; set; }


    }

    public class InvoiceGroup
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceName { get; set; }

        public string raw_Name { get; set; }
        public List<HomeModel> Stages { get; set; }
    }


    public class StageViewModel
    {
        public string Name { get; set; }            // "Uploaded", "Approved", "OCR", "SAPPush"
        public string Status { get; set; }          // "Done", "Pending", "Rejected", "ReUploaded"
    }

    public class InvoiceProgressViewModel
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceName { get; set; }
        public string RawName { get; set; }
        public DateTime? LatestDt { get; set; }     // latest ins_dt (optional display / sorting)
        public List<StageViewModel> Stages { get; set; } = new List<StageViewModel>();
    }

    public class InvoicePageViewModel
    {
        public List<InvoiceGroup> Invoices { get; set; } = new List<InvoiceGroup>();
        public List<HomeModel> StoreWise { get; set; } = new List<HomeModel>();

        public List<VendorInvoiceList> VendorInvoiceModel { get; set; } = new List<VendorInvoiceList>();
    }


    public class VendorInvoiceList
    {

        public string ID { get; set; }
        public string InvoiceName { get; set; }
        public string raw_Name { get; set; }
        public DateTime? ins_dt { get; set; }
        public string Category { get; set; }
        public string S3BucketLink { get; set; }
        public string action { get; set; }
    }
}
