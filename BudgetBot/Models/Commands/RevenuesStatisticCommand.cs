using BudgetBot.Models.DataBase;
using BudgetBot.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class RevenuesStatisticCommand : Command
    {
        public override string Name { get => "/getrevenuestat"; }

        private readonly BotDbContext _dbContext = new BotDbContext();

        private readonly IFormatProvider _culture = new CultureInfo("Uk-ua");

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            {
                var answer = GetRevenueStatisticText(userId);
                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 1)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    SetCurrentDate(userId);
                    switch (update.CallbackQuery.Data)
                    {
                        case "left":
                        {
                            PreviousMonth(userId);
                            var startDate = new DateTime(_currentDates[userId].Year, _currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                            var answer = GetRevenueStatisticText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            return;
                        }
                        case "right" when _currentDates[userId].Date.AddMonths(1) > DateTime.Now.AddMonths(1):
                            return;
                        case "right" when _currentDates[userId].Date.AddMonths(1) > DateTime.Now:
                        {
                            var answer = GetRevenueStatisticText(userId);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            NextMonth(userId);
                            break;
                        }
                        case "right":
                        {
                            NextMonth(userId);
                            var startDate = new DateTime(_currentDates[userId].Year, _currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                            var answer = GetRevenueStatisticText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            break;
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
                period = startDate.Value.Year < DateTime.Now.Year ? startDate.Value.ToString("MMMM", _culture) + " " + startDate.Value.Year
                    : startDate.Value.ToString("MMMM", _culture);
            }
            var topChart = new Emoji(0x1F4C8);
            StringBuilder answer = new StringBuilder($"{topChart} Статистика доходів по категоріям за <b>{period}</b>:\n");
            foreach (var row in revenueStatistics)
            {
                var categoryEmoji = _dbContext.GetCategoryEmoji(row.Category, CategoryType.Revenue);
                answer.Append($"\t\t\t{categoryEmoji} {row.Category} - {row.TotalAmount} ₴ ({row.Percent}%)\n");
            }
            answer.Append($"Загальна сума доходів: <u><b>{totalAmount} ₴</b></u>");
            return answer.ToString();
        }

        private readonly Dictionary<long, DateTime> _currentDates = new Dictionary<long, DateTime>();
        private void NextMonth(long userId)
        {
            if (_currentDates[userId].Date.AddMonths(1) <= DateTime.Now.AddMonths(1))
            {
                _currentDates[userId] = _currentDates[userId].Date.AddMonths(1);
            }
        }
        private void PreviousMonth(long userId)
        {
            _currentDates[userId] = _currentDates[userId].Date.AddMonths(-1);
        }
        private void SetCurrentDate(long userId)
        {
            if (!_currentDates.ContainsKey(userId))
            {
                _currentDates.Add(userId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddMonths(1));
            }
        }
    }
}
