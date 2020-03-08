using BudgetBot.Models.DataBase;
using BudgetBot.Models.StateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetBot.Models.Command
{
    public class AddExpenseCommand : BaseCommand
    {
        public override string Name { get => "/addexpense"; }

        private List<Expense> userExpenses = new List<Expense>();

        private BotDbContext dbContext = new BotDbContext();
        private void AddExpense(long userId,string category="",decimal amount=default,DateTime date = default)
        {
            var record = userExpenses.Where(r => r.UserId == userId).FirstOrDefault();
            if (record!=null)
            {
                if (category != "")
                {
                    record.Category = dbContext.GetCategory(userId,category,CategoryType.Expense);
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

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            var chatId = GetChatId(update);
            InitCommandSteps(userId);
            if (GetCurrentStep(userId) == 0)
            {
                State.AddCurrentCommand(userId, Name);
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

                var expense = userExpenses.Where(r => r.UserId == userId).Single();
                var successEmoji = new Emoji(0x2705);
                await client.SendTextMessageAsync(chatId, successEmoji + $" Витрату додано:\n" +
                    $"Категорія - {expense.Category.Name}\n" +
                    $"Сума - {expense.Amount}$\n" +
                    $"Дата - {expense.Date.ToShortDateString()}");
                await dbContext.Expenses.AddAsync(expense);
                await dbContext.SaveChangesAsync();
                userExpenses.Remove(expense);
                State.FinishCurrentCommand(userId);
                ResetCommandSteps(userId);
            }
        }
    }
}
