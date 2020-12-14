using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CCMaster.API.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        public IActionResult Version()
        {
            return Ok("Chinese Chess Master version 1.0.1");
        }
    }
}
