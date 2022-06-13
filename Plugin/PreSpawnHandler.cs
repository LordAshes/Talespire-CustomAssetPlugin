using BepInEx;
using Bounce.Unmanaged;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {

        #region Pre Spawn Router

        public static bool PreSpawnHandlerRouter(NGuid guid, AssetDb.DbEntry entry)
        {
            // Trigger Post Spawn Callback
            Dictionary<string, string> tags = new Dictionary<string, string>();
            // Build Tags Dictionary
            foreach (string item in entry.Tags)
            {
                if (item.Contains(":")) { tags.Add(item.Substring(0, item.IndexOf(":")).ToUpper(), item.Substring(item.IndexOf(":") + 1)); } else { tags.Add(item, item); }
            }
            if (!tags.ContainsKey("KIND")) { tags.Add("KIND", "Creature"); }
            string kind = Helpers.ModifyKindBasedOnModifier(tags["KIND"]);
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Router: Asset is kind "+tags["KIND"]+" being treated as "+kind); }
            // Trigger Remote Pre Spawn Callback
            return (bool)typeof(CustomAssetsPlugin).GetMethod("PreSpawn" + kind + "Handler").Invoke(null, new object[] { guid, entry, tags });            
        }

        #endregion

        #region Pre Spawn Handlers

        public static bool PreSpawnAudioHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Audio Of Type "+nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnAuraHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Aura Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            _self.StartCoroutine(SpawnCreatureByNGuid(nguid));
            return false;
        }

        public static bool PreSpawnCreatureHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Creature Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnEffectHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Effect Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnEncounterHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Encounter Of Type " + nguid.ToString()); }
            SystemMessage.DisplayInfoText("Encounter Type Is Not Yet Supported");
            Helpers.SpawnPrevent();
            return false;
        }

        public static bool PreSpawnFilterHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Filter Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            _self.StartCoroutine(SpawnCreatureByNGuid(nguid));
            return false;
        }

        public static bool PreSpawnPropHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Prop Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnSlabHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Slab Of Type " + nguid.ToString()); }
            SystemMessage.DisplayInfoText("Slab Type Is Not Yet Supported");
            Helpers.SpawnPrevent();
            return false;
        }

        public static bool PreSpawnTileHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Tile Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnTransformHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Processing Transform Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            _self.StartCoroutine(SpawnCreatureByNGuid(nguid));
            return false;
        }

        #endregion

        #region Pre Spawn Helpers

        private static IEnumerator SpawnCreatureByNGuid(NGuid nguid)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Pre Spawn Handler: Spawning Creature By NGuid"); }
            yield return new WaitForSeconds(0.1f);
            CreatureBoardAsset asset;
            CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
            Vector3 spawnPos = (asset != null) ? asset.CorrectPos : Vector3.zero;
            Quaternion spawnRot = (asset != null) ? asset.CorrectRotation : Quaternion.Euler(Vector3.zero);
            Helpers.SpawnCreature(new CreatureDataV2()
            {
                CreatureId = new CreatureGuid(new Bounce.Unmanaged.NGuid(System.Guid.NewGuid())),
                BoardAssetIds = new NGuid[] { nguid },
                Position = spawnPos,
                Rotation = Bounce.Mathematics.bam3.FromEulerDegrees(spawnRot.eulerAngles),
                ExplicitlyHidden = false,
                Flying = false
            });
        }

        #endregion
    }
}
