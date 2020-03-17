using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBot.Models.DataBase
{
    public class BotDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<Revenue> Revenues { get; set; }
        public BotDbContext()
        {
            Database.EnsureCreated();
        }
        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
        {
            
           Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("dbsettings.json");
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
            return GetCategories(userId, categoryType).Where(r => r.Name == name).Single();
        }

        public string GetCategoryEmoji(string categoryName, CategoryType categoryType)
        {
            var category = Categories.Where(r => r.Name == categoryName && r.CategoryType == categoryType).FirstOrDefault();
            if (category == null)
            {
                return "";
            }
            return category.GetImage();
        }

        public bool ContainsCategory(long userId, string categoryName, CategoryType categoryType)
        {
            var userCategories = GetCategories(userId, categoryType);
            foreach (var category in userCategories)
            {
                if (category.Name.ToLower() == categoryName.ToLower())
                {
                    return true;
                }
            }
            return false;
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
    }
}

