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
            var userId = update.Message.From.Id;
            var statisticManager = new StatisticsManager();
            var totalAmountOfExpenses = statisticManager.GetTotalAmountOfExpenses(userId);
            var TotalAmountOfRevenues = statisticManager.GetTotalAmountOfRevenues(userId);
            var answer = $"Баланс: \n" +
                $"Загальна сума витрат - {totalAmountOfExpenses}\n\n" +
                $"Загальна сума доходів - {TotalAmountOfRevenues}\n" +
                $"------------------------------\n" +
                $"Баланс\t\t {TotalAmountOfRevenues - totalAmountOfExpenses}";
            
       
            await client.SendTextMessageAsync(update.Message.Chat.Id, answer);
        }
    }
}
