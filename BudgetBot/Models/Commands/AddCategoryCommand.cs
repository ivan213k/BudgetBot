using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class AddCategoryCommand : Command
    {
        public override string Name { get => "/addcategory"; }

        private readonly List<Category> _userCategories  = new List<Category>();

        private readonly BotDbContext _dbContext = new BotDbContext();

        private void AddCategory(long userId,string name = null, CategoryType categoryType = default)
        {
            var category = _userCategories.FirstOrDefault(r => r.UserId == userId);
            if (category ==null)
            {
                _userCategories.Add(new Category(userId, name, categoryType));
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
            if (StateMachine.GetCurrentStep(userId)==0)
            {
                var revenueButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C8)} В доходи", "revenue");
                var expenseButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C9)} До витрат", "expense");
                
                await client.SendTextMessageAsync(chatId, "Куди саме додати категорію?", replyMarkup: Bot.MakeInlineKeyboard(revenueButton,expenseButton));
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId)==1)
            {
                if (update.CallbackQuery.Data == "revenue")
                {
                    AddCategory(userId, "", CategoryType.Revenue);
                }
                if (update.CallbackQuery.Data == "expense")
                {
                    AddCategory(userId, "", CategoryType.Expense);
                }
                await client.EditMessageTextAsync(chatId,messageId,"Введіть назву категорії");
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId)==2)
            {
                var category = _userCategories.Single(r => r.UserId == userId);
                category.Name = update.Message.Text;
                if (_dbContext.ContainsCategory(userId, category.Name, category.CategoryType))
                {
                    await client.SendTextMessageAsync(chatId, "Упс... така категорія вже існує, спробуйте ввести щось інше");
                    return;
                }
                _dbContext.Categories.Add(category);
                await _dbContext.SaveChangesAsync();
                var answer = $"{new Emoji(0x2705)} Категорію <u><b>{category.Name}</b></u> додано";
                answer += category.CategoryType == CategoryType.Expense ? " до витрат" : " в доходи";

                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html);
                _userCategories.Remove(category);
                StateMachine.FinishCurrentCommand(userId);
            }
        }
    }
}
