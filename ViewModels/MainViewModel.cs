using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using BudgetManager.Infrastructure;
using BudgetManager.Models;

namespace BudgetManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // ── Kolekcje ─────────────────────────────────────────────────────────────
        private readonly ObservableCollection<TransactionItem> _allTransactions = new();

        private ObservableCollection<TransactionItem> _transactions = new();
        public ObservableCollection<TransactionItem> Transactions
        {
            get => _transactions;
            private set { _transactions = value; OnPropertyChanged(); }
        }

        // ── Formularz ─────────────────────────────────────────────────────────────
        private TransactionItem _newTransaction = new();
        public TransactionItem NewTransaction
        {
            get => _newTransaction;
            set { _newTransaction = value; OnPropertyChanged(); }
        }

        private TransactionItem? _selectedTransaction;
        public TransactionItem? SelectedTransaction
        {
            get => _selectedTransaction;
            set { _selectedTransaction = value; OnPropertyChanged(); }
        }

        // ── Opcje ComboBoxów formularza ───────────────────────────────────────────
        public List<string> TypeOptions { get; } = new List<string> { "Przychód", "Wydatek" };

        public List<string> CategoryOptions { get; } = new List<string>
        {
            "Wynagrodzenie", "Zakupy spożywcze", "Transport",
            "Rachunki", "Rozrywka", "Sport", "Inne"
        };

        private string _selectedType = "Wydatek";
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
                if (NewTransaction != null)
                    NewTransaction.Type = value;
            }
        }

        private string _selectedCategory = "Zakupy spożywcze";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (NewTransaction != null)
                    NewTransaction.Category = value;
            }
        }

        // ── Walidacja kwoty ───────────────────────────────────────────────────────
        private string _amountText = string.Empty;
        public string AmountText
        {
            get => _amountText;
            set { _amountText = value; OnPropertyChanged(); ValidateAmount(value); }
        }

        private string _amountError = string.Empty;
        public string AmountError
        {
            get => _amountError;
            private set { _amountError = value; OnPropertyChanged(); }
        }

        private bool _hasAmountError;
        public bool HasAmountError
        {
            get => _hasAmountError;
            private set { _hasAmountError = value; OnPropertyChanged(); }
        }

        // ── Statystyki ────────────────────────────────────────────────────────────
        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            private set { _balance = value; OnPropertyChanged(); }
        }

        private decimal _incomeSummary;
        public decimal IncomeSummary
        {
            get => _incomeSummary;
            private set { _incomeSummary = value; OnPropertyChanged(); }
        }

        private decimal _expensesSummary;
        public decimal ExpensesSummary
        {
            get => _expensesSummary;
            private set { _expensesSummary = value; OnPropertyChanged(); }
        }

        // Etykieta pod statystykami pokazująca aktywny filtr
        private string _summaryLabel = "wszystkie miesiące";
        public string SummaryLabel
        {
            get => _summaryLabel;
            private set { _summaryLabel = value; OnPropertyChanged(); }
        }

        // ── Filtry ────────────────────────────────────────────────────────────────
        private List<MonthOption> _availableMonths = new();
        public List<MonthOption> AvailableMonths
        {
            get => _availableMonths;
            private set { _availableMonths = value; OnPropertyChanged(); }
        }

        private MonthOption? _selectedMonth;
        public MonthOption? SelectedMonth
        {
            get => _selectedMonth;
            set { _selectedMonth = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedTypeFilter = "Wszystkie";
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set { _selectedTypeFilter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public List<string> TypeFilters { get; } = new List<string> { "Wszystkie", "Przychód", "Wydatek" };

        // ── Komendy ───────────────────────────────────────────────────────────────
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand SaveCommand { get; }

        private TransactionItem? _editingTransaction;
        private int _nextId = 1;

        // ── Konstruktor ───────────────────────────────────────────────────────────
        public MainViewModel()
        {
            AddCommand = new RelayCommand(AddTransaction, CanAddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, CanModifyTransaction);
            ClearCommand = new RelayCommand(ClearForm);
            EditCommand = new RelayCommand(EditTransaction, CanModifyTransaction);
            PreviewCommand = new RelayCommand(PreviewTransaction, CanModifyTransaction);
            SaveCommand = new RelayCommand(SaveData);

            LoadData();
            ClearForm(null);
        }

        // ── Dane ──────────────────────────────────────────────────────────────────
        private void LoadData()
        {
            var items = DataService.Load();
            foreach (var item in items)
                _allTransactions.Add(item);

            _nextId = _allTransactions.Any() ? _allTransactions.Max(t => t.Id) + 1 : 1;
            RefreshMonthList();
            ApplyFilter();
        }

        private void SaveData(object? parameter)
        {
            var dialog = new Views.PdfExportDialog(
                _allTransactions,
                AvailableMonths,
                _selectedMonth ?? MonthOption.All);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.ShowDialog();
        }


        // ── Filtry ────────────────────────────────────────────────────────────────
        private void RefreshMonthList()
        {
            var months = _allTransactions
                .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .Distinct()
                .OrderByDescending(d => d)
                .Select(d => new MonthOption(d))
                .ToList();

            months.Insert(0, MonthOption.All);
            AvailableMonths = months;

            if (_selectedMonth == null || !months.Any(m => m.Equals(_selectedMonth)))
                _selectedMonth = months.First();
            OnPropertyChanged(nameof(SelectedMonth));
        }

        private void ApplyFilter()
        {
            IEnumerable<TransactionItem> filtered = _allTransactions;

            if (_selectedMonth != null && !_selectedMonth.IsAll)
                filtered = filtered.Where(t =>
                    t.Date.Year == _selectedMonth.Year && t.Date.Month == _selectedMonth.Month);

            if (_selectedTypeFilter != "Wszystkie")
                filtered = filtered.Where(t => t.Type == _selectedTypeFilter);

            Transactions = new ObservableCollection<TransactionItem>(
                filtered.OrderByDescending(t => t.Date));

            // Statystyki liczone z przefiltrowanej kolekcji
            UpdateSummary(filtered);
        }

        // ── Formularz ─────────────────────────────────────────────────────────────
        private bool CanAddTransaction(object? parameter) =>
            NewTransaction != null
            && !HasAmountError
            && NewTransaction.Amount > 0
            && !string.IsNullOrWhiteSpace(NewTransaction.Description);

        private void AddTransaction(object? parameter)
        {
            if (!TryParseAmount(_amountText, out decimal amount))
            {
                AmountError = "Podaj poprawną kwotę (np. 12,50 lub 12.50)";
                HasAmountError = true;
                return;
            }

            var newTx = new TransactionItem
            {
                Date = NewTransaction.Date,
                Type = SelectedType,
                Category = SelectedCategory,
                Amount = amount,
                Description = NewTransaction.Description
            };

            if (_editingTransaction != null)
            {
                newTx.Id = _editingTransaction.Id;
                int index = _allTransactions.IndexOf(_editingTransaction);
                if (index != -1) _allTransactions[index] = newTx;
                _editingTransaction = null;
            }
            else
            {
                newTx.Id = _nextId++;
                _allTransactions.Add(newTx);
            }

            RefreshMonthList();
            ApplyFilter();
            DataService.Save(_allTransactions);
            ClearForm(null);
        }

        private bool CanModifyTransaction(object? parameter) => _selectedTransaction != null;

        private void DeleteTransaction(object? parameter)
        {
            if (_selectedTransaction == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Czy na pewno chcesz usunąć:\n\"{_selectedTransaction.Description}\"?",
                "Potwierdź usunięcie",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _allTransactions.Remove(_selectedTransaction);
                RefreshMonthList();
                ApplyFilter();
                DataService.Save(_allTransactions);
            }
        }

        private void ClearForm(object? parameter)
        {
            _editingTransaction = null;
            NewTransaction = new TransactionItem
            {
                Date = DateTime.Today,
                Type = "Wydatek",
                Category = "Zakupy spożywcze",
                Amount = 0,
                Description = string.Empty
            };
            SelectedType = "Wydatek";
            SelectedCategory = "Zakupy spożywcze";
            AmountText = string.Empty;
            AmountError = string.Empty;
            HasAmountError = false;
        }

        private void EditTransaction(object? parameter)
        {
            if (_selectedTransaction == null) return;

            _editingTransaction = _selectedTransaction;
            NewTransaction = new TransactionItem
            {
                Date = _selectedTransaction.Date,
                Type = _selectedTransaction.Type,
                Category = _selectedTransaction.Category,
                Amount = _selectedTransaction.Amount,
                Description = _selectedTransaction.Description
            };
            // Ustaw ComboBoxy na właściwe wartości
            SelectedType = _selectedTransaction.Type;
            SelectedCategory = _selectedTransaction.Category;
            AmountText = _selectedTransaction.Amount.ToString("N2");
            HasAmountError = false;
            AmountError = string.Empty;
        }

        private void PreviewTransaction(object? parameter)
        {
            if (_selectedTransaction == null) return;
            var window = new Views.TransactionDetailWindow(_selectedTransaction);
            window.ShowDialog();
        }

        // ── Walidacja ─────────────────────────────────────────────────────────────
        private void ValidateAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                AmountError = "Kwota jest wymagana";
                HasAmountError = true;
                return;
            }

            if (!TryParseAmount(text, out decimal value))
            {
                AmountError = "Niepoprawny format (użyj np. 12,50 lub 12.50)";
                HasAmountError = true;
                return;
            }

            if (value <= 0)
            {
                AmountError = "Kwota musi być większa od zera";
                HasAmountError = true;
                return;
            }

            if (value > 1_000_000)
            {
                AmountError = "Kwota nie może przekraczać 1 000 000 zł";
                HasAmountError = true;
                return;
            }

            NewTransaction.Amount = value;
            AmountError = string.Empty;
            HasAmountError = false;
        }

        private static bool TryParseAmount(string text, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string normalized = text.Trim().Replace(',', '.');
            return decimal.TryParse(normalized,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out result);
        }

        // Statystyki liczone z przekazanej kolekcji (przefiltrowanej lub pełnej)
        private void UpdateSummary(IEnumerable<TransactionItem> source)
        {
            var list = source.ToList();
            IncomeSummary = list.Where(t => t.Type == "Przychód").Sum(t => t.Amount);
            ExpensesSummary = list.Where(t => t.Type == "Wydatek").Sum(t => t.Amount);
            Balance = IncomeSummary - ExpensesSummary;

            SummaryLabel = (_selectedMonth == null || _selectedMonth.IsAll)
                ? "wszystkie miesiące"
                : _selectedMonth.Label.ToLower();
        }
    }

    // ── Klasa pomocnicza miesięcy ──────────────────────────────────────────────
    public class MonthOption
    {
        public static readonly MonthOption All = new MonthOption
        {
            Label = "Wszystkie miesiące",
            IsAll = true,
            Year = 0,
            Month = 0
        };

        public string Label { get; private set; } = string.Empty;
        public int Year { get; private set; }
        public int Month { get; private set; }
        public bool IsAll { get; private set; }

        private MonthOption() { }

        public MonthOption(DateTime date)
        {
            Year = date.Year;
            Month = date.Month;
            IsAll = false;
            var pl = new System.Globalization.CultureInfo("pl-PL");
            Label = date.ToString("MMMM yyyy", pl);
            Label = char.ToUpper(Label[0]) + Label.Substring(1);
        }

        public override string ToString() => Label;

        public override bool Equals(object? obj) =>
            obj is MonthOption o && IsAll == o.IsAll && Year == o.Year && Month == o.Month;

        public override int GetHashCode() => HashCode.Combine(IsAll, Year, Month);
    }
}