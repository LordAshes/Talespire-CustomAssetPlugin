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

        #region Post Spawn Router

        public IEnumerator PostSpawnHandlerRouter(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Mini "+creatureData.CreatureId+" Placed ("+databaseData.Name+")"); }
            yield return new WaitForSeconds(0.1f);
            if (creatureData.Link == "SpawnMode=CodeSpawn")
            {
                // Supress Post Spawn Callback
                creatureData.Link = "";
            }
            else
            {
                // Trigger Post Spawn Callback
                Dictionary<string, string> tags = new Dictionary<string, string>();
                // Build Tags Dictionary
                foreach (string item in ((AssetDb.DbEntry)inputs[1]).Tags)
                {
                    if(item.Contains(":")) { tags.Add(item.Substring(0,item.IndexOf(":")).ToUpper(), item.Substring(item.IndexOf(":")+1)); } else { tags.Add(item, item); }
                }
                if (!tags.ContainsKey("KIND")) { tags.Add("KIND", "Creature"); }
                string kind = Helpers.ModifyKindBasedOnModifier(tags["KIND"]);
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Asset is kind " + tags["KIND"] + " being treated as " + kind); }
                // Trigger Remote Post Spawn Callback
                CustomAssetsPlugin._self.StartCoroutine("PostSpawn" + kind + "Handler", new object[] { creatureData, databaseData, tags});
                if (creatureData.Link!=null && creatureData.Link.Trim() != "") 
                {
                    // Trigger Remote Post Spawn Callback
                    try
                    {
                        // AssetDataPlugin.SendInfo(creatureData.Link, DateTime.UtcNow);
                    }
                    catch(Exception)
                    {
                        Debug.LogWarning("Custom Assets Plugin: AssetDataPlugin Not Available. Notification Ignored"); 
                    }
                }
            }
        }

        #endregion

        #region Post Spawn Handlers

        public IEnumerator PostSpawnAudioHandler(object[] inputs)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Audio"); }
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            RequestHandler.Audio(creatureData.CreatureId, 1);
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnAuraHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Aura "+creatureData.CreatureId); }
            // Apply custom shader
            StartCoroutine(ApplyCustomShader(Utility.GetAssetLoader(creatureData.CreatureId), tags));
            // Attach aura to selected mini
            GameObject target = Utility.GetAssetLoader(LocalClient.SelectedCreatureId);
            GameObject aura = Utility.GetAssetLoader(creatureData.CreatureId);
            if(target!=null && aura!=null)
            {
                aura.transform.position = target.transform.position;
                aura.transform.rotation = target.transform.rotation;
                aura.transform.SetParent(target.transform);
            }
            else
            {
                SystemMessage.DisplayInfoText("Unable To Attach Aura");
            }
            yield return new WaitForSeconds(0.1f);

        }

        public IEnumerator PostSpawnCreatureHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Creature " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnEffectHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Effect " + creatureData.CreatureId); }
            StartCoroutine(ApplyCustomShader(Utility.GetAssetLoader(creatureData.CreatureId), tags));
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnEncounterHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Encounter " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnFilterHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Filter " + creatureData.CreatureId); }
            StartCoroutine(ApplyCustomShader(Utility.GetAssetLoader(creatureData.CreatureId), tags));
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnPropHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Prop " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnSlabHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Slab " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnTileHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Tile " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnTransformHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Processing Transform " + creatureData.CreatureId); }
            CreatureBoardAsset assetOrig;
            CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out assetOrig);
            CreatureBoardAsset assetNew;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out assetNew);
            GameObject go = Utility.GetAssetLoader(creatureData.CreatureId);
            if(assetOrig!=null && assetNew!=null)
            {
                Vector3 pos = assetOrig.CorrectPos;
                Quaternion rot = assetOrig.CorrectRotation;
                float height = assetOrig.CorrectHeight;
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Removing Old Asset"); }
                assetOrig.RequestDelete();
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Post Spawn Handler: Adjusting New Asset"); }
                assetNew.CorrectPos = pos;
                assetNew.CorrectRotation = rot;
                assetNew.CorrectHeight = height;
            }
            yield return new WaitForSeconds(0.1f);
        }

        #endregion

        #region Post Spawn Helpers

        private IEnumerator ApplyCustomShader(GameObject asset, Dictionary<string,string> tags)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Applying AssetBundle Materials/Shader To "+asset.name); }
            if (tags.ContainsKey("ASSETBUNDLE"))
            {
                string assetBundleFile = tags["ASSETBUNDLE"];
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: AssetFile Location Is " + assetBundleFile); }
                AssetBundle assetBundle = null;
                GameObject prefab = null;
                foreach(AssetBundle ab in AssetBundle.GetAllLoadedAssetBundles())
                {
                    if (ab.name == System.IO.Path.GetFileNameWithoutExtension(tags["ASSETBUNDLE"])) { assetBundle = ab; break; }
                }
                if (assetBundle != null) { assetBundle.Unload(false); }
                assetBundle = AssetBundle.LoadFromFile(assetBundleFile);
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: AssetFile Reference Obtained (" + Convert.ToString(assetBundle) + ")"); }
                prefab = assetBundle.LoadAsset<GameObject>(System.IO.Path.GetFileNameWithoutExtension(tags["ASSETBUNDLE"]));
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Prefab '"+ System.IO.Path.GetFileNameWithoutExtension(tags["ASSETBUNDLE"]) + "' Reference Obtained (" + Convert.ToString(prefab) + ")"); }
                if (asset != null)
                {
                    if (prefab != null)
                    {
                        for (int r = 0; r < prefab.GetComponentsInChildren<MeshRenderer>().Count(); r++)
                        {
                            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Copying Material " + prefab.GetComponentsInChildren<MeshRenderer>()[r].material.name+" With Shader Name "+ prefab.GetComponentsInChildren<MeshRenderer>()[r].material.shader.name); }
                            asset.GetComponentsInChildren<MeshRenderer>()[r].material = prefab.GetComponentsInChildren<MeshRenderer>()[r].material;
                            for(int mat = 0; mat < asset.GetComponentsInChildren<MeshRenderer>()[r].materials.Length; mat++)
                            {
                                asset.GetComponentsInChildren<MeshRenderer>()[r].materials[mat] = prefab.GetComponentsInChildren<MeshRenderer>()[r].materials[mat];
                            }
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Source Prefab Is Null"); }
                    }
                }
                else
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Asset Is Null"); }
                }
            }
            else
            {
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: AssetFile Location Key Is Missing"); }
            }
        }

        #endregion
    }
}
