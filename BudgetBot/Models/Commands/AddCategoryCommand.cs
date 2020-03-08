using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBot.Models.Commands
{
    public class AddCategoryCommand : BaseCommand
    {
        public override string Name { get => "/addcategory"; }

        List<Category> UserCategories { get; set; } = new List<Category>();

        private BotDbContext dbContext = new BotDbContext();
        void AddCategory(long userId,string name = null, CategoryType categoryType = default)
        {
            var category = UserCategories.Where(r=>r.UserId== userId).FirstOrDefault();
            if (category ==null)
            {
                UserCategories.Add(new Category(userId, name, categoryType));
            }
            else
            {
                if (category.Name!=null)
                {
                    category.Name = name;
                }
                if (categoryType!=default)
                {
                    category.CategoryType = categoryType;
                }
            }
        }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            InitCommandSteps(userId);
            if (GetCurrentStep(userId)==0)
            {
                State.AddCurrentCommand(userId,Name);
                var revenueButton = new InlineKeyboardButton();
                revenueButton.Text = new Emoji(0x1F4C8).ToString() + " В доходи";
                revenueButton.CallbackData = "revenue";

                var expenseButton = new InlineKeyboardButton();
                expenseButton.Text = new Emoji(0x1F4C9).ToString() + " До витрат";
                expenseButton.CallbackData = "expense";
                var buttons = new List<InlineKeyboardButton>()
                {
                    revenueButton,
                    expenseButton
                };
                var keyboard = new InlineKeyboardMarkup(buttons);
                await client.SendTextMessageAsync(chatId, "Куди саме додати категорію?", replyMarkup: keyboard);
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)==1)
            {
                if (update.CallbackQuery.Data == "revenue")
                {
                    AddCategory(userId, "", categoryType: CategoryType.Revenue);
                }
                if (update.CallbackQuery.Data == "expense")
                {
                    AddCategory(userId, "", categoryType: CategoryType.Expense);
                }
                await client.SendTextMessageAsync(chatId,"Введіть назву категорії");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)==2)
            {
                var category = UserCategories.Where(r => r.UserId == userId).Single();
                category.Name = update.Message.Text;
                if (dbContext.ContainsCategory(userId, category.Name,category.CategoryType))
                {
                    await client.SendTextMessageAsync(chatId, "Упс... така категорія вже існує, спробуйте ввести щось інше");
                    return;
                }
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
                var successEmoji = new Emoji(0x2705);
                var answer = $" Категорію <u><b>{category.Name}</b></u> додано";
                answer += category.CategoryType == CategoryType.Expense ? " до витрат" : " в доходи";

                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html);
                UserCategories.Remove(category);
                State.FinishCurrentCommand(userId);
                ResetCommandSteps(userId);
            }
        }
    }
}
