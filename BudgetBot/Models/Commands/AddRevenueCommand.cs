using BudgetBot.Models.Command;
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

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            InitCommandSteps(userId);
            if (GetCurrentStep(userId)==0)
            {
                State.AddCurrentCommand(userId, Name);
                await Bot.SendRevenueCategories(update.Message,"Виберіть категорію доходу\t");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)==1)
            {
                AddRevenue(userId,category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId,"Введіть суму доходу");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 2)
            {
                if (!decimal.TryParse(update.Message.Text, out decimal amount))
                {
                    await client.SendTextMessageAsync(chatId, "Упс..., введіть суму доходу");
                    return;
                }
                AddRevenue(userId, amount: amount, date: DateTime.Now);

                var revenue = userRevenues.Where(r => r.UserId == userId).Single();
                var successEmoji = new Emoji(0x2705);
                await client.SendTextMessageAsync(chatId, successEmoji + $" Дохід додано:\n" +
                    $"Категорія доходу - {revenue.Category}\n" +
                    $"Сума - {revenue.Amount}$\n" +
                    $"Дата - {revenue.Date.ToShortDateString()}");
                Repository.Revenues.Add(revenue);
                userRevenues.Remove(revenue);
                State.FinishCurrentCommand(userId);
                ResetCommandSteps(userId);
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
                    record.Category = category;
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
                userRevenues.Add(new Revenue(userId, category, amount, date));
            }
        }
    }
}
