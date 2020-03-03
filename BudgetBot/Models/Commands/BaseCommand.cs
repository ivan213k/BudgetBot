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

        private Dictionary<long, ushort> userSteps = new Dictionary<long, ushort>();

        protected ushort GetCurrentStep(long userId)
        {
            return userSteps[userId];
        }
        protected void ResetCommandSteps(long userId)
        {
            userSteps[userId] = 0;
        }
        protected void NextStep(long userId)
        {
            var step = userSteps[userId];
            userSteps[userId] = (ushort)(step + 1);
        }
        protected void InitCommandSteps(long userId)
        {
            if (!userSteps.ContainsKey(userId))
            {
                userSteps.Add(userId, 0);
            }
        }
        protected long GetUserId(Update update)
        {
            if (update.Message != null)
            {
                return update.Message.From.Id;
            }
            else
            {
                return update.CallbackQuery.From.Id;
            }
        }
    }
}
