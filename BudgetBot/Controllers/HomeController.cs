using Microsoft.AspNetCore.Mvc;

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
