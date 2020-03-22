using System.Threading.Tasks;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Commands
{
    public class HelpCommand : Command
    {
        public override string Name { get => "/help"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var chatId = GetChatId(update);
            var answer = new Emoji(0x2139) + " Вас вітає Budget_bot) \n" +
                "Цей бот допоможе вам слідкувати за доходами та витратими. \n" +
                "Щоб почати роботу скористайтесь доступними командами: \n" +
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
