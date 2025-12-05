namespace Vendor_OCR.Models.Invoice
{
    public class InvoiceDetail
    {
        public int Rid { get; set; }
        public string? InvoiceId { get; set; }
        public string? InvoiceType { get; set; }

        public string? SupplierGSTN { get; set; }
        public string? InvoiceDate { get; set; }
        public string? PurchaseOrder { get; set; }
        public string? DueDate { get; set; }

        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }

        public string? SubTotal { get; set; }
        public string? TotalTax { get; set; }
        public string? InvoiceTotal { get; set; }

        public string? Currency { get; set; }
        public string? GRNNumber { get; set; }
        public string? SESNumber { get; set; }

        public string? SupplierEmail { get; set; }
        public string? SupplierVATNumber { get; set; }
        public string? SupplierCINNumber { get; set; }

        public string? CustomerGSTN { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerVAT { get; set; }

        public string? CGST_P { get; set; }
        public string? CGST_Value { get; set; }
        public string? SGST_P { get; set; }
        public string? SGST_Value { get; set; }
        public string? IGST_P { get; set; }
        public string? IGST_Value { get; set; }
        public string? VAT_P { get; set; }
        public string? VAT_Value { get; set; }
               
        public string? Freight { get; set; }
        public string? Insurance { get; set; }
        public string? Package { get; set; }
        public string? LoadingCharges { get; set; }

        public string? CustomerNo { get; set; }
        public string? PanNumber { get; set; }

        public string? InsDt { get; set; }
        public string? InsBy { get; set; }
        public string? UpdDt { get; set; }
        public string? UpdBy { get; set; }

        public string? FileName { get; set; }

        public string? actiontype { get; set; }
        public List<InvoiceSubDetail> SubItems { get; set; } = new List<InvoiceSubDetail>();

    }
}
