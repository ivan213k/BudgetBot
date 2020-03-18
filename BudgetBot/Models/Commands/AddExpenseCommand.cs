using BudgetBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
            var messageId = GetMessageId(update);
            if (GetCurrentStep(userId) == 0)
            {
                await Bot.SendCategories(update.Message, "Виберіть категорію зі списку: ", CategoryType.Expense);
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 1)
            {
                AddExpense(userId, category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(chatId,"Введіть суму витрати наприклад:" +
                    "\n<b>300</b> або <b>300 опис витрати</b>", ParseMode.Html);
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 2)
            {
                if (!TryParseAmountWithDescription(update.Message.Text,out decimal amount, out string description))
                {
                    await client.SendTextMessageAsync(chatId, "Упс... введіть суму витрати");
                    return;
                }
                AddExpense(userId, amount: amount, date: DateTime.Now, description: description);
                var expense = userExpenses.Where(r=>r.UserId == userId).Single();
                
                var successEmoji = new Emoji(0x2705);
                var answer = MakeAddedExpenseText(expense);

                await client.SendTextMessageAsync(chatId, answer,replyMarkup: Bot.MakeDateEditKeyboard());
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
                                    $"Сума - {insertedExpense[userId].Amount} ₴\n" +
                                    $"Дата - {insertedExpense[userId].Date.ToString("dd.MM.yyyy",culture)}";
                        await client.EditMessageTextAsync(chatId, messageId, answer, parseMode:ParseMode.Html);
                        FinishCurrentCommand(userId);
                    }
                    if (update.CallbackQuery.Data == "selectDate")
                    {
                        await client.EditMessageTextAsync(chatId, messageId,
                            $"Введіть дату в наступному форматі: <b>{DateTime.Now.ToString("dd.MM.yyyy",culture)}</b>",parseMode:ParseMode.Html);
                        NextStep(userId);
                    }   
                }
            }
            if (GetCurrentStep(userId)==4)
            {
                if (DateTime.TryParse(update.Message.Text, culture , DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    insertedExpense[userId].Date = date;
                    dbContext.Expenses.Update(insertedExpense[userId]);
                    await dbContext.SaveChangesAsync();
                    var answer = MakeAddedExpenseText(insertedExpense[userId]).Replace("додано","відредаговано");

                    await client.SendTextMessageAsync(chatId, answer, replyMarkup: Bot.MakeDateEditKeyboard());
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

        private string MakeAddedExpenseText(Expense expense)
        {
            var successEmoji = new Emoji(0x2705);
            return successEmoji + $" Витрату додано:\n" +
                       $"Категорія - {expense.Category.Name}\n" +
                       $"Сума - {expense.Amount} ₴\n" +
                       $"Дата - {expense.Date.ToString("dd.MM.yyyy", culture)}";
        }

        private bool TryParseAmountWithDescription(string text, out decimal amount, out string description)
        {
            description = "";
            if (decimal.TryParse(text,out amount))
            {
                return true;
            }
            else
            {
                string pattern = @"\d+(\.\d+)?";
                if (Regex.IsMatch(text, pattern))
                {
                    var strs = Regex.Match(text, pattern);
                    if (decimal.TryParse(strs.Value,out amount))
                    {
                        description  = text.Replace(strs.Value,"");
                        return true;
                    }
                }
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
                if (description!="")
                {
                    record.Description = description;
                }
            }
            else
            {
                userExpenses.Add(new Expense(userId, dbContext.GetCategory(userId, category, CategoryType.Expense), amount, date,description));
            }
        }
    }
}
