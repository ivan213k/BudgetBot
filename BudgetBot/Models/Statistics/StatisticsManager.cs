using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBot.Models.Statistics
{
    public class StatisticsManager
    {
        public List<ExpenseStatistic> GetExpensesStatistic(long userId)
        {
            var expenses = Repository.Expenses.Where(r => r.UserId == userId).ToList();
            decimal totalAmount = expenses.Select(r => r.Amount).Sum();
            List<ExpenseStatistic> expenseStatistics = new List<ExpenseStatistic>();
            foreach (var category in expenses.Select(r => r.Category).Distinct())
            {
                var categoryAmount = expenses.Where(r => r.Category == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / expenses.Select(r => r.Amount).Sum(), 0);
                expenseStatistics.Add(new ExpenseStatistic(category,categoryAmount,percent));
            }
            
            return expenseStatistics.OrderByDescending(r=>r.TotalAmount).ToList();
        }
        public List<RevenueStatistic> GetRevenuesStatistic(long userId)
        {
            var revenues = Repository.Revenues.Where(r => r.UserId == userId).ToList();
            decimal totalAmount = revenues.Select(r => r.Amount).Sum();
            List<RevenueStatistic> revenueStatistics = new List<RevenueStatistic>();
            foreach (var category in revenues.Select(r => r.Category).Distinct())
            {
                var categoryAmount = revenues.Where(r => r.Category == category).Select(r => r.Amount).Sum();
                var percent = Math.Round(categoryAmount * 100 / revenues.Select(r => r.Amount).Sum(), 0);
                revenueStatistics.Add(new RevenueStatistic(category, categoryAmount, percent));
            }

            return revenueStatistics.OrderByDescending(r => r.TotalAmount).ToList();
        }

        public decimal GetTotalAmountOfExpenses(long userId)
        {
            var expenses = Repository.Expenses.Where(r => r.UserId == userId).ToList();
            return expenses.Select(r => r.Amount).Sum();
        }
        public decimal GetTotalAmountOfRevenues(long userId)
        {
            var revenues = Repository.Revenues.Where(r => r.UserId == userId).ToList();
            return revenues.Select(r => r.Amount).Sum();
        }
    }
}
