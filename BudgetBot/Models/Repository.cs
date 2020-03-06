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
                new Category("Їжа",isStandard: true, new Emoji(0x1F374)),
                new Category("Одяг", isStandard: true, new Emoji(0x1F455)),
                new Category("Електроніка", isStandard: true, new Emoji(0x1F50C)),
                new Category("Розваги", isStandard: true, new Emoji(0x1F3C4)),
                new Category("Особистий догляд", isStandard: true, new Emoji(0x1F6C0)),
                new Category("Транспорт", isStandard: true, new Emoji(0x1F68C)),
                new Category("Подорожі", isStandard: true, new Emoji(0x1F42B)),
                new Category("Лікування", isStandard: true, new Emoji(0x1F48A)),
                new Category("Будинок", isStandard: true, new Emoji(0x1F3E1)),
            };
            RevenueCategories = new List<Category>()
            {
                new Category("Заробітня плата",isStandard:true, new Emoji(0x1F4B5)),
                new Category("Премія",isStandard: true, new Emoji(0x1F4B8)),
                new Category("Підробіток",isStandard: true, new Emoji(0x1F4B0))
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
