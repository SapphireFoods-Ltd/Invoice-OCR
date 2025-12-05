namespace Vendor_OCR.Services
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Vendor_OCR.Repositories;

    public class VendorExportService
    {
        private readonly VendorRepository _repo;
        private readonly string _defaultOutputFolder;

        public VendorExportService(VendorRepository repo, string defaultOutputFolder)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _defaultOutputFolder = string.IsNullOrWhiteSpace(defaultOutputFolder) ? Path.GetTempPath() : defaultOutputFolder;
        }

        /// <summary>
        /// Exports the result of stored proc for the given mode to a hash-separated CSV.
        /// If outputFolder is provided, it will be used; otherwise the configured default is used.
        /// Returns the full path of the created file.
        /// </summary>
        public async Task<string> ExportHashCsvAsync(string mode, string? outputFolder = null, CancellationToken ct = default)
        {
            var dt = await _repo.GetVendorsByModeAsync(mode, ct).ConfigureAwait(false);

            var folder = string.IsNullOrWhiteSpace(outputFolder) ? _defaultOutputFolder : outputFolder!;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{DateTime.Now:dd-MM-yyyy HH-mm}_Input.csv";
            string path = Path.Combine(folder, fileName);

            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var sw = new StreamWriter(fs, new UTF8Encoding(true)); // BOM included

            // Header row (dynamic from DataTable)
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0) await sw.WriteAsync("#");
                await sw.WriteAsync(SanitizeHeader(dt.Columns[i].ColumnName));
            }
            await sw.WriteLineAsync();

            // Rows
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) await sw.WriteAsync("#");

                    var raw = row[i];
                    var formatted = FormatValue(raw);
                    formatted = SanitizeField(formatted);

                    await sw.WriteAsync(formatted);
                }
                await sw.WriteLineAsync();
            }

            await sw.FlushAsync();
            return path;
        }

        private static string SanitizeHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return "";
            return header.Replace("#", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string SanitizeField(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("#", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string FormatValue(object? raw)
        {
            if (raw == null || raw == DBNull.Value) return "";

            if (raw is DateTime dt)
                return dt.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            if (raw is string s)
            {
                if (DateTime.TryParse(s, out var parsed))
                    return parsed.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return s;
            }

            if (raw is IFormattable f)
                return f.ToString(null, CultureInfo.InvariantCulture);

            return raw.ToString() ?? "";
        }
    }


}
