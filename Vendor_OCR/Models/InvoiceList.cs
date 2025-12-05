namespace Vendor_OCR.Models
{
    public class InvoiceList
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceName { get; set; }
        public string ins_Name { get; set; }
        public string Remark { get; set; }
        // Checkbox flag
        public bool IsSelected { get; set; }
        public string Status { get; set; }

        public string RejectRemark { get; set; }

        public string raw_Name { get; set; }

        public string S3BucketLink { get; set; }

    }
}
