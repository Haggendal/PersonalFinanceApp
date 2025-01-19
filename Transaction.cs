using System;

namespace PersonalFinanceApp
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public Transaction(DateTime date, decimal amount, string type, string description)
        {
            Date = date;
            Amount = amount;
            Type = type;
            Description = description;
        }
    }
}