using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.Statistics;
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
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var topChart = new Emoji(0x1F4C8);
            var answer = $"{topChart} Статистика доходів по категоріям:\n";
            var statisticManager = new StatisticsManager();
            foreach (var row in statisticManager.GetRevenuesStatistic(userId))
            {
                var categoryEmoji = dbContext.GetCategoryEmoji(row.Categrory,CategoryType.Revenue);
                answer += $"\t\t\t\t{categoryEmoji} {row.Categrory} - {row.TotalAmount} ({row.Percent}%)\n";
            }
            answer += $"Загальна сума доходів: <u><b>{statisticManager.GetTotalAmountOfRevenues(userId)}</b></u>";
            await client.SendTextMessageAsync(chatId, answer, ParseMode.Html,replyMarkup:Bot.MakeDateSwichKeyboard());
            FinishCurrentCommand(userId);
        }
    }
}
