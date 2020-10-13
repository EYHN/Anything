using OwnHub.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons.Renderers
{
    public interface IDynamicIconsRenderer
    {
        public Task<bool> Render(IconsRenderContext ctx, DynamicIconsRenderInfo info);
        public bool IsSupported(IFile file);
    }

    public class DynamicIconsRenderInfo
    {
        public IFile file { get; set; }
    }
}
