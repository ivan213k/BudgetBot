using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Command
{
    public class CategoriesCommand : BaseCommand
    {
        public override string Name { get => "/categories"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            await Bot.SendExpenseCategories(update.Message,"Список категорій: ");  
        }
    }
}
