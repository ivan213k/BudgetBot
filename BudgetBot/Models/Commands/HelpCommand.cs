using BudgetBot.Models.Command;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static BudgetBot.Models.StateData.State;

namespace BudgetBot.Models.Commands
{
    public class HelpCommand : BaseCommand
    {
        public override string Name { get => "/help"; }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var chatId = GetChatId(update);
            var answer = new Emoji(0x2139).ToString() + " Вас вітає Budget_bot) \n" +
                "Цей бот допоможе вам слідкувати за доходами та витратими. \n" +
                "Щоб почати роботу скористайтесь доступними командами: \n" +
                "/addexpense - додати витрату\n" +
                "/addrevenue - додати дохід\n" +
                "/getexpensestat - статистика витрат\n" +
                "/getrevenuestat - статистика доходів\n" +
                "/balance - баланс\n" +
                "/addcategory - додати категорію витрат чи доходів";
            await client.SendTextMessageAsync(chatId, answer);
            FinishCurrentCommand(GetUserId(update));
        }
    }
}
