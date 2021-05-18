using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using StagingBox.File;
using StagingBox.Preview.Icons;

namespace StagingBox.Server.Api
{
    [Route("api/dynamic-icons")]
    [ApiController]
    public class DynamicIconsController : ControllerBase
    {
        private readonly DynamicIconsService dynamicIconsService;
        private readonly IFileSystem fileSystem;

        public DynamicIconsController(DynamicIconsService dynamicIconsService, IFileSystem fileSystem)
        {
            this.dynamicIconsService = dynamicIconsService;
            this.fileSystem = fileSystem;
        }

        public static string BuildUrl(string path)
        {
            return $"/api/dynamic-icons?path={HttpUtility.UrlEncode(path)}";
        }

        public static string BuildUrl(IFile file)
        {
            return $"/api/dynamic-icons?path={HttpUtility.UrlEncode(file.Path)}";
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicIcon(string path, int size = 256)
        {
            if (path == null) return BadRequest("The \"Path\" argument out of range.");

            Stream? iconStream = await dynamicIconsService.Render(fileSystem.Open(path), size);

            if (iconStream != null)
                return new FileStreamResult(iconStream, "image/png");
            return NoContent();
        }
    }
}
