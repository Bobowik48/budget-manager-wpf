using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BudgetManager.Infrastructure;
using BudgetManager.Models;

namespace BudgetManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // Specjalna lista z WPF, która sama odświeża DataGrid po dodaniu/usunięciu elementu
        public ObservableCollection<TransactionItem> Transactions { get; set; }

        // Obiekt powiązany z polami formularza
        private TransactionItem _newTransaction;
        public TransactionItem NewTransaction
        {
            get => _newTransaction;
            set { _newTransaction = value; OnPropertyChanged(); }
        }

        // Obiekt kliknięty/zaznaczony na liście w DataGrid
        private TransactionItem _selectedTransaction;
        public TransactionItem SelectedTransaction
        {
            get => _selectedTransaction;
            set { _selectedTransaction = value; OnPropertyChanged(); }
        }

        // --- Statystyki (Podsumowanie) ---
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

        // --- Komendy (Zastępują eventy Click) ---
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<TransactionItem>();
            LoadTestData();

            // Inicjalizacja komend i podpięcie funkcji
            AddCommand = new RelayCommand(AddTransaction, CanAddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, CanDeleteTransaction);
            ClearCommand = new RelayCommand(ClearForm);

            // Przygotowanie czystego formularza na start i obliczenie bilansu
            ClearForm(null);
            UpdateSummary();
        }

        private void LoadTestData()
        {
            // Możesz tu przekleić resztę swoich testowych danych
            Transactions.Add(new TransactionItem { Date = new DateTime(2026, 3, 1), Type = "Przychód", Category = "Wynagrodzenie", Amount = 5200.00m, Description = "Wynagrodzenie za marzec" });
            Transactions.Add(new TransactionItem { Date = new DateTime(2026, 3, 2), Type = "Wydatek", Category = "Zakupy spożywcze", Amount = 185.40m, Description = "Zakupy w supermarkecie" });
        }

        // Logika biznesowa - kiedy przycisk "Dodaj" ma być klikalny?
        private bool CanAddTransaction(object parameter)
        {
            return NewTransaction != null && NewTransaction.Amount > 0 && !string.IsNullOrWhiteSpace(NewTransaction.Description);
        }

        // Akcja dodawania
        private void AddTransaction(object parameter)
        {
            Transactions.Add(new TransactionItem
            {
                Date = NewTransaction.Date,
                Type = NewTransaction.Type,
                Category = NewTransaction.Category,
                Amount = NewTransaction.Amount,
                Description = NewTransaction.Description
            });

            UpdateSummary();
            ClearForm(null);
        }

        // Kiedy przycisk "Usuń" ma być klikalny? (tylko jak coś jest zaznaczone)
        private bool CanDeleteTransaction(object parameter)
        {
            return SelectedTransaction != null;
        }

        // Akcja usuwania
        private void DeleteTransaction(object parameter)
        {
            if (SelectedTransaction != null)
            {
                Transactions.Remove(SelectedTransaction);
                UpdateSummary();
            }
        }

        // Akcja czyszczenia formularza
        private void ClearForm(object parameter)
        {
            NewTransaction = new TransactionItem
            {
                Date = DateTime.Today,
                Type = "Wydatek",
                Category = "Zakupy spożywcze",
                Amount = 0,
                Description = string.Empty
            };
        }

        // Przeliczanie hajsu
        private void UpdateSummary()
        {
            IncomeSummary = Transactions.Where(t => t.Type == "Przychód").Sum(t => t.Amount);
            ExpensesSummary = Transactions.Where(t => t.Type == "Wydatek").Sum(t => t.Amount);
            Balance = IncomeSummary - ExpensesSummary;
        }
    }
}