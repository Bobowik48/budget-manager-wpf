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
        public ICommand EditCommand { get; }
        public ICommand PreviewCommand { get; }

        // Zmienna, która zapamięta, którą transakcję właśnie edytujemy
        private TransactionItem _editingTransaction;

        public MainViewModel()
        {
            Transactions = new ObservableCollection<TransactionItem>();
            LoadTestData();

            // Inicjalizacja komend i podpięcie funkcji
            AddCommand = new RelayCommand(AddTransaction, CanAddTransaction);
            DeleteCommand = new RelayCommand(DeleteTransaction, CanModifyTransaction);
            ClearCommand = new RelayCommand(ClearForm);
            EditCommand = new RelayCommand(EditTransaction, CanModifyTransaction);
            PreviewCommand = new RelayCommand(PreviewTransaction, CanModifyTransaction);

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
            var newTx = new TransactionItem
            {
                Date = NewTransaction.Date,
                Type = NewTransaction.Type,
                Category = NewTransaction.Category,
                Amount = NewTransaction.Amount,
                Description = NewTransaction.Description
            };

            if (_editingTransaction != null)
            {
                // TRYB EDYCJI: Podmieniamy stary wpis na nowy w tym samym miejscu na liście
                int index = Transactions.IndexOf(_editingTransaction);
                if (index != -1)
                {
                    Transactions[index] = newTx;
                }
                _editingTransaction = null; // Wyłączamy tryb edycji
            }
            else
            {
                // TRYB NORMALNY: Po prostu dodajemy nowy wpis na koniec
                Transactions.Add(newTx);
            }

            UpdateSummary();
            ClearForm(null);
        }

        // Kiedy przycisk "Usuń" ma być klikalny? (tylko jak coś jest zaznaczone)
        private bool CanModifyTransaction(object parameter)
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
            _editingTransaction = null; // Czyszczenie resetuje też ewentualny stan edycji
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

        private void EditTransaction(object parameter)
        {
            if (SelectedTransaction == null) return;

            // Zapisujemy referencję do edytowanego elementu
            _editingTransaction = SelectedTransaction;

            // Kopiujemy dane zaznaczonego elementu do formularza
            NewTransaction = new TransactionItem
            {
                Date = SelectedTransaction.Date,
                Type = SelectedTransaction.Type,
                Category = SelectedTransaction.Category,
                Amount = SelectedTransaction.Amount,
                Description = SelectedTransaction.Description
            };
        }

        private void PreviewTransaction(object parameter)
        {
            if (SelectedTransaction != null)
            {
                // Proste okienko wyskakujące z detalami transakcji
                System.Windows.MessageBox.Show(
                    $"Data: {SelectedTransaction.Date:dd.MM.yyyy}\n" +
                    $"Typ: {SelectedTransaction.Type}\n" +
                    $"Kategoria: {SelectedTransaction.Category}\n" +
                    $"Kwota: {SelectedTransaction.Amount:N2} zł\n\n" +
                    $"Opis:\n{SelectedTransaction.Description}",
                    "Szczegóły transakcji",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}