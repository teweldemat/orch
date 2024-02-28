using ClosedXML.Excel;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace orch.core.report.Converters.ClosedXML
{
    public class ClosedXmlXlsGenerator : IXlsGenerator
    {
        public Task<FileContentResult> Generate(XlsRequest request)
        {
            if (!request.Worksheets.Any())
            {
                throw new InvalidOperationException("No worksheets to generate");
            }

            using var workbook = new XLWorkbook();
            foreach (var worksheet in request.Worksheets)
            {
                var sheet = workbook.Worksheets.Add(worksheet.Name);

                if (request.Header?.Center is not null)
                {
                    if (!string.IsNullOrEmpty(request.Header.Center.Title))
                    {
                        sheet.PageSetup.Header.Center.AddText(request.Header.Center.Title)
                            .SetFontSize(16);
                    }
                }

                if (worksheet is XlsDataTableWorksheet dataTableWorksheet)
                {
                    if (dataTableWorksheet.DataTable.Rows.Count.Equals(0))
                    {
                        continue;
                    }

                    sheet.Cell(1, 1).InsertTable(dataTableWorksheet.DataTable);

                    continue;
                }

                var worksheetType = worksheet.GetType();

                if (worksheetType.IsGenericType && worksheetType.GetGenericTypeDefinition() == typeof(XlsCollectionWorksheet<>))
                {
                    dynamic collectionWorksheet = worksheet;
                    PropertyInfo[] properties = collectionWorksheet.Data[0].GetType().GetProperties() ?? Array.Empty<PropertyInfo>();

                    if (request.Header?.Center is not null)
                    {
                        if (collectionWorksheet.Data.Count.Equals(0))
                        {
                            continue;
                        }

                        if (collectionWorksheet.IncludedProperties.Count > 0)
                        {

                            var includedPropertiesList = (from property in properties
                                                          where collectionWorksheet.IncludedProperties.Contains(property.Name)
                                                          select property).ToList();

                            var includedProperties = includedPropertiesList.ToArray();

                            var rowCount = sheet.RowsUsed().Count();

                            for (int index = 0; index < includedProperties.Length; index++)
                            {
                                var property = includedProperties[index];
                                var cell = sheet.Cell(rowCount + 1, index + 1);
                                cell.Style.Font.FontSize = 12;
                                cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.InsideBorderColor = XLColor.LightGray;

                                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.OutsideBorderColor = XLColor.LightGray;

                                cell.Value = property.Name.Humanize(LetterCasing.Title);
                            }

                            for (var i = rowCount; i < collectionWorksheet.Data.Count; i++)
                            {
                                for (var j = 0; j < includedProperties.Length; j++)
                                {
                                    var cell = sheet.Cell(i + 2, j + 1);
                                    cell.Style.Font.FontSize = 12;

                                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.BottomBorderColor = XLColor.LightGray;

                                    cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.InsideBorderColor = XLColor.LightGray;

                                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.OutsideBorderColor = XLColor.LightGray;

                                    cell.Value = includedProperties[j].GetValue(collectionWorksheet.Data[i])?.ToString() ?? default;
                                }
                            }
                        }
                        else
                        {
                            sheet.Cell(1, 1).InsertTable(collectionWorksheet.Data);
                        }

                    }

                    sheet.Columns().AdjustToContents();
                    sheet.Rows().AdjustToContents();
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return Task.FromResult(new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = FormatFileName(request.FileDownloadName)
            });

        }

        private static string FormatFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "data.xlsx";
            }

            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || !fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".xlsx";
            }

            return fileName;
        }
    }
}
