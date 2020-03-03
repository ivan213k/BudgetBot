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
                    await commands.Where(r => r.Name == update.Message.Text).Single().Execute(update, client);
                }
                else if (State.GetCurrentCommand(userId)!=null)
                {
                    await commands.Where(r => r.Name == State.GetCurrentCommand(userId)).
                        Single().Execute(update, client);
                }
                else
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id,"Команду не розпізнано(");
                }
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                var userId = update.CallbackQuery.From.Id;
                await commands.Where(r => r.Name == State.GetCurrentCommand(userId)).Single().
                    Execute(update, client);
            }
          
            return Ok();
        }
    }
}