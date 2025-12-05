namespace Vendor_OCR.Models
{
 
    public class VendorInvoiceModel
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
