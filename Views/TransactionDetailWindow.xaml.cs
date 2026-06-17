using BudgetManager.Models;
using System.Windows;

namespace BudgetManager.Views
{
    public partial class TransactionDetailWindow : Window
    {
        public TransactionDetailWindow(TransactionItem transaction)
        {
            InitializeComponent();
            // Wszystkie Bindingi w XAML działają automatycznie przez DataContext
            DataContext = transaction;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}