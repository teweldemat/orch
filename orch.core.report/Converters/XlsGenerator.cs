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
        public static FileContentResult DataTableToXlsFileContentResult(
            this DataTable dataTable, string fileName)
        {
            var workbook = new XLWorkbook();
            // Use the file name as the sheet name, truncating to 31 chars (Excel limit)
            var sheetName = string.IsNullOrEmpty(fileName) ? "Sheet1" : fileName;
            if (sheetName.Length > 31)
                sheetName = sheetName.Substring(0, 31);

            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(dataTable, true);
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                // Append .xlsx extension for the download name
                var fileDownloadName = $"{fileName}.xlsx";

                // Return the result with the correct MIME type for .xlsx
                return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileDownloadName
                };
            }
        }
    }
}