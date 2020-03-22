using BudgetBot.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class GetBalanceCommand : Command
    {
        public override string Name { get => "/balance"; }

        private readonly IFormatProvider _culture = new CultureInfo("Uk-ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            { 
                var answer = GetBalanceText(userId);
                await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeDateSwichKeyboard(), parseMode:ParseMode.Html);
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
                            var answer = GetBalanceText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            return;
                        }
                        case "right" when _currentDates[userId].Date.AddMonths(1) > DateTime.Now.AddMonths(1):
                            return;
                        case "right" when _currentDates[userId].Date.AddMonths(1) > DateTime.Now:
                        {
                            var answer = GetBalanceText(userId);
                            await client.EditMessageTextAsync(chatId, messageId, answer, ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            NextMonth(userId);
                            break;
                        }
                        case "right":
                        {
                            NextMonth(userId);
                            var startDate = new DateTime(_currentDates[userId].Year, _currentDates[userId].Month, 01);
                            var endDate = startDate.AddMonths(1).AddDays(-1);
                       
                            var answer = GetBalanceText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            break;
                        }
                    }
                }
            }
            
        }
        private string GetBalanceText(long userId, DateTime? startDate=null, DateTime? endDate=null)
        {
            var chartEmoji = new Emoji(0x1F4CA);
            var upChart = new Emoji(0x1F4C8);
            var downChart = new Emoji(0x1F4C9);
            string period;
            var statisticManager = new StatisticsManager();
            decimal totalAmountOfRevenues, totalAmountOfExpenses;
            if (startDate == null || endDate == null)
            {
                totalAmountOfRevenues = statisticManager.GetTotalAmountOfRevenues(userId);
                totalAmountOfExpenses = statisticManager.GetTotalAmountOfExpenses(userId);
                period = "весь час";
            }
            else
            {
                totalAmountOfRevenues = statisticManager.GetTotalAmountOfRevenues(userId,startDate.Value,endDate.Value);
                totalAmountOfExpenses = statisticManager.GetTotalAmountOfExpenses(userId, startDate.Value, endDate.Value);
                period = startDate.Value.Year < DateTime.Now.Year ? startDate.Value.ToString("MMMM", _culture) + " " + startDate.Value.Year
                    : startDate.Value.ToString("MMMM", _culture);
            }
            return $"{chartEmoji} Баланс за <u><b>{period}</b></u> \n" +
                   $"Загальна сума доходів {upChart} - {totalAmountOfRevenues} ₴\n\n" +
                   $"Загальна сума витрат  {downChart} - {totalAmountOfExpenses} ₴\n" +
                   $"------------------------------\n" +
                   $"Баланс\t\t {totalAmountOfRevenues - totalAmountOfExpenses} ₴";
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
