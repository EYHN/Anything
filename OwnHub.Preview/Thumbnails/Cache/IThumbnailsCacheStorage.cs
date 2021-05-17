﻿using System.Threading.Tasks;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails.Cache
{
    public interface IThumbnailsCacheStorage
    {
        public ValueTask Cache(Url url, string tag, IThumbnail thumbnail);

        public ValueTask<IThumbnail[]> GetCache(Url url, string tag);

        public ValueTask Delete(Url url);
    }
}
