using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;
using StagingBox.Server.Models;
using StagingBox.Utils;

namespace StagingBox.Server.Api
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
            return $"/api/thumbnails?url={HttpUtility.UrlEncode(fileUrl.ToString())}";
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicIcon(string? url, int size = 256)
        {
            if (url == null)
            {
                return BadRequest("The \"url\" argument out of range.");
            }

            var thumbnail = await _application.PreviewService.GetThumbnails(
                Utils.Url.Parse(url),
                new ThumbnailOption() { Size = size });

            if (thumbnail != null)
            {
                return new FileStreamResult(thumbnail.GetStream(), thumbnail.ImageFormat);
            }

            return NoContent();
        }
    }
}
