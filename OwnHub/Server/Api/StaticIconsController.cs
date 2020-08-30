using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.Preview.Icons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OwnHub.Server.Api
{
    [Route("api/static-icons")]
    [ApiController]
    public class StaticIconsController : ControllerBase
    {
        private IconsDatabase Database;

        public static string BuildUrl(string IconName) => "/api/static-icons/" + IconName;

        public StaticIconsController(IconsDatabase Database)
        {
            this.Database = Database;
        }

        [HttpGet("{Name}")]
        public async Task<IActionResult> GetStaticIcon(string Name, int Size = 256)
        {
            var icon = await Database.Read("icon:" + Name);

            if (icon != null)
            {
                if (Array.IndexOf(IconsConstants.AvailableSize, Size) == -1)
                {
                    return BadRequest("The \"scale\" argument out of range.");
                }
                var iconStream = icon.Read(Size);
                return new FileStreamResult(iconStream, "image/png");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
