using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }

        public abstract Task Execute(Update update, TelegramBotClient client);

        protected long GetUserId(Update update)
        {
            return update.Message?.From.Id ?? update.CallbackQuery.From.Id;
        }

        protected long GetChatId(Update update)
        {
            return update.Message?.Chat.Id ?? update.CallbackQuery.Message.Chat.Id;
        }

        protected int GetMessageId(Update update)
        {
            return update.Message?.MessageId ?? update.CallbackQuery.Message.MessageId;
        }
    }
}
