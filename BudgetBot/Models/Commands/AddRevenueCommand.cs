using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Commands
{
    public class AddRevenueCommand : BaseCommand
    {
        public override string Name { get => "/addrevenue"; }

        private BotDbContext dbContext = new BotDbContext();
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            if (State.GetCurrentStep(userId) == 0)
            {
                await Bot.SendCategories(update.Message,"Виберіть категорію доходу\t", CategoryType.Revenue);
                State.NextStep(userId);
                return;
            }
            if (State.GetCurrentStep(userId) == 1)
            {
                AddRevenue(userId,category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId,"Введіть суму доходу");
                State.NextStep(userId);
                return;
            }
            if (State.GetCurrentStep(userId) == 2)
            {
                if (!decimal.TryParse(update.Message.Text, out decimal amount))
                {
                    await client.SendTextMessageAsync(chatId, "Упс..., введіть суму доходу");
                    return;
                }
                AddRevenue(userId, amount: amount, date: DateTime.Now);

                var revenue = userRevenues.Where(r => r.UserId == userId).Single();
                var successEmoji = new Emoji(0x2705);

                var selectDateButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C6)}Змінити дату", "selectDate");
                var cancelButton = Bot.MakeInlineButton($"{new Emoji(0x274C)}Скасувати", "cancel");

                var answer = $"{successEmoji} Дохід додано:\n" +
                    $"Категорія доходу - {revenue.Category.Name}\n" +
                    $"Сума - {revenue.Amount}$\n" +
                    $"Дата - {revenue.Date.ToShortDateString()}";

                await client.SendTextMessageAsync(chatId, answer, replyMarkup:Bot.MakeInlineKeyboard(selectDateButton,cancelButton));

                dbContext.Revenues.Add(revenue);
                await dbContext.SaveChangesAsync();
                userRevenues.Remove(revenue);
                State.FinishCurrentCommand(userId);
            }
        }

        private List<Revenue> userRevenues = new List<Revenue>();

        private void AddRevenue(long userId, string category = "", decimal amount = default, DateTime date = default)
        {
            var record = userRevenues.Where(r => r.UserId == userId).FirstOrDefault();
            if (record != null)
            {
                if (category != "")
                {
                    record.Category = dbContext.GetCategory(userId,category,CategoryType.Revenue);
                }
                if (amount != default)
                {
                    record.Amount = amount;
                }
                if (date != default)
                {
                    record.Date = date;
                }
            }
            else
            {
                userRevenues.Add(new Revenue(userId, dbContext.GetCategory(userId,category,CategoryType.Revenue), amount, date));
            }
        }
    }
}
