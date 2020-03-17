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
    public class RevenuesStatisticCommand : BaseCommand
    {
        public override string Name { get => "/getrevenuestat"; }

        private BotDbContext dbContext = new BotDbContext();

        private IFormatProvider culture = new CultureInfo("Uk-ua");

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (GetCurrentStep(userId) == 0)
            {
                var answer = GetRevenueStatisticText(userId);
                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
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
                        var answer = GetRevenueStatisticText(userId, startDate, endDate);
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                        return;
                    }
                    if (update.CallbackQuery.Data == "right")
                    {
                        if (currentDates[userId].Date.AddMonths(1) > DateTime.Now.AddMonths(1))
                        {
                            return;
                        }
                        if (currentDates[userId].Date.AddMonths(1) > DateTime.Now)
                        {
                            var answer = GetRevenueStatisticText(userId);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            NextMonth(userId);
                        }
                        else
                        {
                            NextMonth(userId);
                            var startDate = new DateTime(currentDates[userId].Year, currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                            var answer = GetRevenueStatisticText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                        }
                    }
                }
            }
        }
        private string GetRevenueStatisticText(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var statisticManager = new StatisticsManager();
            List<RevenueStatistic> revenueStatistics;
            decimal totalAmount;
            string period;
            if (startDate == null || endDate == null)
            {
                revenueStatistics = statisticManager.GetRevenuesStatistic(userId);
                totalAmount = statisticManager.GetTotalAmountOfRevenues(userId);
                period = "весь час";
            }
            else
            {
                revenueStatistics = statisticManager.GetRevenuesStatistic(userId, startDate.Value, endDate.Value);
                totalAmount = statisticManager.GetTotalAmountOfRevenues(userId, startDate.Value, endDate.Value);
                period = startDate.Value.Year < DateTime.Now.Year ? startDate.Value.ToString("MMMM", culture) + " " + startDate.Value.Year
                    : startDate.Value.ToString("MMMM", culture);
            }
            var topChart = new Emoji(0x1F4C8);
            StringBuilder answer = new StringBuilder($"{topChart} Статистика доходів по категоріям за <b>{period}</b>:\n");
            foreach (var row in revenueStatistics)
            {
                var categoryEmoji = dbContext.GetCategoryEmoji(row.Categrory, CategoryType.Revenue);
                answer.Append($"\t\t\t{categoryEmoji} {row.Categrory} - {row.TotalAmount} ₴ ({row.Percent}%)\n");
            }
            answer.Append($"Загальна сума доходів: <u><b>{totalAmount} ₴</b></u>");
            return answer.ToString();
        }

        private Dictionary<long, DateTime> currentDates = new Dictionary<long, DateTime>();
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
