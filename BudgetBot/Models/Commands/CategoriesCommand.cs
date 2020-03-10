using BudgetBot.Models.DataBase;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static BudgetBot.Models.StateData.State;
namespace BudgetBot.Models.Command
{
    public class CategoriesCommand : BaseCommand
    {
        public override string Name { get => "/categories"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            await Bot.SendCategories(update.Message,"Список категорій: ", CategoryType.Expense);
            FinishCurrentCommand(userId);
        }
    }
}
