using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BudgetBot.Models.Commands;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBot.Models
{
    public static class Bot
    {
        private static TelegramBotClient _botClient;

        private static List<Command> _commands;

        private static readonly BotDbContext DbContext = new BotDbContext();
        public static IReadOnlyList<Command> Commands { get => _commands.AsReadOnly(); }
        public static async Task<TelegramBotClient> Get()
        {
            if (_botClient!=null)
            {
                return _botClient;
            }
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("botsettings.json");
            var config = builder.Build();
            var token = config.GetSection("bot_settings")["Token"];
            _botClient = new TelegramBotClient(token);

            _commands = new List<Command>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(Command));
            foreach (var type in types)
            {
                _commands.Add(Activator.CreateInstance(type) as Command);
            }

            await _botClient.SetWebhookAsync(config.GetSection("bot_settings")["Url"] + "/api/message/update");
            return _botClient;
        }
        public static async Task SendCategories(Message message, string messageText, CategoryType categoryType)
        {
            var chatId = message.Chat.Id;
            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var category in DbContext.GetCategories(message.From.Id, categoryType))
            {
                var row = new List<InlineKeyboardButton>
                {
                    MakeInlineButton(category.GetImage() + " " + category.Name, category.Name)
                };
                buttons.Add(row);
            }

            await _botClient.SendTextMessageAsync(chatId, messageText, replyMarkup: new InlineKeyboardMarkup(buttons));
        }

        public static InlineKeyboardButton MakeInlineButton(string text, string callBackData)
        {
            return new InlineKeyboardButton
            {
                Text = text,
                CallbackData = callBackData
            };
        }

        public static InlineKeyboardMarkup MakeInlineKeyboard(params InlineKeyboardButton[] buttons)
        {
            return new InlineKeyboardMarkup(buttons); 
        }
        public static InlineKeyboardMarkup MakeDateSwichKeyboard()
        {
            var leftButton = MakeInlineButton("<<<","left");
            var rightButton = MakeInlineButton(">>>", "right");
            return MakeInlineKeyboard(leftButton,rightButton);
        }

        public static InlineKeyboardMarkup MakeDateEditKeyboard()
        {
            var selectDateButton = MakeInlineButton($"{new Emoji(0x1F4C6)}Змінити дату", "selectDate");
            var cancelButton = MakeInlineButton($"{new Emoji(0x274C)}Скасувати", "cancel");
            return MakeInlineKeyboard(selectDateButton,cancelButton);
        }

        public static InlineKeyboardMarkup MakeYesNoKeyboard()
        {
            var yesButton = MakeInlineButton("Так","yes");
            var noButton = MakeInlineButton("Ні", "no");
            return MakeInlineKeyboard(yesButton, noButton);
        }
        public static bool HasCommand(string commandName)
        {
            return _commands.Any(command => command.Name == commandName);
        }
    }
}
