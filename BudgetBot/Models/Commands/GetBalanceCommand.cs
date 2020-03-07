using BudgetBot.Models.Command;
using BudgetBot.Models.Statistics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Commands
{
    public class GetBalanceCommand : BaseCommand
    {
        public override string Name { get => "/balance"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);

            var statisticManager = new StatisticsManager();
            var totalAmountOfExpenses = statisticManager.GetTotalAmountOfExpenses(userId);
            var TotalAmountOfRevenues = statisticManager.GetTotalAmountOfRevenues(userId);

            var chartEmoji = new Emoji(0x1F4CA);
            var upChart = new Emoji(0x1F4C8);
            var downChart = new Emoji(0x1F4C9);

            var answer = $"{chartEmoji} Баланс: \n" +
                $"Загальна сума доходів {upChart} - {TotalAmountOfRevenues}\n\n" +
                $"Загальна сума витрат  {downChart} - {totalAmountOfExpenses}\n" +
                $"------------------------------\n" +
                $"Баланс\t\t {TotalAmountOfRevenues - totalAmountOfExpenses}";
             
            await client.SendTextMessageAsync(chatId, answer);
        }
    }
}
