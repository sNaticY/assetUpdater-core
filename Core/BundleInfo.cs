using System;

namespace Meow.AssetUpdater.Core
{
    [Serializable]
    public class BundleInfo
    {
        public string Name;

        public int Size;

        public string Md5Code;

        public string[] Dependencies;

    }
}