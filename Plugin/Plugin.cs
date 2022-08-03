using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;


namespace PluginMasters
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CustomAssetsLibrary.CustomAssetLib.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Assets Library Plugin";
        public const string Guid = "org.lordashes.plugins.customassetslibrary";
        public const string Version = "2.1.0.0";

        // Public Enum
        public enum DiagnosticMode
        {
            none = 0,
            low = 1,
            high = 2,
            ultra = 3
        }

        public enum OperationMode
        {
            rebuildIndexAlways = 0,
            rebuildIndexIfMissing = 1,
            rebuildNever = 2
        }

        // Configuration
        private OperationMode operationMode { get; set; }
        private DiagnosticMode diagnosticMode { get; set; }
        private bool deleteJSONIndexFiles { get; set; }

        public static CustomAssetsPlugin _self = null;

        void Awake()
        {
            _self = this;
            
            operationMode = Config.Bind("Settings", "Build Index Mode", CustomAssetsPlugin.OperationMode.rebuildIndexIfMissing).Value;
            diagnosticMode = Config.Bind("Troubleshooting", "Diagnostic Mode", DiagnosticMode.ultra).Value;
            deleteJSONIndexFiles = Config.Bind("Troubleshooting", "Delete Temporary JSON Index Files", false).Value;

            Debug.LogWarning("Custom Asset Library Plugin: This is a test version of CALP. Setting diagnostioc mode to 'Ultra'");
            diagnosticMode = DiagnosticMode.ultra;

            UnityEngine.Debug.Log("Custom Asset Library Plugin: " + this.GetType().AssemblyQualifiedName + " Active. (Diagnostic Mode = "+diagnosticMode.ToString()+")");

            if (operationMode != OperationMode.rebuildNever)
            {
                Setup.RegisterAssets();
            }

            Utility.PostOnMainPage(this.GetType());
        }

        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                // Board is loaded

                if (Setup.violations.Count > 0)
                {
                    SystemMessage.DisplayInfoText("Custom Asset Library Plugin:\r\nPrime Directive Violation(s)!\r\nSee Log For Violations List", 10f);
                    Debug.LogWarning("Custom Asset Plugin: List Of Prime Directive Violations:");
                    foreach (string violation in Setup.violations)
                    {
                        Debug.LogWarning("Custom Asset Plugin: -> " + violation);
                    }
                    Setup.violations.Clear();
                }
            }
        }

        public static DiagnosticMode Diagnostics()
        {
            return CustomAssetsPlugin._self.diagnosticMode;
        }

        public static bool DeleteJSONIndexFiles()
        {
            return CustomAssetsPlugin._self.deleteJSONIndexFiles;
        }
    }
}
