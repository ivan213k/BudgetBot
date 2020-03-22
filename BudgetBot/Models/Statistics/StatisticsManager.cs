using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBot.Models.Statistics
{
    public class StatisticsManager
    {
        private readonly BotDbContext _dbContext = new BotDbContext();
        public List<ExpenseStatistic> GetExpensesStatistic(long userId)
        {
            var expenses = _dbContext.GetExpenses(userId);
            var expenseStatistics = new List<ExpenseStatistic>();
            foreach (var category in expenses.Select(r => r.Category.Name).Distinct())
            {
                var categoryAmount = expenses.Where(r => r.Category.Name == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / expenses.Select(r => r.Amount).Sum(), 1);
                expenseStatistics.Add(new ExpenseStatistic(category,categoryAmount,percent));
            }
            
            return expenseStatistics.OrderByDescending(r=>r.TotalAmount).ToList();
        }
        public List<ExpenseStatistic> GetExpensesStatistic(long userId, DateTime startDate, DateTime endDate)
        {
            var expenses = _dbContext.GetExpenses(userId).Where(r=>r.Date >= startDate && r.Date <= endDate).ToList();
            var expensesStatistic = new List<ExpenseStatistic>();
            foreach (var category in expenses.Select(r => r.Category.Name).Distinct())
            {
                var categoryAmount = expenses.Where(r => r.Category.Name == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / expenses.Select(r => r.Amount).Sum(), 1);
                expensesStatistic.Add(new ExpenseStatistic(category, categoryAmount, percent));
            }
            return expensesStatistic.OrderByDescending(r => r.TotalAmount).ToList();
        }
        public List<RevenueStatistic> GetRevenuesStatistic(long userId)
        {
            var revenues = _dbContext.GetRevenues(userId);
            List<RevenueStatistic> revenueStatistics = new List<RevenueStatistic>();
            foreach (var category in revenues.Select(r => r.Category).Distinct())
            {
                var categoryAmount = revenues.Where(r => r.Category == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / revenues.Select(r => r.Amount).Sum(), 1);
                revenueStatistics.Add(new RevenueStatistic(category.Name, categoryAmount, percent));
            }

            return revenueStatistics.OrderByDescending(r => r.TotalAmount).ToList();
        }

        public List<RevenueStatistic> GetRevenuesStatistic(long userId, DateTime startDate, DateTime endDate)
        {
            var revenues = _dbContext.GetRevenues(userId).Where(r => r.Date >= startDate && r.Date <= endDate).ToList();
            var revenueStatistic = new List<RevenueStatistic>();
            foreach (var category in revenues.Select(r => r.Category.Name).Distinct())
            {
                var categoryAmount = revenues.Where(r => r.Category.Name == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / revenues.Select(r => r.Amount).Sum(), 1);
                revenueStatistic.Add(new RevenueStatistic(category, categoryAmount, percent));
            }

            return revenueStatistic.OrderByDescending(r => r.TotalAmount).ToList();
        }
        public decimal GetTotalAmountOfExpenses(long userId)
        {
            return _dbContext.GetExpenses(userId).Select(r => r.Amount).Sum();
        }
        public decimal GetTotalAmountOfExpenses(long userId,DateTime startDate, DateTime endDate)
        {
            return _dbContext.GetExpenses(userId).Where(r => r.Date >= startDate && r.Date <= endDate).Select(r => r.Amount).Sum();
        }
        public decimal GetTotalAmountOfRevenues(long userId)
        {
            return _dbContext.GetRevenues(userId).Select(r => r.Amount).Sum();
        }
        public decimal GetTotalAmountOfRevenues(long userId, DateTime startDate, DateTime endDate)
        {
            return _dbContext.GetRevenues(userId).Where(r => r.Date >= startDate && r.Date <= endDate).Select(r => r.Amount).Sum();
        }
    }
}
