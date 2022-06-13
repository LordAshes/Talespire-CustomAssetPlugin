using BepInEx;
using BepInEx.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LordAshes
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        public static class Utility
        {
            public static void PostOnMainPage(System.Reflection.MemberInfo plugin)
            {
                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    try
                    {
                        if (scene.name == "UI")
                        {
                            TextMeshProUGUI betaText = GetUITextByName("BETA");
                            if (betaText)
                            {
                                betaText.text = "INJECTED BUILD - unstable mods";
                            }
                        }
                        else
                        {
                            TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                            if (modListText)
                            {
                                BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(plugin, typeof(BepInPlugin));
                                if (modListText.text.EndsWith("</size>"))
                                {
                                    modListText.text += "\n\nMods Currently Installed:\n";
                                }
                                modListText.text += "\nLord Ashes' " + bepInPlugin.Name + " - " + bepInPlugin.Version;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                };
            }

            /// <summary>
            /// Function to check if the board is loaded
            /// </summary>
            /// <returns></returns>
            public static bool isBoardLoaded()
            {
                return CameraController.HasInstance && BoardSessionManager.HasInstance && !BoardSessionManager.IsLoading;
            }

            /// <summary>
            /// Method to properly evaluate shortcut keys. 
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public static bool StrictKeyCheck(KeyboardShortcut check)
            {
                if (!check.IsUp()) { return false; }
                foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift, KeyCode.RightShift })
                {
                    if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
                }
                return true;
            }

            /// <summary>
            /// Method to generate a Guid from a string
            /// </summary>
            /// <param name="input">Text</param>
            /// <returns>Guid based on the input text</returns>
            public static Guid GuidFromString(string input)
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                    return new Guid(hash);
                }
            }

            /// <summary>
            /// Method to obtain the Base Loader Game Object based on a CreatureGuid
            /// </summary>
            /// <param name="cid">Creature Guid</param>
            /// <returns>BaseLoader Game Object</returns>
            public static GameObject GetBaseLoader(CreatureGuid cid)
            {
                CreatureBoardAsset asset = null;
                CreaturePresenter.TryGetAsset(cid, out asset);
                if (asset != null)
                {
                    Type cba = typeof(CreatureBoardAsset);
                    foreach (FieldInfo fi in cba.GetRuntimeFields())
                    {
                        if (fi.Name == "_base")
                        {
                            CreatureBase obj = (CreatureBase)fi.GetValue(asset);
                            return obj.transform.GetChild(0).gameObject;
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// Method to obtain the Asset Loader Game Object based on a CreatureGuid
            /// </summary>
            /// <param name="cid">Creature Guid</param>
            /// <returns>AssetLoader Game Object</returns>
            public static GameObject GetAssetLoader(CreatureGuid cid)
            {
                CreatureBoardAsset asset = null;
                CreaturePresenter.TryGetAsset(cid, out asset);
                if (asset != null)
                {
                    string blockName = (asset.Name != null) ? asset.Name : ((asset.name != null) ? asset.name : "(Unknown)");
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: GetAssetLoader is searching " + Utility.GetCreatureName(blockName) + " hierarchy..."); }
                    Type cba = typeof(CreatureBoardAsset);
                    foreach (FieldInfo fi in cba.GetRuntimeFields())
                    {
                        // Find hierarchy starting point
                        if (fi.Name == "_creatureRoot")
                        {
                            Transform obj = (Transform)fi.GetValue(asset);
                            // Look for each step in the hierarchy
                            foreach (string step in new string[] { "CreatureMorph(Clone)", "AssetLoader" })
                            {
                                bool found = false;
                                for (int c = 0; c < obj.childCount; c++)
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: Found '"+obj.GetChild(c).name+"' while seeking step '"+step+"'"); }
                                    if ((obj.GetChild(c).name == step) || (step == "")) { obj = obj.GetChild(c); found = true; break; }
                                }
                                // Current step was not found, return null
                                if (found == false) 
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Plugin: GetAssetLoader didn't find '"+step+"' in " + Utility.GetCreatureName(blockName) + " hierarchy..."); }
                                    return null; 
                                }
                            }
                            // All steps were found, return corresponding game object
                            if (obj.childCount > 0)
                            {
                                return obj.GetChild(0).gameObject;
                            }
                        }
                    }
                }
                return null;
            }

            public static string GetCreatureName(string nameBlock)
            {
                if (nameBlock==null) { return "(Unknown)"; }
                if (!nameBlock.Contains("<size=0>")) { return nameBlock; }
                return nameBlock.Substring(0, nameBlock.IndexOf("<size=0>")).Trim();
            }

            private static TextMeshProUGUI GetUITextByName(string name)
            {
                TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].name == name)
                    {
                        return texts[i];
                    }
                }
                return null;
            }
        }
    }
}
