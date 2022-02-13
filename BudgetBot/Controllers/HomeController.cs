using Microsoft.AspNetCore.Mvc;

namespace BudgetBot.Controllers
{
    public class HomeController : Controller
    {
        [Route("home/index")]
        public string Index()
        {
            return "It`s my telegram bot:)";
        }
    }
}
