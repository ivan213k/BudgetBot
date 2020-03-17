using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            var messageId = GetMessageId(update);
            if (State.GetCurrentStep(userId)==0)
            {
                var revenueButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C8)} В доходи", "revenue");
                var expenseButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C9)} До витрат", "expense");
                
                await client.SendTextMessageAsync(chatId, "Куди саме додати категорію?", replyMarkup: Bot.MakeInlineKeyboard(revenueButton,expenseButton));
                State.NextStep(userId);
                return;
            }
            if (State.GetCurrentStep(userId)==1)
            {
                if (update.CallbackQuery.Data == "revenue")
                {
                    AddCategory(userId, "", categoryType: CategoryType.Revenue);
                }
                if (update.CallbackQuery.Data == "expense")
                {
                    AddCategory(userId, "", categoryType: CategoryType.Expense);
                }
                await client.EditMessageTextAsync(chatId,messageId,"Введіть назву категорії");
                State.NextStep(userId);
                return;
            }
            if (State.GetCurrentStep(userId)==2)
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
                var answer = $"{new Emoji(0x2705)} Категорію <u><b>{category.Name}</b></u> додано";
                answer += category.CategoryType == CategoryType.Expense ? " до витрат" : " в доходи";

                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html);
                UserCategories.Remove(category);
                State.FinishCurrentCommand(userId);
            }
        }
    }
}
