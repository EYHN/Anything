using System;
using System.Threading.Tasks;
using Anything.FileSystem;
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

        public static string BuildUrl(FileHandle fileHandle)
        {
            return $"/api/thumbnails?fileHandle={Uri.EscapeDataString(fileHandle.Identifier)}";
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicIcon(string? fileHandle, int size = 256)
        {
            if (fileHandle == null)
            {
                return BadRequest("The \"fileHandle\" argument out of range.");
            }

            var thumbnail = await _application.PreviewService.GetThumbnail(
                new FileHandle(fileHandle),
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
