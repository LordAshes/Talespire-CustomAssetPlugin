using BepInEx;
using Bounce.Unmanaged;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        public static class Helpers
        {
            public static void SpawnPrevent()
            {
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Preventing Asset Spawn"); }
                if (SingletonBehaviour<BoardToolManager>.HasInstance)
                {
                    SingletonBehaviour<BoardToolManager>.Instance.SwitchToTool<DefaultBoardTool>(BoardToolManager.Type.Normal);
                }
            }

            public static void CheckModifierKeyStates(ref List<KeyCode> activeModifierKeys)
            {
                bool changed = false;
                foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.RightAlt, KeyCode.RightControl, KeyCode.RightShift })
                {
                    if (Input.GetKeyDown(modifier) && !activeModifierKeys.Contains(modifier)) { activeModifierKeys.Add(modifier); changed = true; }
                    if (Input.GetKeyUp(modifier) && activeModifierKeys.Contains(modifier)) { activeModifierKeys.Remove(modifier); changed = true; }
                }
                if(changed)
                {
                    string states = "L:";
                    if (activeModifierKeys.Contains(KeyCode.LeftAlt)) { states = states + "A"; } else { states = states + "-"; }
                    if (activeModifierKeys.Contains(KeyCode.LeftControl)) { states = states + "C"; } else { states = states + "-"; }
                    if (activeModifierKeys.Contains(KeyCode.LeftShift)) { states = states + "S"; } else { states = states + "-"; }
                    states = states + " R:";
                    if (activeModifierKeys.Contains(KeyCode.RightAlt)) { states = states + "A"; } else { states = states + "-"; }
                    if (activeModifierKeys.Contains(KeyCode.RightControl)) { states = states + "C"; } else { states = states + "-"; }
                    if (activeModifierKeys.Contains(KeyCode.RightShift)) { states = states + "S"; } else { states = states + "-"; }
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Modifiers = "+states); }
                }
            }

            public static string ModifyKindBasedOnModifier(string kind)
            {
                // Debug.Log("Custom Assets Plugin: Modifiers = " + JsonConvert.SerializeObject(activeModifierKeys));
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.LeftShift)) { return "Creature"; }
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.RightShift)) { return "Transform"; }
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.LeftControl)) { return "Effect"; }
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.RightControl)) { return "Aura"; }
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.LeftAlt)) { return "Audio"; }
                if (CustomAssetsPlugin.activeModifierKeys.Contains(KeyCode.RightAlt)) { return "Filter"; }
                return kind;
            }

            public static CreatureGuid SpawnCreature(CreatureDataV2 creatureData)
            {
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Creating Mini Of Type " + creatureData.BoardAssetIds[0] + " Which " + (creatureData.ExplicitlyHidden ? "Is" : "Is Not") + " Hidden"); }

                Quaternion q = Quaternion.Euler(new Vector3(creatureData.Rotation.x.ToDegrees(), creatureData.Rotation.y.ToDegrees(), creatureData.Rotation.z.ToDegrees()));

                Debug.Log("Custom Assets Plugin: SpawnResult=" + Convert.ToString(CreatureManager.TryCreateAndAddNewCreature(creatureData, creatureData.Position, new Unity.Mathematics.quaternion(q.x, q.y, q.z, q.w), creatureData.Flying, creatureData.ExplicitlyHidden, false)));

                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Registering mini for saving"); }
                BuildingBoardTool.RecordInBuildHistory(creatureData.GetActiveBoardAssetId());

                return creatureData.CreatureId;
            }

            public static NGuid FindAssetId(string assetName)
            {
                List<AssetDb.DbGroup> groups = new List<AssetDb.DbGroup>();
                groups.AddRange(AssetDb.GetGroupsByKind(AssetDb.DbEntry.EntryKind.Creature));
                groups.AddRange(AssetDb.GetGroupsByKind(AssetDb.DbEntry.EntryKind.Prop));
                groups.AddRange(AssetDb.GetGroupsByKind(AssetDb.DbEntry.EntryKind.Tile));
                foreach (AssetDb.DbGroup group in groups)
                {
                    foreach (AssetDb.DbEntry item in group.Entries)
                    {
                        if (item.Name.ToUpper() == assetName.ToUpper()) { return item.Id; }
                    }
                }
                return NGuid.Empty;
            }
        }
    }
}
