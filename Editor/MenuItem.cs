using System.IO;
using UnityEditor;
using UnityEngine;
using Meow.AssetUpdater.Core;

namespace Meow.AssetUpdater.Editor
{
    public static class MenuItems
    {
        [MenuItem("Window/AssetUpdater Setting")]
        public static void Init()
        {
            Selection.activeObject = Settings.Instance;
        }
    }
}