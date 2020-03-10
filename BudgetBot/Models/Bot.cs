using BudgetBot.Models.Command;
using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBot.Models
{
    public static class Bot
    {
        private static TelegramBotClient botClient;

        private static List<Command.BaseCommand> commands;

        private static BotDbContext dbContext = new BotDbContext();
        public static IReadOnlyList<Command.BaseCommand> Commands { get => commands.AsReadOnly(); }
        public static async Task<TelegramBotClient> Get()
        {
            if (botClient!=null)
            {
                return botClient;
            }
            botClient = new TelegramBotClient(AppSettings.Token);

            commands = new List<BaseCommand>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(BaseCommand));
            foreach (var type in types)
            {
                commands.Add(Activator.CreateInstance(type) as BaseCommand);
            }

            await botClient.SetWebhookAsync(AppSettings.Url + "/api/message/update");
            return botClient;
        }
        public static async Task SendCategories(Message message, string messageText, CategoryType categoryType)
        {
            var chatId = message.Chat.Id;
            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var category in dbContext.GetCategories(message.From.Id, categoryType))
            {
                var row = new List<InlineKeyboardButton>();
                row.Add(MakeInlineButton(category.GetImage() + " " + category.Name, category.Name));
                buttons.Add(row);
            }

            await botClient.SendTextMessageAsync(chatId, messageText, replyMarkup: new InlineKeyboardMarkup(buttons));
        }

        public static InlineKeyboardButton MakeInlineButton(string text, string callBackData)
        {
            var inlineButton = new InlineKeyboardButton();
            inlineButton.Text = text;
            inlineButton.CallbackData = callBackData;
            return inlineButton;
        }

        public static InlineKeyboardMarkup MakeInlineKeyboard(params InlineKeyboardButton[] buttons)
        {
            return new InlineKeyboardMarkup(buttons); 
        }
        public static bool HasCommand(string commandName)
        {
            foreach (var command in commands)
            {
                if (command.Name == commandName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
