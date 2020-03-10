using BudgetBot.Models.Command;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static BudgetBot.Models.StateData.State;

namespace BudgetBot.Models.Commands
{
    public class StartCommand : BaseCommand
    {
        public override string Name { get => "/start"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var chatId = GetChatId(update);
            var answer = "Вас вітає Budget_bot) \n" +
                "Цей бот допоможе вам слідкувати за доходами та витратими. \n" +
                "Що вміє цей бот: \n" +
                "-Вести облік доходів\n" +
                "-Вести облік витрат\n" +
                "-Відображати статистику витрат, доходів та баланс\n" +
                "Щоб почати роботу скористайтесь доступними командами.";
            await client.SendTextMessageAsync(chatId, answer);
            FinishCurrentCommand(GetUserId(update));
        }
    }
}
