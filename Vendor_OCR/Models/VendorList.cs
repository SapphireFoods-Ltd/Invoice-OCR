namespace Vendor_OCR.Models
{
    public class VendorList
    {
        public int Vendor_Code { get; set; }
        public string Vendor_Name { get; set; }
        public string Vendor_Email { get; set; }
        public int? Flag { get; set; }
        public DateTime? ins_dt { get; set; }
    }

}
