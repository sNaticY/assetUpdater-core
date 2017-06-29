using UnityEngine;
using System.Security.Cryptography;
using System.Text;

namespace Meow.AssetUpdater.Core
{
    public class BundleInfo
    {
        public string Name;

        public int Size;

        public string Md5Code;

        public string[] Dependencies;

    }
}