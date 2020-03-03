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

        private void AddExpense(long userId,string category="",decimal amount=default,DateTime date = default)
        {
            var record = userExpenses.Where(r => r.UserId == userId).FirstOrDefault();
            if (record!=null)
            {
                if (category != "")
                {
                    record.Category = category;
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
                userExpenses.Add(new Expense(userId, category, amount, date));
            }
        }

        public override async Task Execute(Update update, TelegramBotClient client)
        {
            var userId = GetUserId(update);
            InitCommandSteps(userId);
            if (GetCurrentStep(userId) == 0)
            {
                State.AddCurrentCommand(userId, Name);
                await Bot.SendExpenseCategories(update.Message, "Виберіть категорію зі списку: ");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 1)
            {
                AddExpense(userId, category: update.CallbackQuery.Data);
                await client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Введіть суму витрати");
                NextStep(userId);
                return;
            }
            if (GetCurrentStep(userId) == 2)
            {
                if (!decimal.TryParse(update.Message.Text,out decimal amount))
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, "Упс..., введіть суму витрати");
                    return;
                }
                AddExpense(userId, amount: amount, date: DateTime.Now);

                var expense = userExpenses.Where(r => r.UserId == userId).Single();
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"Витрату додано:\n" +
                    $"Категорія - {expense.Category}\n" +
                    $"Сума - {expense.Amount}$\n" +
                    $"Дата - {expense.Date.ToShortDateString()}");
                Repository.Expenses.Add(expense);
                userExpenses.Remove(expense);
                State.FinishCurrentCommand(userId);
                ResetCommandSteps(userId);
            }

        }
    }
}
