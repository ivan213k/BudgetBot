using System;

namespace BudgetBot.Models
{
    public class Revenue
    {
        public long UserId { get; set; }

        public string Category { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public Revenue(long userId, string category, decimal amount, DateTime date)
        {
            UserId = userId;
            Category = category;
            Amount = amount;
            Date = date;
        }
    }
}
