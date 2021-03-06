﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBot.Models.DataBase
{
    public sealed class BotDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<Revenue> Revenues { get; set; }
        public BotDbContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("botsettings.json");
            var config = builder.Build();
            #if DEBUG
            optionsBuilder.UseSqlServer(config.GetConnectionString("LocalConnection"));
            #else
                optionsBuilder.UseSqlServer(config.GetConnectionString("RemoteConnection"));
            #endif
        }

        public List<Category> GetCategories(long userId, CategoryType categoryType)
        {
            return Categories.Where(r => (r.IsStandardCategory || r.UserId == userId) && r.CategoryType == categoryType).
                ToList();
        }

        public Category GetCategory(long userId, string name, CategoryType categoryType)
        {
            return GetCategories(userId, categoryType).Single(r => r.Name == name);
        }

        public string GetCategoryEmoji(string categoryName, CategoryType categoryType)
        {
            var category = Categories.FirstOrDefault(r => r.Name == categoryName && r.CategoryType == categoryType);
            return category == null ? "" : category.GetImage();
        }

        public bool ContainsCategory(long userId, string categoryName, CategoryType categoryType)
        {
            var userCategories = GetCategories(userId, categoryType);
            return userCategories.Any(category => string.Equals(category.Name, categoryName, StringComparison.CurrentCultureIgnoreCase));
        }

        public List<Expense> GetExpenses(long userId)
        {
            return Expenses.Where(r => r.UserId == userId).Include(r => r.Category).ToList();
        }
        public List<Expense> GetExpenses(long userId, DateTime startDate, DateTime endDate)
        {
            return Expenses.Where(r => r.UserId == userId
            &&(r.Date>=startDate&& r.Date<=endDate)).Include(r => r.Category).ToList();
        }

        public List<Revenue> GetRevenues(long userId)
        {
            return Revenues.Where(r => r.UserId == userId).Include(r => r.Category).ToList();
        }

        public List<Revenue> GetRevenues(long userId, DateTime startDate, DateTime endDate)
        {
            return Revenues.Where(r => r.UserId == userId
            &&(r.Date >= startDate && r.Date <= endDate)).Include(r => r.Category).ToList();
        }

        public async Task DeleteAllRecords(long userId)
        {
            Expenses.RemoveRange(Expenses.Where(r=>r.UserId==userId));
            Revenues.RemoveRange(Revenues.Where(r=>r.UserId==userId));
            Categories.RemoveRange(Categories.Where(r=>!r.IsStandardCategory&&r.UserId==userId));
            await SaveChangesAsync();
        }
    }
}

