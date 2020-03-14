using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static BudgetBot.Models.StateData.State;

namespace BudgetBot.Models.Commands
{
    public class ExpensesStatisticCommand : BaseCommand
    {
        public override string Name { get => "/getexpensestat"; }

        private BotDbContext dbContext = new BotDbContext();

        private IFormatProvider culture = new CultureInfo("Uk-ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (GetCurrentStep(userId) == 0)
            {
                var answer = GetExpenseStatisticText(userId);
                await client.SendTextMessageAsync(chatId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 1)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    SetCurrentDate(userId);
                    if (update.CallbackQuery.Data == "left")
                    {
                        PreviousMonth(userId);
                        var startDate = new DateTime(currentDates[userId].Year, currentDates[userId].Month, 01);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        var answer = GetExpenseStatisticText(userId,startDate,endDate);
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                        return;
                    }
                    if (update.CallbackQuery.Data == "right")
                    {
                        if (currentDates[userId].Date.AddMonths(1)>DateTime.Now.AddMonths(1))
                        {
                            return;
                        }
                        if (currentDates[userId].Date.AddMonths(1) > DateTime.Now)
                        {
                            var answer = GetExpenseStatisticText(userId);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            NextMonth(userId);
                        }
                        else
                        {
                            NextMonth(userId);
                            var startDate = new DateTime(currentDates[userId].Year, currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                            var answer = GetExpenseStatisticText(userId,startDate,endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                        }
                    }
                }
            }
        }

        private Dictionary<long, DateTime> currentDates = new Dictionary<long, DateTime>();

        private string GetExpenseStatisticText(long userId, DateTime? startDate=null, DateTime? endDate=null)
        {
            var statisticManager = new StatisticsManager();
            List<ExpenseStatistic> expenseStatistics;
            decimal totalAmount;
            string period;
            if (startDate == null || endDate == null)
            {
                expenseStatistics = statisticManager.GetExpensesStatistic(userId);
                totalAmount = statisticManager.GetTotalAmountOfExpenses(userId);
                period = "весь час";
            }
            else
            {
                expenseStatistics = statisticManager.GetExpensesStatistic(userId,startDate.Value, endDate.Value);
                totalAmount = statisticManager.GetTotalAmountOfExpenses(userId, startDate.Value, endDate.Value);
                period = startDate.Value.Year < DateTime.Now.Year ? startDate.Value.ToString("MMMM", culture) + " " + startDate.Value.Year 
                    : startDate.Value.ToString("MMMM", culture);
            }
            var downChart = new Emoji(0x1F4C9);
            StringBuilder answer = new StringBuilder($"{downChart} Статистика витрат по категоріям за <b>{period}</b>:\n");
            foreach (var row in expenseStatistics)
            {
                var categoryEmoji = dbContext.GetCategoryEmoji(row.Categrory, CategoryType.Expense);
                answer.Append($"\t\t\t{categoryEmoji} {row.Categrory} - {row.TotalAmount} ({row.Percent}%)\n");
            }
            answer.Append($"Загальна сума витрат: <u><b>{totalAmount}</b></u>");
            return answer.ToString();
        }
        private void NextMonth(long userId)
        {
            if (currentDates[userId].Date.AddMonths(1) <= DateTime.Now.AddMonths(1))
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
                currentDates.Add(userId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddMonths(1));
            }
        }
    }
}
