﻿using System.IO;

namespace OwnHub.Preview.Icons
{
    public interface IIcon
    {
        public string ImageFormat { get; }

        public int Size { get; }

        public Stream GetStream();
    }
}
