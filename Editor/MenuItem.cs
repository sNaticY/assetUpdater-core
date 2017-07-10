using System.IO;
using UnityEditor;
using UnityEngine;
using Meow.AssetUpdater.Core;

namespace Meow.AssetUpdater.Editor
{
    public static class MenuItems
    {
        [MenuItem("Window/Meow Asset Updater/Simulation Mode")]
        public static void ToggleSimulationMode ()
        {
            MainUpdater.IsSimulationMode = !MainUpdater.IsSimulationMode;
        }
	
        [MenuItem("Window/Meow Asset Updater/Simulation Mode", true)]
        public static bool ToggleSimulationModeValidate ()
        {
            Menu.SetChecked("Window/Meow Asset Updater/Simulation Mode", MainUpdater.IsSimulationMode);
            return true;
        }
        
        [MenuItem("Window/Meow Asset Updater/Settings")]
        public static void Init()
        {
            EditorUtility.SetDirty(Settings.Instance);
            Selection.activeObject = Settings.Instance;
        }
    }
}