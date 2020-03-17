using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static BudgetBot.Models.StateData.State;

namespace BudgetBot.Models.Commands
{
    public class GetExpensesListCommand : BaseCommand
    {
        public override string Name { get => "/expenseslist"; }

        private BotDbContext dbContext = new BotDbContext();

        private IFormatProvider culture = new CultureInfo("Uk-ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (GetCurrentStep(userId) == 0)
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                var answer = GetExpensesListText(userId,startDate,endDate);
                await client.SendTextMessageAsync(chatId,answer,parseMode:ParseMode.Html,replyMarkup:Bot.MakeDateSwichKeyboard());
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId)== 1)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    SetCurrentDate(userId);
                    if (update.CallbackQuery.Data == "left")
                    {
                        PreviousMonth(userId);
                        var startDate = new DateTime(currentDates[userId].Year, currentDates[userId].Month, 01);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        var answer = GetExpensesListText(userId, startDate, endDate);
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                        return;
                    }
                    if (update.CallbackQuery.Data == "right")
                    {
                        if (currentDates[userId].Date.AddMonths(1) > DateTime.Now)
                        {
                            return;
                        }
                        else
                        {
                            NextMonth(userId);
                            var startDate = new DateTime(currentDates[userId].Year, currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                            var answer = GetExpensesListText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            return;
                        }
                    }
                }
            }
        }

        private string GetExpensesListText(long userId, DateTime startDate, DateTime endDate)
        {
            List<Expense> expenses = dbContext.GetExpenses(userId, startDate, endDate).OrderByDescending(r=>r.Date).ToList();
            StringBuilder stringBuilder = new StringBuilder();
            string period = startDate.Year == DateTime.Now.Year ? startDate.ToString("MMMM", culture) : startDate.ToString("MMMM yyyy", culture);
            stringBuilder.Append($"{new Emoji(0x1F4DC)} Список витрат за <u><b>{period}</b></u>\n");
            foreach (var expense in expenses)
            {
                stringBuilder.Append(expense.ToString()+"\n");
            }
            return stringBuilder.ToString();
        }

        private Dictionary<long, DateTime> currentDates = new Dictionary<long, DateTime>();
        private void NextMonth(long userId)
        {
            if (currentDates[userId].Date.AddMonths(1) <= DateTime.Now)
            {
                currentDates[userId] = currentDates[userId].Date.AddMonths(1);
            }
        }
        private void PreviousMonth(long userId)
        {
            currentDates[userId] = currentDates[userId].Date.AddMonths(-1);
        }
        private void SetCurrentDate(long userId)
        {
            if (!currentDates.ContainsKey(userId))
            {
                currentDates.Add(userId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
            }
        }
    }
}
