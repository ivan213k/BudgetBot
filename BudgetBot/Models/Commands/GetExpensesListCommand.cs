using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class GetExpensesListCommand : Command
    {
        public override string Name { get => "/expenseslist"; }

        private readonly BotDbContext _dbContext = new BotDbContext();

        private readonly IFormatProvider _culture = new CultureInfo("Uk-ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                var answer = GetExpensesListText(userId, startDate, endDate);
                await client.SendTextMessageAsync(chatId, answer, ParseMode.Html, replyMarkup:Bot.MakeDateSwichKeyboard());
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId)== 1)
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
                            var answer = GetExpensesListText(userId, startDate, endDate);
                            await client.EditMessageTextAsync(chatId, messageId, answer, ParseMode.Html, replyMarkup: Bot.MakeDateSwichKeyboard());
                            return;
                        }
                        case "right":
                        {
                            if (_currentDates[userId].Date.AddMonths(1) > DateTime.Now) return;
                            NextMonth(userId);
                            var startDate = new DateTime(_currentDates[userId].Year, _currentDates[userId].Month, 01);
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
            List<Expense> expenses = _dbContext.GetExpenses(userId, startDate, endDate).OrderByDescending(r=>r.Date).ToList();
            StringBuilder stringBuilder = new StringBuilder();
            string period = startDate.Year == DateTime.Now.Year ? startDate.ToString("MMMM", _culture) : startDate.ToString("MMMM yyyy", _culture);
            stringBuilder.Append($"{new Emoji(0x1F4DC)} Список витрат за <u><b>{period}</b></u>\n");
            foreach (var expense in expenses)
            {
                stringBuilder.Append(expense + "\n");
            }
            return stringBuilder.ToString();
        }

        private readonly Dictionary<long, DateTime> _currentDates = new Dictionary<long, DateTime>();
        private void NextMonth(long userId)
        {
            if (_currentDates[userId].Date.AddMonths(1) <= DateTime.Now)
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
                _currentDates.Add(userId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
            }
        }
    }
}
