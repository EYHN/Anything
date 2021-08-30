using System;
using System.Threading.Tasks;
using Anything.Preview.Thumbnails;
using Anything.Server.Models;
using Anything.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Anything.Server.Api
{
    [Route("api/thumbnails")]
    [ApiController]
    public class ThumbnailsController : ControllerBase
    {
        private readonly Application _application;

        public ThumbnailsController(Application application)
        {
            _application = application;
        }

        public static string BuildUrl(Url fileUrl)
        {
            return $"/api/thumbnails?url={Uri.EscapeDataString(fileUrl.ToString())}";
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicIcon(string? url, int size = 256)
        {
            if (url == null)
            {
                return BadRequest("The \"url\" argument out of range.");
            }

            var thumbnail = await _application.PreviewService.GetThumbnail(
                Utils.Url.Parse(url),
                new ThumbnailOption { Size = size });

            if (thumbnail != null)
            {
#pragma warning disable IDISP004
                return new FileStreamResult(thumbnail.GetStream(), thumbnail.ImageFormat);
#pragma warning restore IDISP004
            }

            return NoContent();
        }
    }
}
