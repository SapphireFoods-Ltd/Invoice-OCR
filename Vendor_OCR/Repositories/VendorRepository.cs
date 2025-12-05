using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;
using Vendor_OCR.Controllers;
using Vendor_OCR.Models;
using Vendor_OCR.Models.Invoice;
using Vendor_OCR.Services;
using static Vendor_OCR.Models.VendorRegisterModel;

namespace Vendor_OCR.Repositories
{
    public class VendorRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public VendorRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("SqlConnectionString");
        }

        public RegisterModel GetVendorDetails(string vendorCode)
        {
            var model = new RegisterModel();
            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using var con = new SqlConnection(connStr);
            con.Open();

            using (var cmd = new SqlCommand("SP_GetVendor_Details", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Vendor_Code", vendorCode);

                using var reader = cmd.ExecuteReader();

                if (!reader.Read()) return null;

                model.StoreName = reader["StoreName"]?.ToString();
                model.Category = reader["Category"]?.ToString();
                model.BusinessArea = reader["BusinessArea"]?.ToString();
                model.VendorGroup = reader["VendorGroup"]?.ToString();
                model.CompanyCode = reader["CompanyCode"]?.ToString();
                model.Role = reader["Role"]?.ToString();
                model.PurchaseOrg = reader["PurchaseOrg"]?.ToString();
                model.SearchTerm = reader["SearchTerm"]?.ToString();
                model.PaymentTerm = reader["pt_name"]?.ToString();
                model.PaymentMethod = reader["p_name"]?.ToString();
                model.Currency = reader["OrderCurrency"]?.ToString();
                model.CompanyName = reader["CompanyName"]?.ToString();
                model.ProprietorName = reader["ProprietorName"]?.ToString();
                model.PartnershipName = reader["PartnershipName"]?.ToString();
                model.MobileNumber = reader["MobileNumber"]?.ToString();
                model.Email = reader["Vendor_Email"]?.ToString();
                model.Building = reader["Building"]?.ToString();
                model.Street1 = reader["Street1"]?.ToString();
                model.Street2 = reader["Street2"]?.ToString();
                model.City = reader["City"]?.ToString();
                model.PostalCode = reader["PostalCode"]?.ToString();
                model.State = reader["State"]?.ToString();
                model.Country = reader["Country"]?.ToString();
                model.GstNo = reader["GstNo"]?.ToString();
                model.PanNo = reader["PanNo"]?.ToString();
                model.AadharNo = reader["AadharNo"]?.ToString();
                model.IsMSME = reader["IsMSME"] != DBNull.Value && Convert.ToBoolean(reader["IsMSME"]);
                model.MSMENo = reader["MSMENo"]?.ToString();
                model.NatureOfExpense = reader["NatureOfExpense"]?.ToString();
                model.HasLowerTaxDeduction = reader["HasLowerTaxDeduction"] != DBNull.Value && Convert.ToBoolean(reader["HasLowerTaxDeduction"]);
                model.LowerTaxCertNo = reader["LowerTaxCertNo"]?.ToString();
                model.ReconciliationAccount = reader["ReconciliationAccount"]?.ToString();
                model.CinLlpin = reader["CinLlpin"]?.ToString();
                model.Remarks = reader["Remarks"]?.ToString();
                model.ExistPanFile = reader["PanFile"]?.ToString();
                model.ExistAadharFile = reader["AadharFile"]?.ToString();
                model.ExistBankStatementFile = reader["BankStatementFile"]?.ToString();
                model.ExistContractFile = reader["ContractFile"]?.ToString();
                model.ExistGstCertificateFile = reader["GstCertificateFile"]?.ToString();
                model.ExistMsmeCertificateFile = reader["MsmeCertificateFile"]?.ToString();
                model.ExistLowerTaxCertificateFile = reader["LowerTaxCertificateFile"]?.ToString();
                model.ExistCertificateOfIncorporationFile = reader["CertificateOfIncorporationFile"]?.ToString();

                if (reader.NextResult())
                {
                    model.BankAccounts.Clear();
                    while (reader.Read())
                    {
                        model.BankAccounts.Add(new RegisterModel.BankAccount
                        {
                            IFSC = reader["IFSC_code"]?.ToString(),
                            AccountNumber = reader["Account_Number"]?.ToString(),
                            AccountHolderName = reader["AccountHolder_Name"]?.ToString(),
                            BankName = reader["Bank_Name"]?.ToString()
                        });
                    }
                }

                if (!model.BankAccounts.Any())
                    model.BankAccounts.Add(new RegisterModel.BankAccount());
            }

            return model;
        }

        public int UpdateVendorDetails(RegisterModel model, string actionType, string vendorId)
        {
            int vendorDbId = 0;
            string connStr = _configuration.GetConnectionString("SqlConnectionString");
            string insertDate = DateTime.Now.ToString("dd-MM-yyyy h:mm tt");

            using var con = new SqlConnection(connStr);
            con.Open();

            using (var cmd = new SqlCommand("SP_UpdateVendor_Detailes", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Vendor_Code", vendorId);
                cmd.Parameters.AddWithValue("@mode", actionType);
                cmd.Parameters.AddWithValue("@CompanyName", model.CompanyName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ProprietorName", model.ProprietorName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PartnershipName", model.PartnershipName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Building", model.Building ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Street1", model.Street1 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Street2", model.Street2 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@City", model.City ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PostalCode", model.PostalCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@State", model.State ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Country", model.Country ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GstNo", model.GstNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PanNo", model.PanNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AadharNo", model.AadharNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsMSME", model.IsMSME);
                cmd.Parameters.AddWithValue("@MSMENo", model.MSMENo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NatureOfExpense", model.NatureOfExpense ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@HasLowerTaxDeduction", model.HasLowerTaxDeduction);
                cmd.Parameters.AddWithValue("@LowerTaxCertNo", model.LowerTaxCertNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ReconciliationAccount", model.ReconciliationAccount ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CinLlpin", model.CinLlpin ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OCR_AadharNo", model.OCR_AadharNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OCR_PanNo", model.OCR_PanNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PanFile", model.ExistPanFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AadharFile", model.ExistAadharFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BankStatementFile", model.ExistBankStatementFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ContractFile", model.ExistContractFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GstCertificateFile", model.ExistGstCertificateFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MsmeCertificateFile", model.ExistMsmeCertificateFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LowerTaxCertificateFile", model.ExistLowerTaxCertificateFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CertificateOfIncorporationFile", model.ExistCertificateOfIncorporationFile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ins_dt", insertDate);

                object result = cmd.ExecuteScalar();
                vendorDbId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }

            if (model.BankAccounts != null && model.BankAccounts.Any())
            {
                foreach (var bank in model.BankAccounts)
                {
                    using var cmdBank = new SqlCommand("SP_InsertVendor_BankDetails", con);
                    cmdBank.CommandType = CommandType.StoredProcedure;
                    cmdBank.Parameters.AddWithValue("@VID", vendorDbId);
                    cmdBank.Parameters.AddWithValue("@Vendor_Code", vendorId);
                    cmdBank.Parameters.AddWithValue("@AccountHolder_Name", bank.AccountHolderName ?? (object)DBNull.Value);
                    cmdBank.Parameters.AddWithValue("@Account_Number", bank.AccountNumber ?? (object)DBNull.Value);
                    cmdBank.Parameters.AddWithValue("@IFSC_code", bank.IFSC ?? (object)DBNull.Value);
                    cmdBank.Parameters.AddWithValue("@Bank_Name", bank.BankName ?? (object)DBNull.Value);
                    cmdBank.ExecuteNonQuery();
                }
            }

            return vendorDbId;
        }

        public List<Menu> GetMenuForRole(string roleId)
        {
            var menus = new List<Menu>();
            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("SP_GetMenuByRole", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RoleId", roleId ?? string.Empty);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                menus.Add(new Menu
                {
                    Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Controller = reader["Controller"]?.ToString(),
                    Action = reader["Action"]?.ToString(),
                    Icon = reader["Icon"]?.ToString(),
                    ParentId = reader["ParentId"] != DBNull.Value && !string.IsNullOrEmpty(reader["ParentId"].ToString())
                               ? Convert.ToInt32(reader["ParentId"])
                               : (int?)null,
                    Role = reader["Role"] != DBNull.Value && !string.IsNullOrEmpty(reader["Role"].ToString())
                           ? Convert.ToInt32(reader["Role"])
                           : (int?)null,
                    SortOrder = reader["SortOrder"] != DBNull.Value && !string.IsNullOrEmpty(reader["SortOrder"].ToString())
                                ? Convert.ToInt32(reader["SortOrder"])
                                : 0
                });
            }

            // Build hierarchy
            return menus
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.SortOrder)
                .Select(m => BuildChildren(m, menus))
                .ToList();
        }

        private Menu BuildChildren(Menu parent, List<Menu> allItems)
        {
            parent.Children = allItems
                .Where(m => m.ParentId == parent.Id)
                .OrderBy(m => m.SortOrder)
                .Select(m => BuildChildren(m, allItems))
                .ToList();

            return parent;
        }


        public List<VendorList> GetVendors()
        {
            List<VendorList> vendors = new List<VendorList>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Get_VendorList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vendors.Add(new VendorList
                            {
                                Vendor_Code = Convert.ToInt32(dr["Vendor_Code"]),
                                Vendor_Name = dr["Vendor_Name"].ToString(),
                                Vendor_Email = dr["Vendor_Email"].ToString(),
                                Flag = dr["Flag"] == DBNull.Value ? null : Convert.ToInt32(dr["Flag"]),
                                ins_dt = dr["ins_dt"] == DBNull.Value ? null : Convert.ToDateTime(dr["ins_dt"])
                            });
                        }
                    }
                }
            }

            return vendors;
        }


        public bool UpdateVendorEmail(string vendorId, string newEmail)
        {

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("usp_UpdateVendorEmail", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Vendor_Code", vendorId);
            cmd.Parameters.AddWithValue("@Vendor_Email", newEmail);

            conn.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;


        }



        public async Task<List<VendorInvoiceList>> GetVendorInvoicesAsync(string Vendor_Code)
        {
            var invoices = new List<VendorInvoiceList>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            await using var conn = new SqlConnection(connStr);
            await using var command = new SqlCommand("dbo.SP_OCR_selectquery", conn)
            {
                CommandType = CommandType.StoredProcedure

            };
            command.Parameters.AddWithValue("@mode", "GetInvoiceDetails");
            command.Parameters.AddWithValue("@condition1", Vendor_Code);
            await conn.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                invoices.Add(new VendorInvoiceList
                {
                    InvoiceName = reader["InvoiceName"]?.ToString(),
                    
                    ins_dt = reader["ins_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ins_dt"]),
                    raw_Name = reader["raw_Name"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    S3BucketLink = reader["S3_BucketPath"]?.ToString(),
                    action = reader["action"]?.ToString(),
                    ID = reader["Id"]?.ToString()
                });
            }

            return invoices;
        }


        public async Task<List<HomeModel>>GetInvoicesProcess(string sap_code)
        {
            var invoices = new List<HomeModel>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            await using var conn = new SqlConnection(connStr);
            await using var command = new SqlCommand("dbo.SP_OCR_InvoiceProcess", conn)
            {
                CommandType = CommandType.StoredProcedure

            };
            command.Parameters.AddWithValue("@mode", "getInvoices_Process_Storewise");
            command.Parameters.AddWithValue("@condition1", sap_code);
            await conn.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                invoices.Add(new HomeModel
                {
                    OrderNumber = reader["OrderNumber"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OrderNumber"]),
                    InvoiceNumber = reader["InvoiceNumber"]?.ToString(),
                    Remark = reader["Remark"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    ins_by = reader["ins_by"]?.ToString(),
                    ins_dt = reader["ins_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ins_dt"]),
                    PrevStatus = reader["PrevStatus"]?.ToString(),
                    StatusText = reader["StatusText"]?.ToString(),
                    StatusClass = reader["StatusClass"]?.ToString(),
                    IsSynthetic = reader["IsSynthetic"] == DBNull.Value ? null : (bool?)Convert.ToBoolean(reader["IsSynthetic"]),
                    InvoiceName = reader["InvoiceName"]?.ToString(),
                    raw_Name = reader["raw_Name"]?.ToString(),
                    S3_BucketPath = reader["S3_BucketPath"]?.ToString()
                });
            }
            return invoices;
        }



public async Task<List<HomeModel>> GetInvoicesProcess_storewise(string sap_code)
{
    var list = new List<HomeModel>();

    string connStr = _configuration.GetConnectionString("SqlConnectionString");

    await using var conn = new SqlConnection(connStr);
    await using var command = new SqlCommand("dbo.SP_OCR_InvoiceProcess", conn)
    {
        CommandType = CommandType.StoredProcedure
    };

    command.Parameters.AddWithValue("@mode", "Process_Storewise");
    command.Parameters.AddWithValue("@condition1", sap_code);

    await conn.OpenAsync();
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var model = new HomeModel
        {
            InvoiceNumber = reader["InvoiceNumber"] == DBNull.Value ? null : reader["InvoiceNumber"].ToString(),
            Remark        = reader["Remark"] == DBNull.Value ? null : reader["Remark"].ToString(),
            Status        = reader["Status"] == DBNull.Value ? null : reader["Status"].ToString(),
            InvoiceName   = reader["InvoiceName"] == DBNull.Value ? null : reader["InvoiceName"].ToString(),
            raw_Name      = reader["raw_Name"] == DBNull.Value ? null : reader["raw_Name"].ToString(),
            S3_BucketPath = reader["S3_BucketPath"] == DBNull.Value ? null : reader["S3_BucketPath"].ToString(),
            ins_dt        = reader["ins_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ins_dt"]),
            Uploaded = reader["Uploaded"] == DBNull.Value ? null : reader["Uploaded"].ToString(),
            Approved = reader["Approved"] == DBNull.Value ? null : reader["Approved"].ToString(),
            OCR = reader["OCR"] == DBNull.Value ? null : reader["OCR"].ToString(),
            SAPPush = reader["SAPPush"] == DBNull.Value ? null : reader["SAPPush"].ToString()
        };

        list.Add(model);
    }

    return list;
}



        public async Task<List<HomeModel>> GetInvoicesProcess_Vendorwise(string empcode)
        {
            var list = new List<HomeModel>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            await using var conn = new SqlConnection(connStr);
            await using var command = new SqlCommand("dbo.SP_OCR_InvoiceProcess", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@mode", "Vendor_wiseProcess");
            command.Parameters.AddWithValue("@condition1", empcode);

            await conn.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var model = new HomeModel
                {
                    InvoiceNumber = reader["InvoiceNumber"] == DBNull.Value ? null : reader["InvoiceNumber"].ToString(),
                    Remark = reader["Remark"] == DBNull.Value ? null : reader["Remark"].ToString(),
                    Status = reader["Status"] == DBNull.Value ? null : reader["Status"].ToString(),
                    InvoiceName = reader["InvoiceName"] == DBNull.Value ? null : reader["InvoiceName"].ToString(),
                    raw_Name = reader["raw_Name"] == DBNull.Value ? null : reader["raw_Name"].ToString(),
                    S3_BucketPath = reader["S3_BucketPath"] == DBNull.Value ? null : reader["S3_BucketPath"].ToString(),
                    ins_dt = reader["ins_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ins_dt"]),
                    Uploaded = reader["Uploaded"] == DBNull.Value ? null : reader["Uploaded"].ToString(),
                    Approved = reader["Approved"] == DBNull.Value ? null : reader["Approved"].ToString(),
                    OCR = reader["OCR"] == DBNull.Value ? null : reader["OCR"].ToString(),
                    SAPPush = reader["SAPPush"] == DBNull.Value ? null : reader["SAPPush"].ToString()
                };

                list.Add(model);
            }

            return list;
        }

        //Nidhika
        public List<InvoiceList> GetInvoices(string status)
        {
            List<InvoiceList> invoices = new List<InvoiceList>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "getInvoices");
                    cmd.Parameters.AddWithValue("@condition1", status);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            invoices.Add(new InvoiceList
                            {

                                Id = Convert.ToInt32(dr["Id"]),
                                InvoiceNumber = dr["InvoiceNumber"].ToString(),
                                InvoiceName = dr["InvoiceName"].ToString(),
                                ins_Name = dr["ins_Name"].ToString(),
                                Remark = dr["Remarks"].ToString(),
                                Status = dr["Status"].ToString(),
                                raw_Name = dr["raw_Name"].ToString(),
                                S3BucketLink = dr["S3_BucketPath"].ToString()
                            });
                        }
                    }
                }
            }

            return invoices;
        }

        public List<InvoiceList> GetInvoicesSCM(string status, string sapcode)
        {
            List<InvoiceList> invoices = new List<InvoiceList>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "getInvoicesSCM");
                    cmd.Parameters.AddWithValue("@condition1", status);
                    cmd.Parameters.AddWithValue("@condition2", sapcode);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            invoices.Add(new InvoiceList
                            {

                                Id = Convert.ToInt32(dr["Id"]),
                                InvoiceNumber = dr["InvoiceNumber"].ToString(),
                                InvoiceName = dr["InvoiceName"].ToString(),
                                ins_Name = dr["ins_Name"].ToString(),
                                Remark = dr["Remarks"].ToString(),
                                Status = dr["Status"].ToString(),
                                RejectRemark = dr["StatusRemark"].ToString(),
                                raw_Name = dr["raw_Name"].ToString(),
                                S3BucketLink = dr["S3_BucketPath"].ToString()
                            });
                        }
                    }
                }
            }

            return invoices;
        }

        public List<InvoiceDetailList> GetInvoicesDetailList()
        {
            List<InvoiceDetailList> invoices = new List<InvoiceDetailList>();

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "GetInvoicesDetail");
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            invoices.Add(new InvoiceDetailList
                            {
                                rId = Convert.ToInt32(dr["rid"]),
                                InvoiceId = dr["InvoiceId"].ToString(),
                                InvoiceDate = dr["InvoiceDate"].ToString(),
                                PurchaseOrder = dr["PurchaseOrder"].ToString(),
                                VendorName = dr["VendorName"].ToString(),
                            });
                        }
                    }
                }
            }

            return invoices;
        }

        public InvoiceDetail GetInvoicesDetail(int Rid)
        {
            InvoiceDetail invoice = null;

            string connStr = _configuration.GetConnectionString("SqlConnectionString");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "GetInvoicesDetailById");
                    cmd.Parameters.AddWithValue("@Condition1", Rid);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new InvoiceDetail
                            {
                                Rid = Convert.ToInt32(dr["rid"]),
                                InvoiceId = dr["InvoiceId"]?.ToString(),
                                InvoiceType = dr["InvoiceType"]?.ToString(),
                                SupplierGSTN = dr["Supplier_GSTN"]?.ToString(),
                                InvoiceDate = dr["InvoiceDate"]?.ToString(),
                                PurchaseOrder = dr["PurchaseOrder"]?.ToString(),
                                DueDate = dr["DueDate"]?.ToString(),
                                VendorName = dr["VendorName"]?.ToString(),
                                VendorAddress = dr["VendorAddress"]?.ToString(),
                                CustomerName = dr["CustomerName"]?.ToString(),
                                CustomerAddress = dr["Customer_Address"]?.ToString(),
                                SubTotal = dr["SubTotal"]?.ToString(),
                                TotalTax = dr["TotalTax"]?.ToString(),
                                InvoiceTotal = dr["InvoiceTotal"]?.ToString(),
                                Currency = dr["Currency"]?.ToString(),
                                GRNNumber = dr["GRN_Number"]?.ToString(),
                                SESNumber = dr["SES_Number"]?.ToString(),
                                SupplierEmail = dr["Supplier_Email"]?.ToString(),
                                SupplierVATNumber = dr["Supplier_VAT_Number"]?.ToString(),
                                SupplierCINNumber = dr["Supplier_CIN_Number"]?.ToString(),
                                CustomerGSTN = dr["Customer_GSTN"]?.ToString(),
                                CustomerEmail = dr["Customer_Email"]?.ToString(),
                                CustomerVAT = dr["Customer_VAT"]?.ToString(),
                                CGST_P = dr["CGST_P"]?.ToString(),
                                CGST_Value = dr["CGST_Value"]?.ToString(),
                                SGST_P = dr["SGST_P"]?.ToString(),
                                SGST_Value = dr["SGST_Value"]?.ToString(),
                                IGST_P = dr["IGST_P"]?.ToString(),
                                IGST_Value = dr["IGST_Value"]?.ToString(),
                                VAT_P = dr["VAT_P"]?.ToString(),
                                VAT_Value = dr["VAT_Value"]?.ToString(),
                                Freight = dr["Freight"]?.ToString(),
                                Insurance = dr["Insurance"]?.ToString(),
                                Package = dr["Package"]?.ToString(),
                                LoadingCharges = dr["LoadingCharges"]?.ToString(),
                                CustomerNo = dr["CustomerNo"]?.ToString(),
                                PanNumber = dr["PanNumber"]?.ToString(),
                                InsDt = dr["ins_dt"]?.ToString(),
                                InsBy = dr["ins_by"]?.ToString(),
                                UpdDt = dr["upd_dt"]?.ToString(),
                                UpdBy = dr["upd_by"]?.ToString(),
                                FileName = dr["file_name"]?.ToString(),
                            };
                        }
                    }
                }
            }
            if (invoice != null)
                invoice.SubItems = GetInvoiceSubDetail(Rid);
            return invoice;
        }

        public List<InvoiceSubDetail> GetInvoiceSubDetail(int InvoiceDetailID)
        {
            List<InvoiceSubDetail> items = new List<InvoiceSubDetail>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SqlConnectionString")))
            using (SqlCommand cmd = new SqlCommand("SP_OCR_selectquery", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetInvoiceSubDetails");
                cmd.Parameters.AddWithValue("@Condition1", InvoiceDetailID);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        items.Add(new InvoiceSubDetail
                        {
                            Rid = Convert.ToInt32(dr["rid"]),
                            InvoiceDetailsId = Convert.ToInt32(dr["InvoiceDetailsId"]),
                            InvoiceId = dr["InvoiceId"]?.ToString(),
                            ProductCode = dr["ProductCode"]?.ToString(),
                            Description = dr["Description"]?.ToString(),
                            Unit = dr["Unit"]?.ToString(),
                            Quantity = dr["Quantity"]?.ToString(),
                            UnitPrice = dr["UnitPrice"]?.ToString(),
                            Amount = dr["Amount"]?.ToString(),
                            CGST_PER = dr["CGST_PER"]?.ToString(),
                            CGST_VAL = dr["CGST_VAL"]?.ToString(),
                            SGST_PER = dr["SGST_PER"]?.ToString(),
                            SGST_VAL = dr["SGST_VAL"]?.ToString(),
                            IGST_PER = dr["IGST_PER"]?.ToString(),
                            IGST_VAL = dr["IGST_VAL"]?.ToString(),
                            Taxable_Value = dr["Taxable_Value"]?.ToString()
                        });
                    }
                }
            }
            return items;
        }

        public int UpdateInvoiceDetail(InvoiceDetail invoice)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SqlConnectionString")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateInvoiceDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Rid", invoice.Rid);

                    cmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId ?? "");
                    cmd.Parameters.AddWithValue("@InvoiceType", invoice.InvoiceType ?? "");

                    cmd.Parameters.AddWithValue("@SupplierGSTN", invoice.SupplierGSTN ?? "");
                    cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate ?? "");
                    cmd.Parameters.AddWithValue("@PurchaseOrder", invoice.PurchaseOrder ?? "");
                    cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate ?? "");

                    cmd.Parameters.AddWithValue("@VendorName", invoice.VendorName ?? "");
                    cmd.Parameters.AddWithValue("@VendorAddress", invoice.VendorAddress ?? "");

                    cmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName ?? "");
                    cmd.Parameters.AddWithValue("@CustomerAddress", invoice.CustomerAddress ?? "");

                    cmd.Parameters.AddWithValue("@SubTotal", invoice.SubTotal ?? "");
                    cmd.Parameters.AddWithValue("@TotalTax", invoice.TotalTax ?? "");
                    cmd.Parameters.AddWithValue("@InvoiceTotal", invoice.InvoiceTotal ?? "");

                    cmd.Parameters.AddWithValue("@Currency", invoice.Currency ?? "");
                    cmd.Parameters.AddWithValue("@GRNNumber", invoice.GRNNumber ?? "");
                    cmd.Parameters.AddWithValue("@SESNumber", invoice.SESNumber ?? "");

                    cmd.Parameters.AddWithValue("@SupplierEmail", invoice.SupplierEmail ?? "");
                    cmd.Parameters.AddWithValue("@SupplierVATNumber", invoice.SupplierVATNumber ?? "");
                    cmd.Parameters.AddWithValue("@SupplierCINNumber", invoice.SupplierCINNumber ?? "");

                    cmd.Parameters.AddWithValue("@CustomerGSTN", invoice.CustomerGSTN ?? "");
                    cmd.Parameters.AddWithValue("@CustomerEmail", invoice.CustomerEmail ?? "");
                    cmd.Parameters.AddWithValue("@CustomerVAT", invoice.CustomerVAT ?? "");

                    cmd.Parameters.AddWithValue("@CGST_P", invoice.CGST_P ?? "");
                    cmd.Parameters.AddWithValue("@CGST_Value", invoice.CGST_Value ?? "");
                    cmd.Parameters.AddWithValue("@SGST_P", invoice.SGST_P ?? "");
                    cmd.Parameters.AddWithValue("@SGST_Value", invoice.SGST_Value ?? "");
                    cmd.Parameters.AddWithValue("@IGST_P", invoice.IGST_P ?? "");
                    cmd.Parameters.AddWithValue("@IGST_Value", invoice.IGST_Value ?? "");
                    cmd.Parameters.AddWithValue("@VAT_P", invoice.VAT_P ?? "");
                    cmd.Parameters.AddWithValue("@VAT_Value", invoice.VAT_Value ?? "");

                    cmd.Parameters.AddWithValue("@Freight", invoice.Freight ?? "");
                    cmd.Parameters.AddWithValue("@Insurance", invoice.Insurance ?? "");
                    cmd.Parameters.AddWithValue("@Package", invoice.Package ?? "");
                    cmd.Parameters.AddWithValue("@LoadingCharges", invoice.LoadingCharges ?? "");

                    cmd.Parameters.AddWithValue("@CustomerNo", invoice.CustomerNo ?? "");
                    cmd.Parameters.AddWithValue("@PanNumber", invoice.PanNumber ?? "");

                    cmd.Parameters.AddWithValue("@UpdDt", invoice.UpdDt ?? "");
                    cmd.Parameters.AddWithValue("@UpdBy", invoice.UpdBy ?? "");

                    cmd.Parameters.AddWithValue("@actiontype", invoice.actiontype ?? "");


                    DataTable dt = new DataTable();
                    dt.Columns.Add("Rid", typeof(int));
                    dt.Columns.Add("InvoiceDetailsId", typeof(int));
                    dt.Columns.Add("InvoiceId", typeof(string));
                    dt.Columns.Add("ProductCode", typeof(string));
                    dt.Columns.Add("Description", typeof(string));
                    dt.Columns.Add("Unit", typeof(string));
                    dt.Columns.Add("Quantity", typeof(string));
                    dt.Columns.Add("UnitPrice", typeof(string));
                    dt.Columns.Add("Amount", typeof(string));
                    dt.Columns.Add("CGST_PER", typeof(string));
                    dt.Columns.Add("CGST_VAL", typeof(string));
                    dt.Columns.Add("SGST_PER", typeof(string));
                    dt.Columns.Add("SGST_VAL", typeof(string));
                    dt.Columns.Add("IGST_PER", typeof(string));
                    dt.Columns.Add("IGST_VAL", typeof(string));
                    dt.Columns.Add("Taxable_Value", typeof(string));

                    foreach (var item in invoice.SubItems)
                    {
                        dt.Rows.Add(
                            item.Rid,
                            item.InvoiceDetailsId,
                            item.InvoiceId ?? "",
                            item.ProductCode ?? "",
                            item.Description ?? "",
                            item.Unit ?? "",
                            item.Quantity ?? "",
                            item.UnitPrice ?? "",
                            item.Amount ?? "",
                            item.CGST_PER ?? "",
                            item.CGST_VAL ?? "",
                            item.SGST_PER ?? "",
                            item.SGST_VAL ?? "",
                            item.IGST_PER ?? "",
                            item.IGST_VAL ?? "",
                            item.Taxable_Value ?? ""
                        );
                    }

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@SubItems", dt);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.InvoiceSubDetailType";

                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public string AddVendorEmail(VendorInput vendor)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "Insert_vendor_email");
                    cmd.Parameters.AddWithValue("@condition1", vendor.VendorName ?? "");
                    cmd.Parameters.AddWithValue("@condition2", vendor.Email ?? "");
                    cmd.Parameters.AddWithValue("@condition4", vendor.TermsOfPayment ?? "");
                    cmd.Parameters.AddWithValue("@condition5", vendor.PaymentMethods ?? "");
                    cmd.Parameters.AddWithValue("@condition6", vendor.Currency ?? "");
                    cmd.Parameters.AddWithValue("@condition7", vendor.Category ?? "");
                    cmd.Parameters.AddWithValue("@condition8", vendor.BusinessArea ?? "");
                    cmd.Parameters.AddWithValue("@condition9", vendor.VendorGroup ?? "");
                    cmd.Parameters.AddWithValue("@condition10", vendor.CompanyCode ?? "");
                    cmd.Parameters.AddWithValue("@condition11", vendor.Role ?? "");
                    cmd.Parameters.AddWithValue("@condition12", vendor.PurOrg ?? "");
                    cmd.Parameters.AddWithValue("@condition13", vendor.SearchTerm ?? "");
                    cmd.Parameters.AddWithValue("@condition14", vendor.StoreName ?? "");
                    cmd.Parameters.AddWithValue("@condition15", vendor.empcode ?? "");

                    // Add OUTPUT parameter for vendor_code
                    SqlParameter outputParam = new SqlParameter("@condition_output", SqlDbType.VarChar, 10)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    // Retrieve vendor code returned from SP
                    string vendorCode = outputParam.Value?.ToString();
                    return vendorCode;
                }
            }
        }


        public void ErrorLog(string errormsg, string user_code, string page = "")
        {

            if (string.IsNullOrEmpty(page))
            {
                var httpContext = new HttpContextAccessor().HttpContext;
                page = httpContext?.Request?.Path.Value;
            }


            using (SqlConnection con = new SqlConnection(_connectionString))
            {

                using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "Insert_error_log");
                    cmd.Parameters.AddWithValue("@condition1", errormsg);
                    cmd.Parameters.AddWithValue("@condition2", user_code);
                    cmd.Parameters.AddWithValue("@condition3", page);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public string ValidateVendorCode(string vendorCode)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "Validate_vendor_code");
                    cmd.Parameters.AddWithValue("@condition1", vendorCode);

                    con.Open();
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                        return "Invalid";
                    else
                        return result.ToString();
                }
            }
        }

        public VendorRegisterModel GetVendorRequestDetails(string vendorCode)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetVendorRequestDetails");
                cmd.Parameters.AddWithValue("@condition1", vendorCode ?? (object)DBNull.Value);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new VendorRegisterModel
                        {
                            Name = dr["vendor_name"]?.ToString() ?? string.Empty,
                            Email = dr["vendor_email"]?.ToString() ?? string.Empty
                        };
                    }
                    return null;
                }
            }
        }

        public List<VendorGroup> GetVendorGroups()
        {
            List<VendorGroup> vendorGroups = new List<VendorGroup>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetVendorGroups");
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    vendorGroups.Add(new VendorGroup
                    {
                        Id = Convert.ToInt32(dr["rid"]),
                        GroupName = dr["vendor_group"].ToString()
                    });
                }

                conn.Close();
            }

            return vendorGroups;
        }

        public List<VendorGroup> GetCategory()
        {
            List<VendorGroup> vendorGroups = new List<VendorGroup>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetCategories");
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    vendorGroups.Add(new VendorGroup
                    {
                        Id = Convert.ToInt32(dr["rid"]),
                        Category = dr["category"].ToString()
                    });
                }

                conn.Close();
            }

            return vendorGroups;
        }


        public VendorDetails GetVendorDetailsByGroup(int vendorGroupId)
        {
            VendorDetails details = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "GetVendorDetailsByGroup");
                    cmd.Parameters.AddWithValue("@condition1", vendorGroupId);
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        details = new VendorDetails
                        {
                            VendorGroupId = Convert.ToInt32(dr["VendorGroupId"]),
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            Role = dr["role"].ToString(),
                            PurchaseOrg = dr["purchase_org"].ToString(),
                            SearchTerm = dr["search_term"].ToString()
                        };
                    }
                }
            }

            return details;
        }

        public VendorDetails GetVendorDetailsByCategory(int categoryId)
        {
            VendorDetails details = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mode", "GetVendorDetailsByCategory");
                    cmd.Parameters.AddWithValue("@condition1", categoryId);
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        details = new VendorDetails
                        {
                            VendorGroupId = Convert.ToInt32(dr["VendorGroupId"]),
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            Role = dr["role"].ToString(),
                            PurchaseOrg = dr["purchase_org"].ToString(),
                            SearchTerm = dr["search_term"].ToString()
                        };
                    }
                }
            }

            return details;
        }

        public List<PaymentGroup> GetPaymentMethod()
        {
            List<PaymentGroup> paymentGroups = new List<PaymentGroup>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetPaymentMethod");
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    paymentGroups.Add(new PaymentGroup
                    {
                        Id = dr["p_code"].ToString(),
                        PaymentMethod = dr["p_name"].ToString()
                    });
                }

                conn.Close();
            }

            return paymentGroups;
        }

        public List<PaymentGroup> GetPaymentTerms()
        {
            List<PaymentGroup> paymentGroups = new List<PaymentGroup>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mode", "GetPayTermMaster");
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    paymentGroups.Add(new PaymentGroup
                    {
                        Id = dr["pt_code"].ToString(),
                        PaytermStatus = dr["pt_name"].ToString()
                    });
                }

                conn.Close();
            }

            return paymentGroups;
        }

        public async Task<bool> SaveVendorAsync(VendorPassword vendor)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mode", "Insert_Vendor");

                cmd.Parameters.AddWithValue("@condition2", vendor.Password ?? "");

                cmd.Parameters.AddWithValue("@condition1", vendor.VendorCode ?? "");


                await conn.OpenAsync();
                int result = await cmd.ExecuteNonQueryAsync();

                return result > 0;
            }
        }

        public void SaveInvoice(InvoiceHeader cleanHeader, List<InvoiceItem> items, string filename, string json, double accuracy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int headerId = 0;

                        using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@mode", "Insert_invoice_details");
                            cmd.Parameters.AddWithValue("@condition1", cleanHeader.InvoiceId ?? "");
                            cmd.Parameters.AddWithValue("@condition2", cleanHeader.VendorName ?? "");
                            cmd.Parameters.AddWithValue("@condition3", cleanHeader.InvoiceDate ?? "");
                            cmd.Parameters.AddWithValue("@condition4", cleanHeader.PurchaseOrder ?? "");
                            cmd.Parameters.AddWithValue("@condition5", cleanHeader.CustomerName ?? "");
                            cmd.Parameters.AddWithValue("@condition6", cleanHeader.VendorTaxId ?? "");
                            cmd.Parameters.AddWithValue("@condition7", cleanHeader.CustomerTaxId ?? "");
                            cmd.Parameters.AddWithValue("@condition8", cleanHeader.AmountDue ?? "");
                            cmd.Parameters.AddWithValue("@condition9", cleanHeader.PaymentTerm ?? "");
                            cmd.Parameters.AddWithValue("@condition10", cleanHeader.VendorAddress ?? "");
                            cmd.Parameters.AddWithValue("@condition11", cleanHeader.InvoiceTotal ?? "");
                            cmd.Parameters.AddWithValue("@condition12", cleanHeader.SubTotal ?? "");
                            cmd.Parameters.AddWithValue("@condition13", filename ?? "");
                            cmd.Parameters.AddWithValue("@condition14", json ?? "");
                            cmd.Parameters.AddWithValue("@condition15", cleanHeader.CustomerAddress ?? "");
                            cmd.Parameters.AddWithValue("@condition16", cleanHeader.VendorAddressRecipient ?? "");
                            cmd.Parameters.AddWithValue("@condition17", accuracy);

                            object result = cmd.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int newId))
                            {
                                headerId = newId;
                            }
                            else
                            {
                                throw new Exception("Failed to retrieve inserted header ID.");
                            }
                        }

                        foreach (var item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@mode", "Insert_sub_invoice_details");
                                cmd.Parameters.AddWithValue("@condition1", cleanHeader.InvoiceId ?? "");
                                cmd.Parameters.AddWithValue("@condition2", item.Description ?? "");
                                cmd.Parameters.AddWithValue("@condition3", item.ProductCode ?? "");
                                cmd.Parameters.AddWithValue("@condition4", item.Quantity.ToString());
                                cmd.Parameters.AddWithValue("@condition5", item.Unit ?? "");
                                cmd.Parameters.AddWithValue("@condition6", item.Amount.ToString());
                                cmd.Parameters.AddWithValue("@condition7", item.Tax.ToString());
                                cmd.Parameters.AddWithValue("@condition8", item.TaxRate.ToString());
                                cmd.Parameters.AddWithValue("@condition9", item.UnitPrice.ToString());

                                cmd.Parameters.AddWithValue("@condition10", headerId);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error inserting invoice data", ex);
                    }
                }
            }
        }

        public async Task<string> GetInvoiceDataForSapAsync(string invoiceId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mode", "GetInvoiceDataForSap");
                cmd.Parameters.AddWithValue("@condition1", invoiceId ?? "");

                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                {
                    if (await reader.ReadAsync())
                    {
                        using (var textReader = reader.GetTextReader(0))
                        {
                            return await textReader.ReadToEndAsync();
                        }
                    }
                }

                return string.Empty;
            }
        }

        public class DocumentRow
        {
            public int Id { get; set; }
            public string InvoiceId { get; set; }
            public string CustomerName { get; set; }
            public string VendorName { get; set; }
            public int accuracy { get; set; }
            public string PurchaseOrder { get; set; }
            public int uploadInvoiceId { get; set; }
            public string emailid { get; set; }

            public string uploadedon { get; set; }
            public string uploaderName { get; set; }
        }
        
        public async Task<List<DocumentRow>> GetDocumentsToValidate()
        {
            var list = new List<DocumentRow>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_OCR_ValidateDocuments", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mode", "GetInvoiceDetail");

                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new DocumentRow
                        {
                            Id = Convert.ToInt32(reader["rid"]),
                            InvoiceId = reader["InvoiceId"].ToString(),
                            CustomerName = reader["CustomerName"].ToString(),
                            VendorName = reader["VendorName"].ToString(),
                            accuracy = reader["accuracy"] == DBNull.Value ? 0 : Convert.ToInt32(reader["accuracy"]),
                            PurchaseOrder = reader["PurchaseOrder"]?.ToString(),
                            uploadInvoiceId = reader["uploadInvoiceId"] == DBNull.Value? 0 : Convert.ToInt32(reader["uploadInvoiceId"]),
                            emailid = reader["FinalEmail"]?.ToString(),
                            uploadedon = reader["ins_dt"]?.ToString(),
                            uploaderName= reader["ins_name"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        public async Task MarkAsValidated(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_OCR_ValidateDocuments", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mode", "UpdateFlagInvoiceDetail");
                cmd.Parameters.AddWithValue("@rid", id);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task rejectOCRInvoice(int Invoiceid, string failedList)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SP_OCR_ValidateDocuments", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mode", "rejectOCRInvoice");
                cmd.Parameters.AddWithValue("@rid", Invoiceid);
                cmd.Parameters.AddWithValue("@remark", failedList);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void SaveInvoiceFromVendor(IncomingInvoiceDto request, string json)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int headerId = 0;


                        using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@mode", "Insert_invoice_details_fromVendor");

                            cmd.Parameters.AddWithValue("@condition1", (object)request.InvoiceNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition2", (object)request.InvoiceDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition3", (object)request.CustomerName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition4", (object)request.VendorGSTNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition5", (object)request.CustomerGSTNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition6", (object)request.InvoiceTotal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition7", (object)request.VendorAddress ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition8", (object)request.InvoiceTotal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition9", (object)request.InvoiceSubTotal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition26", (object)json ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition11", (object)request.CustomerBillToAddress ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@condition12", (object)request.ConfidenceLevelScore ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@condition13", (object)request.InvoiceTaxes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition14", (object)request.InvoiceCGSTValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition15", (object)request.InvoiceSGSTValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition16", (object)request.InvoiceIGSTValue ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@condition17", (object)request.InvoiceAcknowledgementNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition18", (object)request.InvoiceIRNNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition19", (object)request.InvoiceAcknowledgementDate ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@condition20", (object)request.CustomerOrderNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition21", (object)request.CustomerOrderDate ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@condition22", (object)request.VendorPANNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition23", (object)request.VendorName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition24", (object)request.VendorAddress ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@condition25", (object)request.CustomerShipToAddress ?? DBNull.Value);

                            object result = cmd.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int newId))
                            {
                                headerId = newId;
                            }
                            else
                            {
                                throw new Exception("Failed to retrieve inserted header ID.");
                            }
                        }

                        var items = request.LineItems ?? new List<IncomingLineItemDto>();
                        foreach (var item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@mode", "Insert_sub_invoice_details_fromVendor");

                                cmd.Parameters.AddWithValue("@condition1", (object)request.InvoiceNumber ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition2", (object)item.ItemDescription ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition3", (object)item.ItemCode ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition4", (object)item.ItemQuantity ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition5", (object)item.ItemUnitOfMeasure ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition6", (object)item.ItemNetTotal ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition7", (object)item.ItemTax ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition8", (object)item.ItemTaxPercentage ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition9", (object)item.ItemUnitPrice ?? DBNull.Value);

                                cmd.Parameters.AddWithValue("@condition10", headerId);

                                cmd.Parameters.AddWithValue("@condition11", (object)item.ItemCGSTPercentage ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition12", (object)item.ItemSGSTPercentage ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition13", (object)item.ItemIGSTPercentage ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition14", (object)item.ItemCGSTValue ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition15", (object)item.ItemSGSTValue ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition16", (object)item.ItemIGSTValue ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition17", (object)item.ItemDiscountAmount ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@condition18", (object)item.ItemDiscountPercentage ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error inserting invoice data", ex);
                    }
                }
            }
        }

        public async Task<bool> ValidateUserAsync(string userId, string password)
        {
            using var conn = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("SP_Vendor_Actions", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@mode", "ValidateUser");
            cmd.Parameters.AddWithValue("@condition1", userId ?? "");
            cmd.Parameters.AddWithValue("@condition2", password ?? "");

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result) > 0;
            }

            return false;
        }

        public DataTable GetDataFromStoredProcedure()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SP_DailyInvoiceSummary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        public void InsertLog_forDeletedS3bucketFiles(string S3BucketName, string filename)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdLog = new SqlCommand("usp_Insert_OCR_DeletedS3bucketLog", conn))
                    {
                        cmdLog.CommandType = CommandType.StoredProcedure;
                        cmdLog.Parameters.AddWithValue("@condition1", S3BucketName);
                        cmdLog.Parameters.AddWithValue("@condition2", filename);
                        cmdLog.Parameters.AddWithValue("@condition3", DateTime.Now);
                        cmdLog.ExecuteNonQuery();
                    }


                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<DataTable> GetVendorsByModeAsync(string modeIgnored, CancellationToken ct = default)
        {

            var dt = new DataTable();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SP_Vendor_Actions", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };

            cmd.Parameters.AddWithValue("@mode", "GetFileDataSendToSap");

            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            dt.Load(reader);

            return dt;
        }


    }
}
