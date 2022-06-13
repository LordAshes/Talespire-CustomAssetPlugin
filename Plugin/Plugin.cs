using BepInEx;
using BepInEx.Configuration;
using Bounce.Unmanaged;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CustomAssetsLibrary.CustomAssetLib.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("org.lordashes.plugins.assetdata", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Assets Plugin Plug-In";
        public const string Guid = "org.lordashes.plugins.customassets";
        public const string Version = "1.5.0.0";

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

        public class KeyboardHandler
        {
            public KeyboardShortcut trigger { get; set; }
            public string handlerMethod { get; set; }
            public object handlerParameter { get; set; }
        }

        // Configuration
        private ConfigEntry<OperationMode> operationMode { get; set; }
        private ConfigEntry<DiagnosticMode> diagnosticMode { get; set; }
        private ConfigEntry<bool> deleteJSONIndexFiles { get; set; }

        public static CustomAssetsPlugin _self = null;

        public string taleWwaverFolder = BepInEx.Paths.GameRootPath + "\\Taleweaver\\";
        public string pluginFolder = BepInEx.Paths.PluginPath + "\\Testing\\";

        public static List<KeyCode> activeModifierKeys = new List<KeyCode>();

        public static Dictionary<string, KeyboardHandler> keyboardHandlers = new Dictionary<string, KeyboardHandler>()
        {
            {"Play Animaton 01", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 1 } },
            {"Play Animaton 02", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 2 } },
            {"Play Animaton 03", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 3 } },
            {"Play Animaton 04", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 4 } },
            {"Play Animaton 05", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 5 } },
            {"Play Animaton 06", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 6 } },
            {"Play Animaton 07", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 7 } },
            {"Play Animaton By Name", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 0 } },
            {"Play Audio", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl), handlerMethod = "Audio", handlerParameter = -1 } },
            {"Stop All", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl), handlerMethod = "Stop", handlerParameter = -1 } },
            {"Analyze Game Object", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.A, KeyCode.RightControl), handlerMethod = "Analyze", handlerParameter = -1 } },
        };

        void Awake()
        {
            UnityEngine.Debug.Log("Custom Asset Plugin: "+this.GetType().AssemblyQualifiedName+" Active.");

            _self = this;
            
            operationMode = Config.Bind("Settings", "Build Index Mode", CustomAssetsPlugin.OperationMode.rebuildIndexIfMissing);
            diagnosticMode = Config.Bind("Troubleshooting", "Diagnostic Mode", DiagnosticMode.high);
            deleteJSONIndexFiles = Config.Bind("Troubleshooting", "Delete Temporary JSON Index Files", false);

            for(int t=0; t<keyboardHandlers.Count; t++)
            {
                keyboardHandlers[keyboardHandlers.ElementAt(t).Key].trigger = Config.Bind("Hotkeys", keyboardHandlers.ElementAt(t).Key, keyboardHandlers.ElementAt(t).Value.trigger).Value;
            }

            new Harmony(Guid).PatchAll();

            AssetDataPluginSoftDependency.Initialize();

            if (operationMode.Value != OperationMode.rebuildNever)
            {
                Setup.RegisterAssets();
            }

            Utility.PostOnMainPage(this.GetType());
        }

        void Update()
        {
            Helpers.CheckModifierKeyStates(ref activeModifierKeys);

            if (Utility.isBoardLoaded())
            {
                // Board is loaded
                if (CustomAssetsLibrary.Patches.AssetDbOnSetupInternalsPatch.HasSetup)
                {
                    for (int spawnId = 0; spawnId < Patches.spawnList.Count; spawnId++)
                    {
                        CreatureBoardAsset asset = null;
                        // Asset is available
                        if (CreaturePresenter.TryGetAsset(Patches.spawnList[spawnId].CreatureId, out asset))
                        {
                            // Asset has finished dropping in
                            if (asset.HasDroppedIn)
                            {
                                // Asset had loaded
                                if (Utility.GetAssetLoader(Patches.spawnList[spawnId].CreatureId) != null)
                                {
                                    try
                                    {
                                        // Creasture data accessible
                                        CreatureDataV2 creatureData;
                                        if (CreatureManager.TryGetCreatureData(Patches.spawnList[spawnId].CreatureId, out creatureData))
                                        {
                                            AssetDb.DbEntry databaseData = AssetDb.GetIndexData(Patches.spawnList[spawnId].BoardAssetId);
                                            StartCoroutine("PostSpawnHandlerRouter", new object[] { creatureData, databaseData });
                                            Patches.spawnList.RemoveAt(spawnId);
                                            spawnId--;
                                        }
                                    }
                                    catch (Exception) {; }
                                }
                            }
                        }
                    }
                }

                foreach(KeyboardHandler handler in keyboardHandlers.Values)
                {
                    if(Utility.StrictKeyCheck(handler.trigger))
                    {
                        if (AssetDataPluginSoftDependency.SetInfo!=null)
                        {
                            // Remote request for functionality
                            Debug.Log("Custom Asset Plugin: (Remote Mode) User Requested Setting Of " + CustomAssetsPlugin.Guid + "." + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                            AssetDataPluginSoftDependency.SetInfo.Invoke(null, new object[] { LocalClient.SelectedCreatureId.ToString(), CustomAssetsPlugin.Guid + "." + handler.handlerMethod, Convert.ToString(handler.handlerParameter)+"@"+DateTime.UtcNow.ToString(), false});
                        }
                        else
                        {
                            // Local request for functionality
                            Debug.Log("Custom Asset Plugin: (Local Mode Only) User Requested Execution Of " + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                            typeof(CustomAssetsPlugin.RequestHandler).GetMethod(handler.handlerMethod).Invoke(null, new object[] { LocalClient.SelectedCreatureId, handler.handlerParameter });
                        }
                    }
                }
            }
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra)
            {
                if (Patches.spawnList.Count > 0) { Debug.Log("Custom Asset Plugin: Backlog Entries = "+Patches.spawnList.Count); }
            }
        }

        public static DiagnosticMode Diagnostics()
        {
            return CustomAssetsPlugin._self.diagnosticMode.Value;
        }

        public static bool DeleteJSONIndexFiles()
        {
            return CustomAssetsPlugin._self.deleteJSONIndexFiles.Value;
        }

        private static void AnalyzeGameObject(GameObject go)
        {
            foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                UnityEngine.Debug.Log("Custom Asset Plugin: Mesh Renderer '" + mr.name + "' uses material with shader '" + mr.material.shader.name + "'");
                foreach (Material mat in mr.materials)
                {
                    UnityEngine.Debug.Log("Custom Asset Plugin: Mesh Renderer '" + mr.name + "' has material with shader '"+mat.shader.name+"'");
                }
            }
            foreach (SkinnedMeshRenderer mr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                UnityEngine.Debug.Log("Custom Asset Plugin: Mesh Renderer '" + mr.name + "' uses material with shader '" + mr.material.shader.name + "'");
                foreach (Material mat in mr.materials)
                {
                    UnityEngine.Debug.Log("Custom Asset Plugin: Mesh Renderer '" + mr.name + "' has material with shader '" + mat.shader.name + "'");
                }
            }
            foreach(Transform trans in go.transform.Children())
            {
                if (trans.gameObject != null)
                {
                    AnalyzeGameObject(trans.gameObject);
                }
            }
        }
    }
}
