using System.Linq;
using System.Threading.Tasks;
using BudgetBot.Models;
using BudgetBot.Models.StateData;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBot.Controllers
{
    public class MessageController : Controller
    {
        [Route("api/message/update")]
        public async Task<OkResult> Update([FromBody]Update update)
        {
            var commands = Bot.Commands;
            var client = await Bot.Get();
            if (update.Type == UpdateType.Message)
            {
                var userId = update.Message.From.Id;
                if (Bot.HasCommand(update.Message.Text))
                {
                    StateMachine.AddCurrentCommand(userId, update.Message.Text);
                    await commands.Single(r => r.Name == update.Message.Text).Execute(update, client);
                }
                else if (StateMachine.GetCurrentCommand(userId)!=null)
                {
                    await commands.Single(r => r.Name == StateMachine.GetCurrentCommand(userId)).Execute(update, client);
                }
                else
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, $"Команду не розпізнано {new Emoji(0x1F61E)}");
                }
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                var userId = update.CallbackQuery.From.Id;
                var command = commands.FirstOrDefault(r => r.Name == StateMachine.GetCurrentCommand(userId));
                if (command!=null)
                {
                    await command.Execute(update,client);
                }
            }
            return Ok();
        }
    }
}