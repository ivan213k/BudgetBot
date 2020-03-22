using System.Threading.Tasks;
using BudgetBot.Models.StateData;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BudgetBot.Models.Commands
{
    public class StartCommand : Command
    {
        public override string Name { get => "/start"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var chatId = GetChatId(update);
            var answer = $"Вас вітає Budget_bot {new Emoji(0x1F60A)} \n" +
                "Цей бот допоможе вам слідкувати за доходами та витратими. \n" +
                "Що вміє цей бот: \n" +
                "-Вести облік доходів\n" +
                "-Вести облік витрат\n" +
                "-Відображати статистику витрат, доходів та баланс\n" +
                "Щоб почати роботу скористайтесь доступними командами:\n" +
                "/addexpense - додати витрату\n" +
                "/addrevenue - додати дохід\n" +
                "/getexpensestat - статистика витрат\n" +
                "/expenseslist - список витрат\n" +
                "/getrevenuestat - статистика доходів\n" +
                "/balance - баланс\n" +
                "/addcategory - додати категорію витрат чи доходів";
            await client.SendTextMessageAsync(chatId, answer);
            StateMachine.FinishCurrentCommand(GetUserId(update));
        }
    }
}
