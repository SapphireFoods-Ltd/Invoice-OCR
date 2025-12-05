using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Vendor_OCR.Repositories;

namespace Vendor_OCR.Services
{

    public class DocumentProcessingService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly VendorRepository _repo;
        private readonly IAmazonS3 _s3;

        public DocumentProcessingService(IConfiguration config, IAmazonS3 s3)
        {
            _s3 = s3;
            _config = config;
            _repo = new VendorRepository(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromHours(1);

            while (!stoppingToken.IsCancellationRequested)
            {
                //await ProcessDocuments();
                //SendInvoiceToSapAsync("3012628055");
                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task ProcessDocuments()
        {
            try
            {
                Console.WriteLine("---- Scheduler Started: Fetch + OCR ----");

                

                // AWS S3 Client
                var s3 = new AmazonS3Client(
                    _config["AWS:AccessKey"],
                    _config["AWS:SecretKey"],
                    Amazon.RegionEndpoint.GetBySystemName(_config["AWS:Region"])
                );

                var bucket = _config["AWS:OtherBucket"];


                var listResponse = await s3.ListObjectsAsync(bucket);

                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                //SendInvoiceToSapAsync("L-INV/2526/02614");

                var recentFiles = listResponse.S3Objects
                    .Where(obj => obj.LastModified >= oneHourAgo)
                    .ToList();

                if (recentFiles.Count == 0)
                {
                    Console.WriteLine("No new files uploaded in the last 1 hour.");
                    return;
                }

                //Azure Document Intelligence Client
                var client = new DocumentIntelligenceClient(
                    new Uri(_config["AzureDocAI:Endpoint"]),
                    new AzureKeyCredential(_config["AzureDocAI:ApiKey"])
                );

                foreach (var obj in recentFiles)
                {
                    Console.WriteLine($"Processing File: {obj.Key}");

                    using var response = await s3.GetObjectAsync(bucket, obj.Key);
                    using var fileStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(fileStream);
                    fileStream.Position = 0;

                    if  (obj.Key == "3012628055..._1001_05122025_151206_1.pdf") //(obj.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {

                        var operation = await client.AnalyzeDocumentAsync(
                            WaitUntil.Completed,
                            "prebuilt-invoice",
                            BinaryData.FromStream(fileStream)
                        );

                        string rawJson = operation.GetRawResponse().Content.ToString();

                        double accuracy = CalculateAccuracy(rawJson, obj.Key);

                        ParseInvoice(rawJson, obj.Key, accuracy);

                        Console.WriteLine("\n✅ Extracted and Saved Successfully\n");

                    }
                }

                Console.WriteLine("---- Scheduler Finished ----");
            }
            catch(Exception ex) 
            {
                _repo.ErrorLog(ex.Message, "");
            }
        }

        private void ParseInvoice(string json, string filename, double accuracy)
        {
            try
            {

                var cleanHeader = new InvoiceHeader();
                var items = new List<InvoiceItem>();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;


                if (!root.TryGetProperty("analyzeResult", out JsonElement analyzeResult))
                {
                    Console.WriteLine("❌ No analyzeResult found");
                    return;
                }

                var docs = analyzeResult.GetProperty("documents");
                if (docs.ValueKind == JsonValueKind.Array && docs.GetArrayLength() > 0)
                {
                    var firstDoc = docs[0];
                    var fields = firstDoc.GetProperty("fields");

                    // Helper method
                    string GetString(string name)
                    {
                        if (fields.TryGetProperty(name, out JsonElement val))
                        {
                            if (val.TryGetProperty("valueString", out var v)) return v.GetString();
                            if (val.TryGetProperty("content", out var c)) return c.GetString();
                            if (val.TryGetProperty("valueDate", out var d)) return d.GetString();
                            if (val.TryGetProperty("valueCurrency", out var cur)
                                && cur.TryGetProperty("amount", out var amt))
                                return amt.GetDouble().ToString("0.00");
                        }
                        return "";
                    }

                    // ✅ HEADER FIELDS
                    cleanHeader.VendorName = GetString("VendorName");
                    cleanHeader.InvoiceId = GetString("InvoiceId");
                    cleanHeader.InvoiceDate = NormalizeDate(GetString("InvoiceDate"));
                    cleanHeader.PurchaseOrder = GetString("PurchaseOrder");
                    cleanHeader.CustomerName = GetString("CustomerName");
                    cleanHeader.VendorTaxId = GetString("VendorTaxId");
                    cleanHeader.CustomerTaxId = GetString("CustomerTaxId");
                    cleanHeader.AmountDue = GetString("AmountDue");
                    cleanHeader.PaymentTerm = GetString("PaymentTerm");
                    cleanHeader.VendorAddressRecipient = GetString("VendorAddressRecipient");
                    cleanHeader.InvoiceTotal = GetString("InvoiceTotal");
                    cleanHeader.SubTotal = GetString("SubTotal");
                    cleanHeader.CustomerAddress = GetString("CustomerAddress");
                    if (string.IsNullOrEmpty(cleanHeader.CustomerAddress))
                    {
                        cleanHeader.CustomerAddress = GetString("BillingAddress");
                    }
                    cleanHeader.VendorAddress = GetString("VendorAddress");

                    // ✅ LINE ITEMS
                    if (fields.TryGetProperty("Items", out JsonElement itemsArr) &&
                        itemsArr.TryGetProperty("valueArray", out JsonElement arr))
                    {
                        foreach (var item in arr.EnumerateArray())
                        {
                            if (item.TryGetProperty("valueObject", out JsonElement obj))
                            {
                                var i = new InvoiceItem
                                {
                                    Description = obj.TryGetProperty("Description", out var desc)
                                        && desc.TryGetProperty("valueString", out var d)
                                        ? d.GetString()
                                        : "",
                                    ProductCode = obj.TryGetProperty("ProductCode", out var prod)
                                        && prod.TryGetProperty("valueString", out var p)
                                        ? p.GetString()
                                        : "",
                                    Quantity = obj.TryGetProperty("Quantity", out var qty)
                                        && qty.TryGetProperty("valueNumber", out var q)
                                        ? q.GetDouble()
                                        : 0,
                                    Unit = obj.TryGetProperty("Unit", out var u)
                                        && u.TryGetProperty("valueString", out var un)
                                        ? un.GetString()
                                        : "",
                                    Amount = obj.TryGetProperty("Amount", out var amt)
                                        && amt.TryGetProperty("valueCurrency", out var cur)
                                        && cur.TryGetProperty("amount", out var a)
                                        ? a.GetDouble()
                                        : 0,
                                    Tax = obj.TryGetProperty("Tax", out var tx)
                                        && tx.TryGetProperty("valueNumber", out var t)
                                        ? t.GetDouble()
                                        : 0,
                                    TaxRate = obj.TryGetProperty("TaxRate", out var txr)
                                        && txr.TryGetProperty("valueNumber", out var tr)
                                        ? tr.GetDouble()
                                        : 0,
                                    UnitPrice = obj.TryGetProperty("UnitPrice", out var up)
                                        && up.TryGetProperty("valueCurrency", out var uprice)
                                        && uprice.TryGetProperty("amount", out var b)
                                        ? b.GetDouble()
                                        : 0
                                };
                                items.Add(i);
                            }
                        }
                    }
                }

                _repo.SaveInvoice(cleanHeader, items, filename, json, accuracy);

                //validateOcrData(cleanHeader.PurchaseOrder, cleanHeader.InvoiceId, cleanHeader.VendorTaxId, cleanHeader.VendorName);

                //SendInvoiceToSapAsync("L-INV/2526/02614");

                Console.WriteLine("=== HEADER ===");
                Console.WriteLine(JsonSerializer.Serialize(cleanHeader, new JsonSerializerOptions { WriteIndented = true }));

                Console.WriteLine("=== LINE ITEMS ===");
                Console.WriteLine(JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));

                string OtherBucket = _config["AWS:OtherBucket"];
                string region = _config["AWS:Region"];
                _s3.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = OtherBucket,
                    Key = filename
                });

                _repo.InsertLog_forDeletedS3bucketFiles(OtherBucket, filename);
            }
            catch (Exception ex)
            {
                _repo.ErrorLog(ex.Message, "");
            }
        }

        private string NormalizeDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            string[] formats = {
                "dd-MM-yyyy", "d-MM-yyyy",
                "dd-MMM-yyyy", "dd-MMMM-yyyy", "d-MMM-yyyy", "d-MMMM-yyyy",
                "dd.MM.yyyy", "d.M.yyyy",
                "dd/MM/yyyy", "d/M/yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.M.d",
                "MM-dd-yyyy", "M-d-yyyy", "MM/dd/yyyy", "M/d/yyyy",
                "MMM dd, yyyy", "MMMM dd, yyyy", "dd MMM yyyy", "d MMM yyyy",
                "dd MMMM yyyy", "d MMMM yyyy",
                "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss",
                "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(input.Trim(), formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {

                return parsedDate.ToString("yyyy-MM-dd");
            }


            return "";
        }

        public async Task validateOcrData(string purchaseOrder, string invoiceId, string gstnum, string vendorname)
        {
            string baseUrl = "https://example.com/api/getData";

            string url = $"{baseUrl}?po={purchaseOrder}&invoice={invoiceId}&gst={gstnum}&vendorname={vendorname}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            string result = await response.Content.ReadAsStringAsync();
        }


        public async Task SendInvoiceToSapAsync(string invoiceId)
        {
            try
            {
                string invoiceJson = await _repo.GetInvoiceDataForSapAsync(invoiceId);

                if (string.IsNullOrWhiteSpace(invoiceJson))
                {
                    Console.WriteLine("⚠️ No invoice data found for given ID.");
                    return;
                }

                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13 
                };

                using (var client = new HttpClient(handler, disposeHandler: true))
                {
                    client.Timeout = TimeSpan.FromSeconds(60);

                    string url = "https://fioriqa.sapphirefoods.in/sap/bc/zinv_service?sap-client=300";

                    var credentials = Encoding.ASCII.GetBytes("pandeyh:P@ndey123456789");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MyApp/1.0)");
                    client.DefaultRequestHeaders.Add("SAP-Client", "300");         
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest"); 

                   
                    var content = new StringContent(invoiceJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);

                    string responseText = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");

                    if (!response.IsSuccessStatusCode)
                    {
                      
                        Console.WriteLine("Response Headers:");
                        foreach (var h in response.Headers)
                            Console.WriteLine($"{h.Key}: {string.Join(",", h.Value)}");

                        Console.WriteLine("Response Body:");
                        Console.WriteLine(responseText);

                        _repo.ErrorLog($"SAP POST failed. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseText}", invoiceId);
                        return;
                    }

                    Console.WriteLine("Response:");
                    Console.WriteLine(responseText);
                }
            }
            catch (TaskCanceledException tce) when (!tce.CancellationToken.IsCancellationRequested)
            {
                _repo.ErrorLog("Timeout when sending invoice to SAP: " + tce.Message, invoiceId);
            }
            catch (Exception ex)
            {
                _repo.ErrorLog("Error sending invoice to SAP: " + ex.Message, invoiceId);
            }
        }


        public static double CalculateAccuracy(string rawJson, string fileName)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("analyzeResult", out var analyzeResult))
                return 0;

            var documents = analyzeResult.GetProperty("documents");
            if (documents.GetArrayLength() == 0)
                return 0;

            var fields = documents[0].GetProperty("fields");

            double headerTotal = 0;
            int headerCount = 0;

            // HEADER ACCURACY

            foreach (var field in fields.EnumerateObject())
            {
                if (field.Value.TryGetProperty("confidence", out var conf))
                {
                    headerTotal += conf.GetDouble();
                    headerCount++;
                }
            }

            double headerAccuracy = headerCount > 0 ? (headerTotal / headerCount) * 100 : 0;


            // LINE ITEM ACCURACY

            double lineTotal = 0;
            int lineCount = 0;

            if (fields.TryGetProperty("Items", out JsonElement itemsArr) &&
                itemsArr.TryGetProperty("valueArray", out JsonElement arr))
            {
                foreach (var item in arr.EnumerateArray())
                {
                    if (item.TryGetProperty("valueObject", out var obj))
                    {
                        double itemTotal = 0;
                        int itemFields = 0;

                        foreach (var subField in obj.EnumerateObject())
                        {
                            if (subField.Value.TryGetProperty("confidence", out var conf))
                            {
                                itemTotal += conf.GetDouble();
                                itemFields++;
                            }
                        }

                        if (itemFields > 0)
                        {
                            lineTotal += (itemTotal / itemFields);
                            lineCount++;
                        }
                    }
                }
            }

            double lineAccuracy = lineCount > 0 ? (lineTotal / lineCount) * 100 : 0;

            // FINAL OVERALL ACCURACY

            double finalAccuracy = (headerAccuracy + lineAccuracy) / 2;

            Console.WriteLine("\n==================== Accuracy Report ====================");
            Console.WriteLine($"File: {fileName}");
            Console.WriteLine($"Header Accuracy     : {headerAccuracy:F2}%");
            Console.WriteLine($"Line Item Accuracy  : {lineAccuracy:F2}%");
            Console.WriteLine($"Final OCR Accuracy  : {finalAccuracy:F2}%");
            Console.WriteLine("=========================================================\n");

            return finalAccuracy;
        }
        

    }




    public class InvoiceHeader
    {
        public string VendorName { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceDate { get; set; }
        public string PurchaseOrder { get; set; }
        public string CustomerName { get; set; }
        public string VendorTaxId { get; set; }
        public string CustomerTaxId { get; set; }
        public string AmountDue { get; set; }
        public string PaymentTerm { get; set; }
        public string VendorAddressRecipient { get; set; }
        public string InvoiceTotal { get; set; }
        public string SubTotal { get; set; }
        public string VendorAddress { get; set; }
        public string CustomerAddress { get; set; }

    }

    public class InvoiceItem
    {
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public double Amount { get; set; }
        public double Tax { get; set; }
        public double TaxRate { get; set; }
        public double UnitPrice { get; set; }
    }
}
