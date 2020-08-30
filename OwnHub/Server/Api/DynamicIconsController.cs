using System;
using System.Threading.Tasks;
using OwnHub.File;
using OwnHub.Preview.Icons;
using Microsoft.AspNetCore.Mvc;

namespace OwnHub.Server.Api
{
    [Route("api/dynamic-icons")]
    [ApiController]
    public class DynamicIconsController : ControllerBase
    {
        private DynamicIconsService DynamicIconsService;
        private IFileSystem FileSystem;

        public DynamicIconsController(DynamicIconsService DynamicIconsService, IFileSystem FileSystem)
        {
            this.DynamicIconsService = DynamicIconsService;
            this.FileSystem = FileSystem;
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicIcon(string Path, int Size = 256)
        {
            if (Path == null)
            {
                return BadRequest("The \"Path\" argument out of range.");
            }

            var iconStream = await DynamicIconsService.Render(FileSystem.Open(Path), Size);

            if (iconStream != null)
            {
                return new FileStreamResult(iconStream, "image/png");
            }
            else
            {
                return NoContent();
            }
        }
    }
}
