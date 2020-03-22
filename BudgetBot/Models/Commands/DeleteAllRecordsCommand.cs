using System.Threading.Tasks;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class DeleteAllRecordsCommand : Command
    {
        public override string Name => "/deleteallrecords";

        private  readonly BotDbContext _dbContext = new BotDbContext();
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var chatId = GetChatId(update);
            var userId = GetUserId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            {
                var answer = $"Дійсно видалити всі записи {new Emoji(0x2753)}";
                await client.SendTextMessageAsync(chatId, answer, replyMarkup:Bot.MakeYesNoKeyboard());
                StateMachine.NextStep(userId);
            }

            if (StateMachine.GetCurrentStep(userId) == 1)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    switch (update.CallbackQuery.Data)
                    {
                        case "yes":
                        {
                            await _dbContext.DeleteAllRecords(userId);
                            await client.EditMessageTextAsync(chatId, messageId, $"{new Emoji(0x274E)}Всі записи було видалено");
                            StateMachine.FinishCurrentCommand(userId);
                            return;
                        }
                        case "no":
                        {
                            await client.DeleteMessageAsync(chatId, messageId);
                            StateMachine.FinishCurrentCommand(userId);
                            return;
                        }
                    }
                }
            }
        }
    }
}
