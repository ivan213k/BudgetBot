﻿using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.Statistics;
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
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var downChart = new Emoji(0x1F4C9);
            var answer = $"{downChart} Статистика витрат по категоріям:\n";
            var statisticManager = new StatisticsManager();
            foreach (var row in statisticManager.GetExpensesStatistic(userId))
            {
                var categoryEmoji = dbContext.GetCategoryEmoji(row.Categrory,CategoryType.Expense);
                answer += $"\t\t\t{categoryEmoji} {row.Categrory} - {row.TotalAmount} ({row.Percent}%)\n";
            }
            answer += $"Загальна сума витрат: <u><b>{statisticManager.GetTotalAmountOfExpenses(userId)}</b></u>";
            await client.SendTextMessageAsync(chatId,answer,parseMode: ParseMode.Html);
            FinishCurrentCommand(userId);
        }
    }
}
