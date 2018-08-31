using Microsoft.AspNetCore.Mvc;
using MidnightLizard.Schemes.Screenshots.Services;

namespace MidnightLizard.Schemes.Screenshots.Controllers
{
    [Route("[controller]/[action]")]
    public class StatusController : Controller
    {
        private readonly IMessagingQueue queue;

        public StatusController(IMessagingQueue eventsQueue)
        {
            this.queue = eventsQueue;
        }

        public IActionResult IsReady()
        {
            return this.Ok("schemes screenshot generator is ready");
        }

        public IActionResult IsAlive()
        {
            if (this.queue.CheckStatus())
            {
                return this.Ok("schemes screenshot generator is alive");
            }
            return this.BadRequest("schemes screenshot generator has too many errors and should be restarted");
        }
    }
}
