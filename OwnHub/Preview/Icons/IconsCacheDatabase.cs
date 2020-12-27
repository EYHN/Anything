using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.File.Fork;
using OwnHub.Utils;

namespace OwnHub.Preview.Icons
{
    public class IconsCacheDatabase: FileForkDatabase
    {
        public IconsCacheDatabase(string databaseFile) : base(databaseFile)
        {
            
        }

        public async Task<IconsCache?> GetIcons(string parentFile)
        {
            return await GetFork<IconsCache>(parentFile);
        }

        public async Task<IconsCache> AddIcons(string parentFile, string etag)
        {
            IconsCache icons = new IconsCache(parentFile, etag);
            await Add(icons, "Icons" + ":" + parentFile);
            return icons;
        }

        public async Task<IconsCache> UpdateIcons(IconsCache icons, string etag)
        {
            await icons.UpdateEtag(etag);
            return icons;
        }

        public async Task<IconsCache> GetOrAddOrUpdate(string parentFile, string etag)
        {
            IconsCache? old = await GetIcons(parentFile);

            if (old != null)
            {
                if (old.Etag == etag) return old;
                return await UpdateIcons(old, etag);
            }
            else
            {
                return await AddIcons(parentFile, etag);
            }
        }
    }
    
    public class IconsCacheItem {
        public int Size { get; set; }
        public string Format { get; set; } = null!;
        public long DataId { get; set; }
    }

    public class IconsCachePayload
    {
        public string Etag { get; set; } = null!;
        public List<IconsCacheItem> Items { get; set; } = new List<IconsCacheItem>();
    }

    public sealed class IconsCache: FileFork<IconsCachePayload>
    {
        public string Etag => Payload.Etag;
        public List<IconsCacheItem> Items => Payload.Items;
        public override IconsCachePayload Payload { get; set; }
        
        public IconsCache(string parentFile) : base(parentFile)
        {
            Payload = null!;
        }

        public IconsCache(string parentFile, IconsCachePayload payload) : base(parentFile)
        {
            Payload = payload;
        }

        public IconsCache(string parentFile, string etag): this(parentFile, new IconsCachePayload(){ Etag = etag })
        {
        }

        public async Task AddIcon(int size, string format, Stream data)
        {
            if (HasSize(size))
            {
                throw new InvalidOperationException($"Can't add icon, size {size}x{size} already exists.");
            }
            Data iconData = await AddData(data, $"{this.ParentFile} - icon cache - {size}x{size} - {format}");
            Items.Add(new IconsCacheItem()
            {
                Size = size,
                Format = format,
                DataId = iconData.Id
            });
            await SaveChanges();
        }

        public bool HasSize(int size)
        {
            return Items.Any(item => item.Size == size);
        }

        public async Task<Stream?> GetIconData(int size)
        {
            IconsCacheItem? icon = Payload.Items.FirstOrDefault((item) => item.Size == size);
            return icon != null ? await GetData(icon.DataId)!.Read() : null;
        }

        public async Task UpdateEtag(string etag)
        {
            Payload.Etag = etag;
            Payload.Items.Clear();
            await SaveChanges();
            await DeleteAllData();
        }
    }
}