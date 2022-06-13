using BepInEx;
using Bounce.Singletons;
using Bounce.Unmanaged;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        public static partial class Patches
        {
            public static List<CreatureBoardAsset> spawnList = new List<CreatureBoardAsset>();

            [HarmonyPatch(typeof(UI_AssetBrowserSlotItem), "Spawn")]
            public class PatchSpawn
            {
                public static bool Prefix(UI_AssetBrowserSlotItem __instance, NGuid ____nGuid)
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Plugin: Library Selection Made (Asset Id " + ____nGuid + ")"); }
                    return CustomAssetsPlugin.PreSpawnHandlerRouter(____nGuid, AssetDb.GetIndexData(____nGuid));
                }
            }

            /*
            [HarmonyPatch(typeof(CreatureManager), "AddCreature")]
            public static class PatcheAddCreaturew
            {
                public static bool Prefix(ref CreatureDataV2 creatureData, PlayerGuid[] owners, bool spawnedByLoad)
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Plugin: Mini Placed On Board (Id " + creatureData.CreatureId + " Of Asset Type " + creatureData.BoardAssetIds[0] + ")"); }
                    if (creatureData.Link != null && creatureData.Link.Contains("SpawnMode=CodeSpawn"))
                    {
                        // Code Generated Spawn
                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Code Spawn Of " + creatureData.CreatureId); }
                        creatureData.Link = creatureData.Link.Replace("SpawnMode=CodeSpawn", "");
                    }
                    else
                    {
                        // GUI Spawn
                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: GUI Spawn Of " + creatureData.CreatureId); }
                        spawnList.Add(creatureData, AssetDb.GetIndexData(creatureData.BoardAssetIds[0]));
                    }
                    return true;
                }
            }
            */

            [HarmonyPatch(typeof(CreatureBoardAsset), "OnBaseLoaded")]
            public static class PatcheOnBaseLoaded
            {
                public static bool Prefix(CreatureBoardAsset __instance)
                {
                    string nameBlock = (__instance.Name != null) ? __instance.Name : ((__instance.name != null) ? __instance.name : "(Unknown)");
                    nameBlock = Utility.GetCreatureName(nameBlock);
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Board Placement Of " + nameBlock); }
                    spawnList.Add(__instance);
                    return true;
                }
            }
        }
    }
}
