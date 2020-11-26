using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OwnHub.Preview.Icons;

namespace OwnHub.Server.Api
{
    [Route("api/static-icons")]
    [ApiController]
    public class StaticIconsController : ControllerBase
    {
        private readonly StaticIconsService staticIcons;

        public StaticIconsController(StaticIconsService staticIcons)
        {
            this.staticIcons = staticIcons;
        }

        public static string BuildUrl(string iconName)
        {
            return "/api/static-icons/" + iconName;
        }

        [HttpGet("{Name}")]
        public async Task<IActionResult> GetStaticIcon(string name, int size = IconsConstants.DefaultSize)
        {
            Stream? iconData = await staticIcons.GetIcon(name, size);

            if (iconData == null) return NotFound();
            
            if (Array.IndexOf(IconsConstants.AvailableSize, size) == -1)
                return BadRequest("The \"scale\" argument out of range.");
            return new FileStreamResult(iconData, "image/png");
        }
    }
}