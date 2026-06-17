using BudgetManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BudgetManager.Infrastructure
{
    /// <summary>
    /// Zapis i odczyt transakcji z pliku JSON.
    /// Plik: %APPDATA%\BudgetManager\transactions.json
    /// </summary>
    public static class DataService
    {
        private static readonly string _folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetManager");
        private static readonly string _filePath = Path.Combine(_folder, "transactions.json");

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static List<TransactionItem> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return GetSampleData();

                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<TransactionItem>>(json, _options)
                       ?? new List<TransactionItem>();
            }
            catch
            {
                return GetSampleData();
            }
        }

        public static void Save(IEnumerable<TransactionItem> transactions)
        {
            try
            {
                Directory.CreateDirectory(_folder);
                string json = JsonSerializer.Serialize(transactions, _options);
                File.WriteAllText(_filePath, json);
            }
            catch { }
        }

        private static List<TransactionItem> GetSampleData()
        {
            return new List<TransactionItem>
            {
                new TransactionItem { Id = 1, Date = new DateTime(2026, 4, 1),  Type = "Przychód", Category = "Wynagrodzenie",     Amount = 5200.00m, Description = "Wynagrodzenie za kwiecień" },
                new TransactionItem { Id = 2, Date = new DateTime(2026, 4, 3),  Type = "Wydatek",  Category = "Zakupy spożywcze", Amount = 185.40m,  Description = "Zakupy w supermarkecie" },
                new TransactionItem { Id = 3, Date = new DateTime(2026, 4, 7),  Type = "Wydatek",  Category = "Transport",        Amount = 120.00m,  Description = "Bilet miesięczny" },
                new TransactionItem { Id = 4, Date = new DateTime(2026, 4, 10), Type = "Wydatek",  Category = "Rachunki",         Amount = 350.00m,  Description = "Rachunki za kwiecień" },
                new TransactionItem { Id = 5, Date = new DateTime(2026, 4, 15), Type = "Przychód", Category = "Inne",             Amount = 800.00m,  Description = "Zlecenie dodatkowe" },
                new TransactionItem { Id = 6, Date = new DateTime(2026, 5, 1),  Type = "Przychód", Category = "Wynagrodzenie",    Amount = 5200.00m, Description = "Wynagrodzenie za maj" },
                new TransactionItem { Id = 7, Date = new DateTime(2026, 5, 4),  Type = "Wydatek",  Category = "Zakupy spożywcze", Amount = 210.80m,  Description = "Tygodniowe zakupy" },
                new TransactionItem { Id = 8, Date = new DateTime(2026, 5, 12), Type = "Wydatek",  Category = "Rozrywka",         Amount = 89.99m,   Description = "Kino + kolacja" },
                new TransactionItem { Id = 9, Date = new DateTime(2026, 5, 20), Type = "Wydatek",  Category = "Sport",            Amount = 150.00m,  Description = "Karnet na siłownię" },
            };
        }
    }
}