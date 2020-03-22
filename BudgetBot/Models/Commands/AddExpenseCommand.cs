using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Models.Commands
{
    public class AddExpenseCommand : Command
    {
        public override string Name { get => "/addexpense"; }

        private readonly BotDbContext _dbContext = new BotDbContext();

        private readonly IFormatProvider _culture = new CultureInfo("uk-Ua");
        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            var messageId = GetMessageId(update);
            if (StateMachine.GetCurrentStep(userId) == 0)
            {
                await Bot.SendCategories(update.Message, "Виберіть категорію зі списку: ", CategoryType.Expense);
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 1)
            {
                AddExpense(userId, category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId, "Введіть суму витрати наприклад:" +
                    "\n<b>300</b> або <b>300 опис витрати</b>", ParseMode.Html);
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 2)
            {
                if (!TryParseAmountWithDescription(update.Message.Text, out decimal amount, out string description))
                {
                    await client.SendTextMessageAsync(chatId, "Упс... введіть суму витрати");
                    return;
                }
                AddExpense(userId, amount: amount, date: DateTime.Now, description: description);
                var expense = userExpenses.Single(r => r.UserId == userId);

                var answer = MakeAddedExpenseText(expense);

                await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeDateEditKeyboard());
                AddInsertedExpense(userId, (await _dbContext.Expenses.AddAsync(expense)).Entity);

                await _dbContext.SaveChangesAsync();
                userExpenses.Remove(expense);
                StateMachine.NextStep(userId);
                return;
            }
            if (StateMachine.GetCurrentStep(userId) == 3)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery.Data == "cancel")
                    {
                        _dbContext.Expenses.Remove(insertedExpense[userId]);
                        _dbContext.SaveChanges();
                        var answer = $"{new Emoji(0x274C)} <u><b>ВИДАЛЕНО:</b></u>\n" +
                                    $"Категорія - {insertedExpense[userId].Category.Name}\n" +
                                    $"Сума - {insertedExpense[userId].Amount} ₴\n" +
                                    $"Дата - {insertedExpense[userId].Date.ToString("dd.MM.yyyy", _culture)}";
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode: ParseMode.Html);
                        StateMachine.FinishCurrentCommand(userId);
                    }
                    if (update.CallbackQuery.Data == "selectDate")
                    {
                        await client.EditMessageTextAsync(chatId, messageId,
                            $"Введіть дату в наступному форматі: <b>{DateTime.Now.ToString("dd.MM.yyyy", _culture)}</b>", parseMode: ParseMode.Html);
                        StateMachine.NextStep(userId);
                    }
                }
            }
            if (StateMachine.GetCurrentStep(userId) == 4)
            {
                if (DateTime.TryParse(update.Message.Text, _culture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    insertedExpense[userId].Date = date;
                    _dbContext.Expenses.Update(insertedExpense[userId]);
                    await _dbContext.SaveChangesAsync();
                    var answer = MakeAddedExpenseText(insertedExpense[userId]).Replace("додано", "відредаговано");

                    await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeDateEditKeyboard());
                    StateMachine.PreviousStep(userId);
                }
                else
                {
                    await client.SendTextMessageAsync(chatId, "Упс... Не вдалось розпізнати дату, спробуйте ще раз");
                }
            }
        }

        private List<Expense> userExpenses = new List<Expense>();

        private Dictionary<long, Expense> insertedExpense = new Dictionary<long, Expense>();

        private string MakeAddedExpenseText(Expense expense)
        {
            var successEmoji = new Emoji(0x2705);
            return successEmoji + " Витрату додано:\n" +
                       $"Категорія - {expense.Category.Name}\n" +
                       $"Сума - {expense.Amount} ₴\n" +
                       $"Дата - {expense.Date.ToString("dd.MM.yyyy", _culture)}";
        }

        private bool TryParseAmountWithDescription(string text, out decimal amount, out string description)
        {
            description = "";
            if (decimal.TryParse(text, out amount))
            {
                return true;
            }
            string pattern = @"\d+(\.\d+)?";
            if (Regex.IsMatch(text, pattern))
            {
                var strs = Regex.Match(text, pattern);
                if (decimal.TryParse(strs.Value, out amount))
                {
                    description = text.Replace(strs.Value, "");
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }
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
        private void AddExpense(long userId, string category = "", decimal amount = default, DateTime date = default, string description = "")
        {
            var record = userExpenses.FirstOrDefault(r => r.UserId == userId);
            if (record != null)
            {
                if (category != "")
                {
                    record.Category = _dbContext.GetCategory(userId, category, CategoryType.Expense);
                }
                if (amount != default)
                {
                    record.Amount = amount;
                }
                if (date != default)
                {
                    record.Date = date;
                }
                if (description != "")
                {
                    record.Description = description;
                }
            }
            else
            {
                userExpenses.Add(new Expense(userId, _dbContext.GetCategory(userId, category, CategoryType.Expense), amount, date, description));
            }
        }
    }
}
