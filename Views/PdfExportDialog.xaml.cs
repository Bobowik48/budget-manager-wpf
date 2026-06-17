using BudgetManager.Infrastructure;
using BudgetManager.Models;
using BudgetManager.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace BudgetManager.Views
{
    public partial class PdfExportDialog : Window
    {
        private readonly IEnumerable<TransactionItem> _allTransactions;

        public PdfExportDialog(IEnumerable<TransactionItem> allTransactions,
                               List<MonthOption> availableMonths,
                               MonthOption currentMonth)
        {
            InitializeComponent();
            _allTransactions = allTransactions;

            MonthComboBox.ItemsSource = availableMonths;
            MonthComboBox.SelectedItem = currentMonth;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            var selected = MonthComboBox.SelectedItem as MonthOption;
            if (selected == null)
            {
                StatusText.Text = "Wybierz zakres danych.";
                return;
            }

            // Filtrowanie
            IEnumerable<TransactionItem> toExport = _allTransactions;
            if (!selected.IsAll)
                toExport = toExport.Where(t =>
                    t.Date.Year == selected.Year && t.Date.Month == selected.Month);

            if (!toExport.Any())
            {
                StatusText.Text = "Brak transakcji dla wybranego okresu.";
                return;
            }

            // Dialog zapisu pliku
            var dialog = new SaveFileDialog
            {
                Title = "Zapisz raport PDF",
                Filter = "PDF|*.pdf",
                FileName = $"BudgetManager_raport_{selected.Label.Replace(" ", "_")}.pdf",
                DefaultExt = "pdf"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                GenerateButton.IsEnabled = false;
                StatusText.Text = "Generowanie...";

                PdfService.GenerateReport(toExport, selected.Label, dialog.FileName);

                if (OpenAfterExport.IsChecked == true)
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });

                Close();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Błąd: {ex.Message}";
                GenerateButton.IsEnabled = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}