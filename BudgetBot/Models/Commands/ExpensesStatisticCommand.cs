using BudgetBot.Models.Command;
using BudgetBot.Models.Statistics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Commands
{
    public class ExpensesStatisticCommand : BaseCommand
    {
        public override string Name { get => "/getexpensestat"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = update.Message.From.Id;
            var answer = "Статистика витрат по категоріям:\n";
            var statisticManager = new StatisticsManager();
            foreach (var row in statisticManager.GetExpensesStatistic(userId))
            {
                answer += $"{row.Categrory} - {row.TotalAmount} ({row.Percent}%)\n";
            }
            answer += $"Загальна сума витрат: {statisticManager.GetTotalAmountOfExpenses(userId)}";
            await client.SendTextMessageAsync(update.Message.Chat.Id,answer);
        }
    }
}
