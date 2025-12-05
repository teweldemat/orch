using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;


namespace orch.report.Generators
{
    public static class XlsGenerator
    {

        public static FileContentResult ToXlsFileContentResult<T>(
            this IEnumerable<T> list, string fileName, params string[] includedProperties) where T : class
        {
            var sheetName = string.IsNullOrEmpty(fileName) ? "Sheet1" : fileName;

            if (sheetName.Length > 31)
                sheetName = sheetName.Substring(0, 31);

            // 1. Setup
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            var currentRow = 1;

            // Use TypeDescriptor to get property information efficiently
            var allProperties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            // Filter and select properties based on the includedProperties list
            var propertiesToExport = includedProperties.Length == 0
                ? allProperties.Values.ToArray()
                : includedProperties
                    .Where(name => allProperties.ContainsKey(name))
                    .Select(name => allProperties[name])
                    .ToArray();

            // 2. Write Headers (Row 1)
            int col = 1;
            foreach (var prop in propertiesToExport)
            {
                worksheet.Cell(currentRow, col++).Value = prop.Name;
            }
            currentRow++;

            // 3. Write Data Rows
            foreach (var item in list)
            {
                col = 1;
                foreach (var prop in propertiesToExport)
                {
                    var value = prop.GetValue(item);
                    // Use XLCellValue.FromObject for robust type handling (numbers, dates, strings)
                    worksheet.Cell(currentRow, col++).Value = XLCellValue.FromObject(value);
                }
                currentRow++;
            }

            // Optional: Auto-adjust column widths
            worksheet.Columns().AdjustToContents();

            // 4. Generate and Return FileContentResult
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                // Append .xlsx extension for the download name
                var fileDownloadName = $"{fileName}.xlsx";

                return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileDownloadName
                };
            }
        }
        public static FileContentResult ToXlsFileContentResultWithHeader<T>(
            this IEnumerable<T> list,string fileName,List<string>? headerData,params string[] includedProperties) where T : class
        {
            // 1. Setup and Sheet Name Validation
            var sheetName = string.IsNullOrEmpty(fileName) ? "Sheet1" : fileName;

            // Truncate sheet name to the 31-character limit
            if (sheetName.Length > 31)
                sheetName = sheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            var currentRow = 1;

            // 2. Write Custom Header Data
            if (headerData != null && headerData.Any())
            {
                foreach (var header in headerData)
                {
                    // Write each header string into the first cell (Column 1)
                    worksheet.Cell(currentRow, 1).Value = header;
                    currentRow++;
                }
                currentRow++; // Add an extra blank line below the header data for separation
            }

            // 3. Prepare Data Properties (Same logic as the original method)
            var allProperties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var propertiesToExport = includedProperties.Length == 0
                ? allProperties.Values.ToArray()
                : includedProperties
                    .Where(name => allProperties.ContainsKey(name))
                    .Select(name => allProperties[name])
                    .ToArray();

            // 4. Write Data Headers (The column headers for the main data)
            int col = 1;
            foreach (var prop in propertiesToExport)
            {
                worksheet.Cell(currentRow, col++).Value = prop.Name;
            }
            currentRow++;

            // 5. Write Data Rows
            foreach (var item in list)
            {
                col = 1;
                foreach (var prop in propertiesToExport)
                {
                    var value = prop.GetValue(item);
                    // Use XLCellValue.FromObject for robust type handling
                    worksheet.Cell(currentRow, col++).Value = XLCellValue.FromObject(value);
                }
                currentRow++;
            }

            // Optional: Auto-adjust column widths
            worksheet.Columns().AdjustToContents();

            // 6. Generate and Return FileContentResult
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileDownloadName = $"{fileName}.xlsx";

                return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileDownloadName
                };
            }
        }
        public static FileContentResult DataTableToXlsFileContentResult(this DataTable dataTable, string fileName)
        {
            var csvData = CsvGenerator.GenerateCsvFromDataTableV2(dataTable);

            var sheetName = string.IsNullOrEmpty(fileName) ? "Sheet1" : fileName;
            if (sheetName.Length > 31)
                sheetName = sheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            var currentRow = 1;

            using (var reader = new StringReader(csvData))
            using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                // Read header
                csv.Read();
                csv.ReadHeader();
                int col = 1;
                foreach (var header in csv.HeaderRecord)
                {
                    worksheet.Cell(currentRow, col++).Value = header;
                }
                currentRow++;

                // Read data rows
                while (csv.Read())
                {
                    col = 1;
                    foreach (var header in csv.HeaderRecord)
                    {
                        var text = csv.GetField(header);
                        object value = ParseToBestType(text);
                        worksheet.Cell(currentRow, col++).Value = XLCellValue.FromObject(value);
                    }
                    currentRow++;
                }
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                var fileDownloadName = $"{fileName}.xlsx";
                return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileDownloadName
                };
            }
        }

        private static object ParseToBestType(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (bool.TryParse(text, out var b)) return b;
            if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i)) return i;
            if (long.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var l)) return l;
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            if (double.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var dbl)) return dbl;
            if (DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var dt)) return dt;
            return text;
        }

    }
}