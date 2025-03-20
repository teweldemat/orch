using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using System.Reflection.PortableExecutable;
using orch.core;
 

namespace orch.report.Generators
{
    public static class CsvGenerator
    {
        
        public static FileContentResult ToCsvFileContentResult<T>(this IEnumerable<T> list, string fileName, params string[] includedProperties)
        {
            var csvData = GenerateCsvFromList(list, includedProperties);
            var contentBytes = Encoding.UTF8.GetBytes(csvData);
            var contentType = "text/csv";
            return new FileContentResult(contentBytes, contentType)
            {
                FileDownloadName = FormatFileName(fileName)
            };
        } 
        public static FileContentResult ToCsvFileContentResultWithHeader<T>(this IEnumerable<T> list, string fileName,List<string> headerData, params string[] includedProperties)
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
            var properties = TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .Where(prop => includedProperties.Length == 0 || includedProperties.Contains(prop.Name))
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
                    stringBuilder.Append(value);
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
    }
}