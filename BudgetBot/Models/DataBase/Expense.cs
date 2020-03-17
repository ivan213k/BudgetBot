using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BudgetBot.Models.DataBase
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Description { get; set; }
        public Expense()
        {

        }
        public Expense(long userId, Category category, decimal amount, DateTime date)
        {
            UserId = userId;
            Category = category;
            Amount = Amount;
            Date = Date;
        }
        public Expense(long userId, Category category, decimal amount, DateTime date, string descripton)
            : this(userId, category, amount, date)
        {
            Description = descripton;
        }

        public override string ToString()
        {
            return $"{Category.Emoji} {Date.ToString("dd.MM.yyyy",new CultureInfo("uk-ua"))} {Category.Name} - {Amount}₴";
        }
    }
}
