using System;
using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Server.Models;
using Anything.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Anything.Server.Api
{
    [Route("api/static-icons")]
    [ApiController]
    public class IconsController : ControllerBase
    {
        private readonly Application _application;

        public IconsController(Application application)
        {
            _application = application;
        }

        public static string BuildUrl(Url fileUrl)
        {
            // return "/api/static-icons/" + iconName;
            throw new NotImplementedException();
        }

        [HttpGet("{Name}")]
        public async Task<IActionResult> GetStaticIcon(string name, int size = IconsConstants.DefaultSize)
        {
            // Stream? iconData = await staticIcons.GetIcon(name, size);
            //
            // if (iconData == null) return NotFound();
            //
            // if (Array.IndexOf(IconsConstants.AvailableSize, size) == -1)
            //     return BadRequest("The \"scale\" argument out of range.");
            // return new FileStreamResult(iconData, "image/png");
            throw new NotImplementedException();
        }
    }
}
