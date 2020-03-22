using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class AddRevenueCommand : Command
    {
        public override string Name { get => "/addrevenue"; }

        private BotDbContext dbContext = new BotDbContext();

        private IFormatProvider culture = new CultureInfo("uk-Ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            {
                await Bot.SendCategories(update.Message,"Виберіть категорію доходу\t", CategoryType.Revenue);
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 1)
            {
                AddRevenue(userId,category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId,"Введіть суму доходу");
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 2)
            {
                if (!decimal.TryParse(update.Message.Text, out decimal amount))
                {
                    await client.SendTextMessageAsync(chatId, "Упс..., введіть суму доходу");
                    return;
                }
                AddRevenue(userId, amount: amount, date: DateTime.Now);

                var revenue = userRevenues.Single(r => r.UserId == userId);
                var answer = MakeAddedRevenueText(revenue);

                await client.SendTextMessageAsync(chatId, answer, replyMarkup:Bot.MakeDateEditKeyboard());

                var insertedRevenue = (await dbContext.Revenues.AddAsync(revenue)).Entity;
                AddInsertedRevenue(userId, insertedRevenue);
                await dbContext.SaveChangesAsync();
                userRevenues.Remove(revenue);
                StateMachine.NextStep(userId);
            }
            if (StateMachine.GetCurrentStep(userId) == 3)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery.Data == "cancel")
                    {
                        dbContext.Revenues.Remove(insertedRevenues[userId]);
                        dbContext.SaveChanges();
                        var answer = $"{new Emoji(0x274C)} <u><b>ВИДАЛЕНО:</b></u>\n" +
                                    $"Категорія - {insertedRevenues[userId].Category.Name}\n" +
                                    $"Сума - {insertedRevenues[userId].Amount} ₴\n" +
                                    $"Дата - {insertedRevenues[userId].Date.ToString("dd.MM.yyyy",culture)}";
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html);
                        StateMachine.FinishCurrentCommand(userId);
                    }
                    if (update.CallbackQuery.Data == "selectDate")
                    {
                        await client.EditMessageTextAsync(chatId, messageId,
                            $"Введіть дату в наступному форматі: <b>{DateTime.Now.ToString("dd.MM.yyyy", culture)}</b>", parseMode: ParseMode.Html);
                        StateMachine.NextStep(userId);
                    }
                }
            }
            if (StateMachine.GetCurrentStep(userId) == 4)
            {
                if (DateTime.TryParse(update.Message.Text,culture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    insertedRevenues[userId].Date = date;
                    dbContext.Revenues.Update(insertedRevenues[userId]);
                    await dbContext.SaveChangesAsync();
                    var answer = MakeAddedRevenueText(insertedRevenues[userId]).Replace("додано","відредаговано");
                    
                    await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeDateEditKeyboard());
                    StateMachine.PreviousStep(userId);
                }
                else
                {
                    await client.SendTextMessageAsync(chatId, "Упс... Не вдалось розпізнати дату, спробуйте ще раз");
                }
            }
        }

        private List<Revenue> userRevenues = new List<Revenue>();

        private Dictionary<long, Revenue> insertedRevenues = new Dictionary<long, Revenue>();
        private string MakeAddedRevenueText(Revenue expense)
        {
            var successEmoji = new Emoji(0x2705);
            return successEmoji + $" Дохід додано:\n" +
                       $"Категорія - {expense.Category.Name}\n" +
                       $"Сума - {expense.Amount} ₴\n" +
                       $"Дата - {expense.Date.ToString("dd.MM.yyyy", culture)}";
        }
        private void AddRevenue(long userId, string category = "", decimal amount = default, DateTime date = default)
        {
            var record = userRevenues.Where(r => r.UserId == userId).FirstOrDefault();
            if (record != null)
            {
                if (category != "")
                {
                    record.Category = dbContext.GetCategory(userId,category,CategoryType.Revenue);
                }
                if (amount != default)
                {
                    record.Amount = amount;
                }
                if (date != default)
                {
                    record.Date = date;
                }
            }
            else
            {
                userRevenues.Add(new Revenue(userId, dbContext.GetCategory(userId,category,CategoryType.Revenue), amount, date));
            }
        }
        private void AddInsertedRevenue(long userId, Revenue revenue)
        {
            if (insertedRevenues.ContainsKey(userId))
            {
                insertedRevenues[userId] = revenue;
            }
            else
            {
                insertedRevenues.Add(userId, revenue);
            }
        }
    }
}
