using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    public class ExternalAuthController : Controller
    {
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExternalCallback()
        {
            return View();
        }
    }
}