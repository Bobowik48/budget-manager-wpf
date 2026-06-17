using BudgetManager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetManager.Infrastructure
{
    public static class PdfService
    {
        public static void GenerateReport(
            IEnumerable<TransactionItem> transactions,
            string monthLabel,
            string outputPath)
        {
            // QuestPDF wymaga ustawienia licencji (Community = darmowa)
            QuestPDF.Settings.License = LicenseType.Community;

            var list = transactions.OrderByDescending(t => t.Date).ToList();
            var income = list.Where(t => t.Type == "Przychód").Sum(t => t.Amount);
            var expenses = list.Where(t => t.Type == "Wydatek").Sum(t => t.Amount);
            var balance = income - expenses;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, list, income, expenses, balance, monthLabel));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Wygenerowano: ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).SemiBold();
                    });
                });
            })
            .GeneratePdf(outputPath);
        }

        private static void ComposeHeader(IContainer container)
        {
            container.BorderBottom(2).BorderColor("#1F2937").PaddingBottom(12).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("💰 Budget Manager")
                       .FontSize(22).Bold().FontColor("#1F2937");
                    col.Item().Text("Raport finansowy")
                       .FontSize(12).FontColor("#6B7280");
                });
            });
        }

        private static void ComposeContent(
            IContainer container,
            List<TransactionItem> list,
            decimal income,
            decimal expenses,
            decimal balance,
            string monthLabel)
        {
            container.Column(col =>
            {
                // Tytuł okresu
                col.Item().PaddingTop(16).Text($"Okres: {monthLabel}")
                   .FontSize(14).SemiBold().FontColor("#111827");

                // Karty podsumowania
                col.Item().PaddingTop(12).PaddingBottom(16).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor("#E5E7EB")
                       .Padding(12).Column(c =>
                       {
                           c.Item().Text("Bilans").FontSize(9).FontColor("#6B7280");
                           c.Item().Text($"{balance:N2} zł")
                            .FontSize(16).Bold()
                            .FontColor(balance >= 0 ? "#15803D" : "#B91C1C");
                       });

                    row.ConstantItem(10);

                    row.RelativeItem().Border(1).BorderColor("#E5E7EB")
                       .Padding(12).Column(c =>
                       {
                           c.Item().Text("Przychody").FontSize(9).FontColor("#6B7280");
                           c.Item().Text($"{income:N2} zł")
                            .FontSize(16).Bold().FontColor("#15803D");
                       });

                    row.ConstantItem(10);

                    row.RelativeItem().Border(1).BorderColor("#E5E7EB")
                       .Padding(12).Column(c =>
                       {
                           c.Item().Text("Wydatki").FontSize(9).FontColor("#6B7280");
                           c.Item().Text($"{expenses:N2} zł")
                            .FontSize(16).Bold().FontColor("#B91C1C");
                       });
                });

                // Nagłówek tabeli
                col.Item().Background("#1F2937").Padding(8).Row(row =>
                {
                    row.ConstantItem(75).Text("Data").FontColor(Colors.White).Bold();
                    row.ConstantItem(70).Text("Typ").FontColor(Colors.White).Bold();
                    row.ConstantItem(110).Text("Kategoria").FontColor(Colors.White).Bold();
                    row.ConstantItem(80).Text("Kwota").FontColor(Colors.White).Bold();
                    row.RelativeItem().Text("Opis").FontColor(Colors.White).Bold();
                });

                // Wiersze tabeli
                bool isEven = false;
                foreach (var t in list)
                {
                    bool isIncome = t.Type == "Przychód";
                    string bg = isEven ? "#F9FAFB" : Colors.White;
                    isEven = !isEven;

                    col.Item().Background(bg).BorderBottom(1).BorderColor("#E5E7EB")
                       .Padding(7).Row(row =>
                       {
                           row.ConstantItem(75).Text(t.Date.ToString("dd.MM.yyyy"));
                           row.ConstantItem(70).Text(t.Type)
                              .FontColor(isIncome ? "#15803D" : "#B91C1C");
                           row.ConstantItem(110).Text(t.Category);
                           row.ConstantItem(80).Text($"{t.Amount:N2} zł")
                              .FontColor(isIncome ? "#15803D" : "#B91C1C");
                           row.RelativeItem().Text(t.Description);
                       });
                }

                // Podsumowanie kategorii wydatków
                if (list.Any(t => t.Type == "Wydatek"))
                {
                    col.Item().PaddingTop(24).Text("Wydatki wg kategorii")
                       .FontSize(12).SemiBold();

                    var byCategory = list
                        .Where(t => t.Type == "Wydatek")
                        .GroupBy(t => t.Category)
                        .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                        .OrderByDescending(x => x.Total);

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.ConstantColumn(100);
                            cols.ConstantColumn(80);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#F3F4F6").Padding(6).Text("Kategoria").Bold();
                            h.Cell().Background("#F3F4F6").Padding(6).Text("Kwota").Bold();
                            h.Cell().Background("#F3F4F6").Padding(6).Text("% wydatków").Bold();
                        });

                        foreach (var cat in byCategory)
                        {
                            double pct = expenses > 0 ? (double)(cat.Total / expenses * 100) : 0;
                            table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(6).Text(cat.Category);
                            table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(6)
                                 .Text($"{cat.Total:N2} zł").FontColor("#B91C1C");
                            table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(6)
                                 .Text($"{pct:F1}%");
                        }
                    });
                }
            });
        }
    }
}