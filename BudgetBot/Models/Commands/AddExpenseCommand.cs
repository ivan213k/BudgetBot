using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static BudgetBot.Models.StateData.State;

namespace BudgetBot.Models.Command
{
    public class AddExpenseCommand : BaseCommand
    {
        public override string Name { get => "/addexpense"; }

        private BotDbContext dbContext = new BotDbContext();

        private IFormatProvider culture = new CultureInfo("uk-Ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            if (GetCurrentStep(userId) == 0)
            {
                await Bot.SendCategories(update.Message, "Виберіть категорію зі списку: ",CategoryType.Expense);
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 1)
            {
                AddExpense(userId, category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId,"Введіть суму витрати");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 2)
            {
                if (!decimal.TryParse(update.Message.Text,out decimal amount))
                {
                    await client.SendTextMessageAsync(chatId, "Упс..., введіть суму витрати");
                    return;
                }
                AddExpense(userId, amount: amount, date: DateTime.Now);
                var expense = userExpenses.Where(r=>r.UserId == userId).Single();
                
                var successEmoji = new Emoji(0x2705);
                var answer = successEmoji + $" Витрату додано:\n" +
                    $"Категорія - {expense.Category.Name}\n" +
                    $"Сума - {expense.Amount}$\n" +
                    $"Дата - {expense.Date.ToShortDateString()}";

                var selectDateButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C6)}Змінити дату","selectDate");
                var cancelButton = Bot.MakeInlineButton($"{new Emoji(0x274C)}Скасувати", "cancel");

                await client.SendTextMessageAsync(chatId, answer,replyMarkup: Bot.MakeInlineKeyboard(selectDateButton,cancelButton));
                AddInsertedExpense(userId,(await dbContext.Expenses.AddAsync(expense)).Entity);
                
                await dbContext.SaveChangesAsync();
                userExpenses.Remove(expense);  
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 3)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery.Data == "cancel")
                    {
                        dbContext.Expenses.Remove(insertedExpense[userId]);
                        dbContext.SaveChanges();
                        var answer = $"{new Emoji(0x274C)} <u><b>ВИДАЛЕНО:</b></u>\n" +
                                    $"Категорія - {insertedExpense[userId].Category.Name}\n" +
                                    $"Сума - {insertedExpense[userId].Amount}$\n" +
                                    $"Дата - {insertedExpense[userId].Date.ToShortDateString()}";
                        await client.EditMessageTextAsync(chatId,update.CallbackQuery.Message.MessageId,answer,parseMode:ParseMode.Html);
                        FinishCurrentCommand(userId);
                    }
                    if (update.CallbackQuery.Data == "selectDate")
                    {
                        await client.EditMessageTextAsync(chatId,update.CallbackQuery.Message.MessageId,
                            $"Введіть дату в наступному форматі: <b>{DateTime.Now.ToString("dd.MM.yyyy",culture)}</b>",parseMode:ParseMode.Html);
                        NextStep(userId);
                    }   
                }
            }
            if (GetCurrentStep(userId)==4)
            {
                if (DateTime.TryParse(update.Message.Text,out DateTime date))
                {
                    insertedExpense[userId].Date = date;
                    dbContext.Expenses.Update(insertedExpense[userId]);
                    await dbContext.SaveChangesAsync();
                    var successEmoji = new Emoji(0x2705);
                    var answer = successEmoji + $" Витрату відредаговано:\n" +
                        $"Категорія - {insertedExpense[userId].Category.Name}\n" +
                        $"Сума - {insertedExpense[userId].Amount}$\n" +
                        $"Дата - {insertedExpense[userId].Date.ToShortDateString()}";
                    var selectDateButton = Bot.MakeInlineButton($"{new Emoji(0x1F4C6)}Змінити дату", "selectDate");
                    var cancelButton = Bot.MakeInlineButton($"{new Emoji(0x274C)}Скасувати", "cancel");
                    await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeInlineKeyboard(selectDateButton, cancelButton));
                    PreviousStep(userId);
                }
                else
                {
                    await client.SendTextMessageAsync(chatId,"Упс... Не вдалось розпізнати дату, спробуйте ще раз");
                }
            }
        }

        private List<Expense> userExpenses = new List<Expense>();

        private Dictionary<long, Expense> insertedExpense = new Dictionary<long, Expense>();

        private void AddInsertedExpense(long userId, Expense expense)
        {
            if (insertedExpense.ContainsKey(userId))
            {
                insertedExpense[userId] = expense;
            }
            else
            {
                insertedExpense.Add(userId, expense);
            }
        }
        private void AddExpense(long userId, string category = "", decimal amount = default, DateTime date = default)
        {
            var record = userExpenses.Where(r => r.UserId == userId).FirstOrDefault();
            if (record != null)
            {
                if (category != "")
                {
                    record.Category = dbContext.GetCategory(userId, category, CategoryType.Expense);
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
                userExpenses.Add(new Expense(userId, dbContext.GetCategory(userId, category, CategoryType.Expense), amount, date));
            }
        }
    }
}
