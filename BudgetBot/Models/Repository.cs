using System.Collections.Generic;
using System.Linq;

namespace BudgetBot.Models
{
    public static class Repository
    {
        static Repository()
        {
            ExpenseCategories = new List<Category>()
            {
                new Category("Їжа",isStandard: true),
                new Category("Одяг", isStandard: true),
                new Category("Електроніка", isStandard: true),
                new Category("Розваги", isStandard: true),
                new Category("Особистий догляд", isStandard: true),
                new Category("Транспорт", isStandard: true),
                new Category("Подорожі", isStandard: true),
                new Category("Лікування", isStandard: true),
                new Category("Будинок", isStandard: true),
            };
            RevenueCategories = new List<Category>()
            {
                new Category("Заробітня плата",isStandard:true),
                new Category("Премія",isStandard: true),
                new Category("Підробіток",isStandard: true)
            };
        }
        public static List<Category> ExpenseCategories { get; set; }

        public static List<Category> RevenueCategories { get; set; }

        public static List<Expense> Expenses { get; set; } = new List<Expense>();

        public static List<Revenue> Revenues { get; set; } = new List<Revenue>();

        public static List<Category> GetExpenseCategories(long userId)
        {
            return ExpenseCategories.Where(r => r.IsStandardCategory || r.UserId == userId).ToList();
        }
        public static List<Category> GetRevenueCategories(long userId)
        {
            return RevenueCategories.Where(r => r.IsStandardCategory || r.UserId == userId).ToList();
        }
    }
}
