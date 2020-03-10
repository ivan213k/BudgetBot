using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Command
{
    public abstract class BaseCommand
    {
        public abstract string Name { get; }

        public abstract Task Execute(Update update, TelegramBotClient client);

        protected long GetUserId(Update update)
        {
            if (update.Message != null)
            {
                return update.Message.From.Id;
            }
            return update.CallbackQuery.From.Id;
        }

        protected long GetChatId(Update update)
        {
            if (update.Message!=null)
            {
                return update.Message.Chat.Id;
            }
            return update.CallbackQuery.Message.Chat.Id;
        }
    }
}
