using ClosedXML.Excel;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using TyrlovInventoryCRM.Models;

namespace TyrlovInventoryCRM.Services
{
    public static class ReportService
    {
        static ReportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (File.Exists(fontPath))
                {
                    using var stream = File.OpenRead(fontPath);
                    FontManager.RegisterFont(stream);
                }
            }
            catch
            {
            }
        }

        public static void ExportToExcel(IEnumerable<Product> products, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Остатки на складе");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Название";
            worksheet.Cell(1, 3).Value = "Категория";
            worksheet.Cell(1, 4).Value = "Цена";
            worksheet.Cell(1, 5).Value = "Остаток";

            var headerRange = worksheet.Range("A1:E1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var p in products)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.Name;
                worksheet.Cell(row, 3).Value = p.Category;
                worksheet.Cell(row, 4).Value = p.Price;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₽";
                worksheet.Cell(row, 5).Value = p.Quantity;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            foreach (var col in worksheet.Columns())
            {
                col.Width = System.Math.Max(col.Width + 4, 12);
            }

            workbook.SaveAs(filePath);
        }

        public static void ExportToPdf(IEnumerable<Product> products, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Text("Отчет: Текущие остатки на складе")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Название").Bold();
                            header.Cell().Text("Категория").Bold();
                            header.Cell().Text("Цена").Bold();
                            header.Cell().Text("Остаток").Bold();
                        });

                        foreach (var p in products)
                        {
                            table.Cell().Text(p.Id.ToString());
                            table.Cell().Text(p.Name);
                            table.Cell().Text(p.Category);
                            table.Cell().Text($"{p.Price:F2} руб.");
                            table.Cell().Text(p.Quantity.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Страница ");
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(filePath);
        }
    }
}