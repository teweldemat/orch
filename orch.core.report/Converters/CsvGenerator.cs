using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using System.Reflection.PortableExecutable;
using orch.core;
using orch.common;


namespace orch.report.Generators
{
    public static class CsvGenerator
    {

        public static FileContentResult ToCsvFileContentResult<T>(this IEnumerable<T> list, string fileName, params string[] includedProperties)
        {
            var csvData = GenerateCsvFromList(list, includedProperties);
            var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true); 
            var contentBytes = utf8WithBom.GetBytes(csvData);
            var contentType = "text/csv";
            return new FileContentResult(contentBytes, contentType)
            {
                FileDownloadName = FormatFileName(fileName)
            };
        }
        public static FileContentResult ToCsvFileContentResultWithHeader<T>(this IEnumerable<T> list, string fileName, List<string> headerData, params string[] includedProperties)
        {


            var csvData = GenerateCsvFromListWithHeader(list, headerData, includedProperties);
            var contentBytes = Encoding.UTF8.GetBytes(csvData);
            var contentType = "text/csv";
            return new FileContentResult(contentBytes, contentType)
            {
                FileDownloadName = FormatFileName(fileName)
            };
        }

        [Obsolete("Use DataTableToCsvFileContentResultV2 instead")]
        public static FileContentResult DataTableToCsvFileContentResult(this DataTable dataTable, string fileName)
        {
            var csvData = GenerateCsvFromDataTable(dataTable);
            var contentBytes = Encoding.UTF8.GetBytes(csvData);
            var contentType = "text/csv";
            return new FileContentResult(contentBytes, contentType)
            {
                FileDownloadName = FormatFileName(fileName)
            };
        }

        public static FileContentResult DataTableToCsvFileContentResultV2(this DataTable dataTable, string fileName)
        {
            var csvData = GenerateCsvFromDataTableV2(dataTable);
            var contentBytes = Encoding.UTF8.GetBytes(csvData);
            const string contentType = "text/csv";
            return new FileContentResult(contentBytes, contentType)
            {
                FileDownloadName = FormatFileName(fileName)
            };
        }

        private static string FormatFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "data.csv";
            }

            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            return fileName;
        }

        private static string GenerateCsvFromList<T>(IEnumerable<T> list, params string[] includedProperties)
        {
            var allProperties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var properties = includedProperties.Length == 0
                ? allProperties.Values.ToArray()
                : includedProperties
                    .Where(name => allProperties.ContainsKey(name))
                    .Select(name => allProperties[name])
                    .ToArray();


            var stringBuilder = new StringBuilder();

            foreach (var prop in properties)
            {
                stringBuilder.Append(prop.Name);
                stringBuilder.Append(",");
            }

            stringBuilder.AppendLine();

            foreach (var item in list)
            {
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? "";
                    stringBuilder.Append(Helpers.EscapeStringForCsvField(value));
                    stringBuilder.Append(",");
                }

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        private static string GenerateCsvFromListWithHeader<T>(IEnumerable<T> list, List<string>? headerData, params string[] includedProperties)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .Where(prop => includedProperties.Length == 0 || includedProperties.Contains(prop.Name))
                .ToArray();

            var stringBuilder = new StringBuilder();

            if (headerData != null)
            {
                foreach (var header in headerData)
                {
                    stringBuilder.Append(header);

                    stringBuilder.AppendLine();
                }
                stringBuilder.AppendLine();
            }

            foreach (var prop in properties)
            {
                stringBuilder.Append(prop.Name);
                stringBuilder.Append(",");

            }

            stringBuilder.AppendLine();

            foreach (var item in list)
            {
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? "";
                    stringBuilder.Append(value);
                    stringBuilder.Append(",");
                }

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
        [Obsolete("Use GenerateCsvFromDataTableV2 instead")]
        private static string GenerateCsvFromDataTable(DataTable dataTable)
        {
            var stringBuilder = new StringBuilder();
            var properties = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(",", properties));


            stringBuilder.AppendLine();
            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                stringBuilder.AppendLine(string.Join(",", fields));
            }

            return stringBuilder.ToString();
        }

        public static string GenerateCsvFromDataTableV2(DataTable dataTable)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            foreach (DataColumn column in dataTable.Columns)
            {
                csv.WriteField(column.ColumnName);
            }
            csv.NextRecord();

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var field in row.ItemArray)
                {
                    csv.WriteField(field);
                }
                csv.NextRecord();
            }

            return writer.ToString();
        }
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
    }
}