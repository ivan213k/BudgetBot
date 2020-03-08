using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBot.Models.Statistics
{
    public class StatisticsManager
    {
        private BotDbContext dbContext = new BotDbContext();
        public List<ExpenseStatistic> GetExpensesStatistic(long userId)
        {
            var expenses = dbContext.GetExpenses(userId);
            decimal totalAmount = GetTotalAmountOfExpenses(userId);
            List<ExpenseStatistic> expenseStatistics = new List<ExpenseStatistic>();
            foreach (var category in expenses.Select(r => r.Category.Name).Distinct())
            {
                var categoryAmount = expenses.Where(r => r.Category.Name == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / expenses.Select(r => r.Amount).Sum(), 0);
                expenseStatistics.Add(new ExpenseStatistic(category,categoryAmount,percent));
            }
            
            return expenseStatistics.OrderByDescending(r=>r.TotalAmount).ToList();
        }
        public List<RevenueStatistic> GetRevenuesStatistic(long userId)
        {
            var revenues = dbContext.GetRevenues(userId);
            decimal totalAmount = GetTotalAmountOfRevenues(userId);
            List<RevenueStatistic> revenueStatistics = new List<RevenueStatistic>();
            foreach (var category in revenues.Select(r => r.Category).Distinct())
            {
                var categoryAmount = revenues.Where(r => r.Category == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / revenues.Select(r => r.Amount).Sum(), 0);
                revenueStatistics.Add(new RevenueStatistic(category.Name, categoryAmount, percent));
            }

            return revenueStatistics.OrderByDescending(r => r.TotalAmount).ToList();
        }

        public decimal GetTotalAmountOfExpenses(long userId)
        {
            return dbContext.GetExpenses(userId).Select(r => r.Amount).Sum();
        }
        public decimal GetTotalAmountOfRevenues(long userId)
        {
            return dbContext.GetRevenues(userId).Select(r => r.Amount).Sum();
        }
    }
}
