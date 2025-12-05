namespace Vendor_OCR.Models.Invoice
{
    public class InvoiceSubDetail
    {
        public int Rid { get; set; }
        public int InvoiceDetailsId { get; set; }
        public string? InvoiceId { get; set; }
        public string? ProductCode { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public string? Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? Amount { get; set; }
        public string? CGST_PER { get; set; }
        public string? CGST_VAL { get; set; }
        public string? SGST_PER { get; set; }
        public string? SGST_VAL { get; set; }
        public string? IGST_PER { get; set; }
        public string? IGST_VAL { get; set; }
        public string? Taxable_Value { get; set; }

    }
}
