using BudgetManager.Infrastructure;
using System;

namespace BudgetManager.Models
{
    public class TransactionItem : ObservableObject
    {
        private int _id;
        private DateTime _date;
        private string _type = string.Empty;
        private string _category = string.Empty;
        private decimal _amount;
        private string _description = string.Empty;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
    }
}