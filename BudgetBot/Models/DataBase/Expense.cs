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
            Amount = amount;
            Date = date;
        }
        public Expense(long userId, Category category, decimal amount, DateTime date, string description)
            : this(userId, category, amount, date)
        {
            Description = description;
        }

        public override string ToString()
        {
            string expense;
            if (string.IsNullOrWhiteSpace(Description))
            {
                expense = $"{Category.Emoji} {Date.ToString("dd.MM.yyyy", new CultureInfo("uk-ua"))} {Category.Name} - {Amount}₴";
            }
            else
            {
                expense = $"{Category.Emoji} {Date.ToString("dd.MM.yyyy", new CultureInfo("uk-ua"))} {Description} - {Amount}₴";
            }
            return expense;
        }
    }
}
