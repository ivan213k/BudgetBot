using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BudgetBot.Controllers
{
    public class HomeController : Controller
    {
        public string Index()
        {
            return "It`s my telegram bot:)";
        }
    }
}
