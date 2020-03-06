using BudgetBot.Models.Command;
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
        public static async Task SendExpenseCategories(Message message, string messageText)
        {
            var chatId = message.Chat.Id;
            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var category in Repository.GetExpenseCategories(message.From.Id))
            {
                var row = new List<InlineKeyboardButton>();
                var button = new InlineKeyboardButton();
                button.Text = category.GetImage()+ " " + category.Name;
                button.CallbackData = category.Name;
                row.Add(button);
                buttons.Add(row);
            }
            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(chatId, messageText, replyMarkup: keyboard);
        }

        public static async Task SendRevenueCategories(Message message, string messageText)
        {
            var chatId = message.Chat.Id;
            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var category in Repository.GetRevenueCategories(message.From.Id))
            {
                var row = new List<InlineKeyboardButton>();
                var button = new InlineKeyboardButton();
                button.Text = category.GetImage() + " " + category.Name;
                button.CallbackData = category.Name;
                row.Add(button);
                buttons.Add(row);
            }
            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(chatId, messageText, replyMarkup: keyboard);
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
