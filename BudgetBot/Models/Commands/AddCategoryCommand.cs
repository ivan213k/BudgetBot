using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBot.Models.Command;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBot.Models.Commands
{
    public class AddCategoryCommand : BaseCommand
    {
        public override string Name { get => "/addcategory"; }

        List<Category> UserCategories { get; set; } = new List<Category>();

        void AddCategory(long userId,string name)
        {
            var category = UserCategories.Where(r=>r.UserId== userId).FirstOrDefault();
            if (category ==null)
            {
                UserCategories.Add(new Category(userId, name));
            }
            else
            {
                category.Name = name;
            }
        }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            InitCommandSteps(userId);
            if (GetCurrentStep(userId)==0)
            {
                State.AddCurrentCommand(userId,Name);
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введіть назву категорії");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)==1)
            {
                AddCategory(userId, update.Message.Text);
                var revenueButton = new InlineKeyboardButton();
                revenueButton.Text = "В доходи";
                revenueButton.CallbackData = "revenue";
                var expenseButton = new InlineKeyboardButton();
                expenseButton.Text = "До витрат";
                expenseButton.CallbackData = "expense";
                var buttons = new List<InlineKeyboardButton>()
                {
                    revenueButton,
                    expenseButton
                };
                var keyboard = new InlineKeyboardMarkup(buttons);
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Куди саме додати категорію?", replyMarkup: keyboard);
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)==2)
            {
                var category = UserCategories.Where(r => r.UserId == userId).Single();
                if (update.CallbackQuery.Data == "revenue")
                {
                    Repository.RevenueCategories.Add(category);
                    await client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Категорію \"{category.Name}\" додано в доходи");
                }
                if (update.CallbackQuery.Data == "expense")
                {
                    Repository.ExpenseCategories.Add(category);
                    await client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Категорію \"{category.Name}\" додано до виитрат");
                }
                UserCategories.Remove(category);
                State.FinishCurrentCommand(userId);
                ResetCommandSteps(userId);
            }
        }
    }
}
