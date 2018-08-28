using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MidnightLizard.Schemes.Screenshots.Controllers
{
    [Route("[controller]/[action]")]
    public class StatusController : Controller
    {
        public IActionResult IsReady()
        {
            return Ok("schemes commander is ready");
        }

        public IActionResult IsAlive()
        {
            return Ok("schemes commander is alive");
        }
    }
}
