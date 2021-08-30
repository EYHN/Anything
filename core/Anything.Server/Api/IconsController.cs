using System;
using System.Threading.Tasks;
using Anything.Preview.Icons;
using Anything.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Anything.Server.Api
{
    [Route("api/icons")]
    [ApiController]
    public class IconsController : ControllerBase
    {
        private readonly Application _application;

        public IconsController(Application application)
        {
            _application = application;
        }

        public static string BuildUrl(string iconId)
        {
            return "/api/icons/" + Uri.EscapeDataString(iconId);
        }

        [HttpGet("{Name}")]
        public async Task<IActionResult> GetStaticIcon(string name, int? size)
        {
            var option = new IconImageOption();
            if (size != null)
            {
                option = option with { Size = size.Value };
            }

            var iconImage = await _application.PreviewService.GetIconImage(name, option);

#pragma warning disable IDISP004
            return new FileStreamResult(iconImage.GetStream(), iconImage.Format);
#pragma warning restore IDISP004
        }
    }
}
