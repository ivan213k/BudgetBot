using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetBot.Models.DataBase
{
    public class Revenue
    {
        public int Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public Revenue()
        {
               
        }
        public Revenue(long userId, Category category, decimal amount, DateTime date)
        {
            UserId = userId;
            Category = category;
            Amount = amount;
            Date = date;
        }
    }
}
