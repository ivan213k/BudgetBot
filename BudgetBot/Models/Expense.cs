using System;

namespace BudgetBot.Models
{
    public class Expense
    {
        public Expense(long userId, string category, decimal amount, DateTime date)
        {
            UserId = userId;
            Category = category;
            Amount = Amount;
            Date = Date;
        }
        public Expense(long userId, string category, decimal amount, DateTime date, string descripton)
            :this(userId,category,amount,date)
        {
            Description = descripton;
        }
        public long UserId { get; set; }
        public decimal Amount { get; set; }

        public string Category { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }
      
    }
}
