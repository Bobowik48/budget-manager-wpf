using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace BudgetManager
{
    public partial class MainWindow : Window
    {
        private List<TransactionItem> transactions;

        public MainWindow()
        {
            InitializeComponent();
            LoadTestData();
            LoadSummary();
            SetDefaultFormValues();
        }
        private void LoadTestData()
        {
            transactions = new List<TransactionItem>
            {
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 1),
                    Type = "Przychód",
                    Category = "Wynagrodzenie",
                    Amount = 5200.00m,
                    Description = "Wynagrodzenie za marzec"
                },
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 2),
                    Type = "Wydatek",
                    Category = "Zakupy spożywcze",
                    Amount = 185.40m,
                    Description = "Zakupy w supermarkecie"
                },
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 3),
                    Type = "Wydatek",
                    Category = "Transport",
                    Amount = 120.00m,
                    Description = "Paliwo"
                },
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 5),
                    Type = "Wydatek",
                    Category = "Sport",
                    Amount = 99.99m,
                    Description = "Karnet na siłownię"
                },
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 6),
                    Type = "Wydatek",
                    Category = "Rozrywka",
                    Amount = 45.00m,
                    Description = "Kino"
                },
                new TransactionItem
                {
                    Date = new System.DateTime(2026, 3, 8),
                    Type = "Przychód",
                    Category = "Inne",
                    Amount = 250.00m,
                    Description = "Zwrot od znajomego"
                }
            };

            TransactionsDataGrid.ItemsSource = transactions;
        }

        private void LoadSummary()
        {
            decimal income = transactions
                .Where(transaction => transaction.Type == "Przychód")
                .Sum(transaction => transaction.Amount);

            decimal expenses = transactions
                .Where(transaction => transaction.Type == "Wydatek")
                .Sum(transaction => transaction.Amount);

            decimal balance = income - expenses;

            IncomeTextBlock.Text = income.ToString("N2") + " zł";
            ExpensesTextBlock.Text = expenses.ToString("N2") + " zł";
            BalanceTextBlock.Text = balance.ToString("N2") + " zł";
        }

        private void SetDefaultFormValues()
        {
            TransactionDatePicker.SelectedDate = System.DateTime.Today;
            TypeComboBox.SelectedIndex = 1;
            CategoryComboBox.SelectedIndex = 0;
        }
    }
}