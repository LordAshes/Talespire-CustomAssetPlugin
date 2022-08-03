using BepInEx;
using Bounce.Unmanaged;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace PluginMasters
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {
        public static class Setup
        {
            public static Data.Index index = null;

            public static Data.LoaderDataType noBase = new Data.LoaderDataType() { BundleId = "", AssetName = "" };
            public static Data.LoaderDataType defaultBase = new Data.LoaderDataType() { BundleId = "char_base01_1462710208", AssetName = "clothBase" };
            public static string ExtraAssetsRegistrationPlugin_Guid = "org.lordashes.plugins.extraassetsregistration";
            public static Texture2D defaultPortrait = null;

            public static List<string> violations = new List<string>();
            public static Dictionary<string,string> effects = new Dictionary<string, string>();

            private static int atlasIndex = -1;

            /// <summary>
            /// Method to find custom assets and registers them with Custom Assets Library (CAL)
            /// </summary>
            public static void RegisterAssets()
            {
                defaultPortrait = LordAshes.FileAccessPlugin.Image.LoadTexture("DefaultPortrait.png");
                List<string> files = new List<string>();
                List<string> folders = new List<string>();
                GetPluginAssetBundleFiles(BepInEx.Paths.PluginPath, ref files);
                GetPluginPacks(BepInEx.Paths.PluginPath, ref folders);
                for(int i=0; i<files.Count; i++)
                {
                    // Remove files immediately in the plugin folder (e.g. index, license, etc)
                    string fileShort = files[i].Substring(BepInEx.Paths.PluginPath.Length + 1);
                    fileShort = fileShort.Substring(fileShort.IndexOf("\\")+1);
                    if (!fileShort.Contains("\\")) 
                    {
                        files.RemoveAt(i);
                        i = i - 1;
                    }
                }
                foreach (string folder in folders)
                {
                    index = new Data.Index();
                    string singleFolder = folder.Trim();
                    singleFolder = singleFolder.Substring(0, singleFolder.Length - 1);
                    singleFolder = singleFolder.Substring(singleFolder.LastIndexOf("/") + 1);
                    // index.assetPackId = new NGuid(Utility.GuidFromString(singleFolder.Substring(0,singleFolder.Length-1))).ToString();
                    index.assetPackId = new NGuid(Utility.GuidFromString(singleFolder)).ToString();
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Building AssetPackId "+index.assetPackId+" (Based On '" + singleFolder + "')"); }
                    if (!System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\" + folder + "index") || CustomAssetsPlugin._self.operationMode==OperationMode.rebuildIndexAlways)
                    {
                        //
                        // New Assets Or Regenerate On
                        //
                        if (System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\" + folder + "index")) { System.IO.File.Delete(BepInEx.Paths.PluginPath + "\\" + folder + "index"); }
                        // If index.json file exists, use it instead allowing localized info.txt tweaking
                        if (!System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\" + folder + "index.json") || CustomAssetsPlugin._self.operationMode == OperationMode.rebuildIndexAlways)
                        {
                            if (System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\" + folder + "index.json")) { System.IO.File.Delete(BepInEx.Paths.PluginPath + "\\" + folder + "index.json"); }
                            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Scanning Pack '" + singleFolder + "' At '" + BepInEx.Paths.PluginPath + "\\" + folder + "'"); }
                            foreach (string assetFile in files.Where(f => f.StartsWith(BepInEx.Paths.PluginPath + "\\" + folder) && f.Contains("CustomData") && System.IO.Path.GetExtension(f).Replace(".", "").Trim() == ""))
                            {
                                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Analyzing Pack '" + singleFolder + "' File '" + assetFile + "'."); }
                                // Potential Asset Bundle
                                AssetBundle ab = null;
                                Data.AssetInfo info = null;
                                Texture2D portrait = null;
                                try
                                {
                                    // Check Asset Bundle
                                    ab = LordAshes.FileAccessPlugin.AssetBundle.Load(assetFile);
                                }
                                catch (Exception x)
                                {
                                    Debug.LogWarning("Custom Asset Library Plugin: File '" + assetFile + "' Does Not Seem To Be A Valid Asset Bundle.");
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: File '" + assetFile + "' Generated " + x); }
                                    continue;
                                }
                                // Check Info File
                                string json = "";
                                try
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Asset Library Plugin: Reading 'info.txt' File"); }
                                    json = ab.LoadAsset<TextAsset>("Info.txt").text;
                                }
                                catch(Exception)
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning("Custom Asset Library Plugin: AssetBundle '" + assetFile + "' Does No Seem To Have An Info.Txt File. Using Default."); }
                                    info = new Data.AssetInfo()
                                    {
                                        name = System.IO.Path.GetFileNameWithoutExtension(assetFile),
                                        kind = "Creature",
                                        category = "Creature",
                                        groupName = "Custom Content",
                                    };
                                }
                                try
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Asset Library Plugin: Parsing 'info.txt' File"); }
                                    info = LordAshes.SmartJsonConvert.DeserializeObject<Data.AssetInfo>(json);
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra)
                                    {
                                        Debug.Log("Custom Asset Library Plugin: Locale Conversion Results: "+ LordAshes.SmartJsonConvert.GetLastConversionInfo());
                                    }
                                }
                                catch (Exception)
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: AssetBundle '" + assetFile + "' Has Issue With Its Info.Txt File. Using Default."); }
                                    info = new Data.AssetInfo()
                                    {
                                        name = System.IO.Path.GetFileNameWithoutExtension(assetFile),
                                        kind = "Creature",
                                        category = "Creature",
                                        groupName = "Custom Content",
                                    };
                                }
                                try
                                {
                                    // Check Portrait
                                    portrait = ab.LoadAsset<Texture2D>("Portrait.png");
                                }
                                catch (Exception)
                                {
                                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: AssetBundle '" + assetFile + "' Does Not Have A Portrait.Png File. Using Default."); }
                                    portrait = LordAshes.FileAccessPlugin.Image.LoadTexture("DefaultPortrait.png");
                                }
                                try
                                {
                                    // Check Prefab
                                    string prefabName = (info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(assetFile).ToLower();
                                    string[] items = ab.GetAllAssetNames();
                                    string fallback = "";
                                    bool found = false;
                                    foreach(string item in items)
                                    {
                                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Asset Library Plugin: Asset Bundle '" + System.IO.Path.GetFileNameWithoutExtension(assetFile) + "' Contains '"+item+"'"); }
                                        if (item.ToLower().Contains(prefabName+".prefab")) { found = true; break; }
                                        if (item.ToLower().EndsWith(".prefab")) { fallback = item; }
                                    }
                                    if(!found)
                                    {
                                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning("Custom Asset Library Plugin: Violation: AssetBundle '" + assetFile + "' Violates The Prime Directive By Not Having A '" + System.IO.Path.GetFileNameWithoutExtension(assetFile) + "' Prefab."); }
                                        violations.Add("Prefab '" + prefabName + "' Missing In Asset Bundle '" + assetFile + "'");
                                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning("Custom Asset Library Plugin: Using Fallback Prefab '" + fallback + "'"); }
                                        info.prefab = fallback;
                                    }
                                }
                                catch(Exception)
                                {
                                    Debug.LogWarning("Custom Asset Library Plugin: Unable To Read Asset Names From AssetBundle '" + assetFile + "'");
                                }
                                // Unload asset bundle freeing to for TS to use
                                if (ab != null) { ab.Unload(false); }
                                if (info != null)
                                {
                                    info.id = Utility.GuidFromString(System.IO.Path.GetFileNameWithoutExtension(assetFile)).ToString(); // info.id = Utility.GuidFromString(assetFile).ToString();
                                    if (!folder.Contains("TaleSpire_CustomData"))
                                    {
                                        info.location = assetFile.Substring(BepInEx.Paths.PluginPath.Length + 1);
                                        info.location = info.location.Substring(info.location.IndexOf("/") + 1);
                                    }
                                    else
                                    {
                                        info.location = assetFile.Substring(assetFile.IndexOf("TaleSpire_CustomData") + "TaleSpire_CustomData".Length + 1);
                                    }
                                    if (info.kind == "") { info.kind = "Creature"; }
                                    if (info.category == "") { info.category = "Creature"; }
                                    Setup.RegisterAsset(info, assetFile.Replace("/CustomData/", "/Assets/").Replace("\\CustomData\\", "\\Assets\\"), singleFolder, index.assetPackId);
                                    Setup.CreatePortrait(info, singleFolder, portrait);
                                }
                            }
                            if ((index.Creatures.Count + index.Music.Count + index.Props.Count + index.Tiles.Count) > 0)
                            {
                                Setup.CreateIndexFile(singleFolder, index.assetPackId);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Custom Asset Library Plugin: Using Cached "+singleFolder+"\\Index.json File For Pack "+index.assetPackId+".");
                            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.ultra)
                            {
                                Debug.LogWarning("Custom Asset Library Plugin: Folder " + BepInEx.Paths.PluginPath + "\\" + folder+" Content:");
                                foreach (string item in System.IO.Directory.EnumerateFiles(BepInEx.Paths.PluginPath + "\\" + folder))
                                {
                                    Debug.LogWarning("Custom Asset Library Plugin: -[FILE]-> " + item);
                                }
                                foreach (string item in System.IO.Directory.EnumerateDirectories(BepInEx.Paths.PluginPath + "\\" + folder))
                                {
                                    Debug.LogWarning("Custom Asset Library Plugin: -[DIR]-> " + item);
                                }
                            }
                            Setup.CreateIndexFile(singleFolder, index.assetPackId, false);
                        }
                    }
                    else
                    {
                        //
                        // Old Assets And Regenerate Off
                        //
                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Content From Pack '" + folder + "' Is Already Registered."); }
                    }
                }
            }

            /// <summary>
            /// Method used to place asset entries into the data index
            /// </summary>
            /// <param name="info">AssetInfo file describing the asset to be registered</param>
            /// <param name="singleFolder">String of the asset location (used for log purpose only)</param>
            /// <param name="assetPackId">String of the asset pack id</param>
            public static void RegisterAsset(Data.AssetInfo info, string assetFile, string singleFolder, string assetPackId)
            {
                string stage = "RegisterAsset";
                try
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Asset Library Plugin: Registering Asset '" + info.name + "' (" + info.location + ") In AssetPack " + singleFolder + " (" + assetPackId + ")"); }
                    stage = "Get Relative Path";
                    string relativePath = info.location.Substring(info.location.IndexOf("\\") + 1);
                    relativePath = relativePath.Substring(relativePath.IndexOf("\\") + 1);
                    stage = "Get Rotation";
                    string[] rot = (info.mesh.rotationOffset == "") ? new string[] { "0", "0", "0" } : info.mesh.rotationOffset.Split(',');

                    stage = "Get Process Type";
                    switch (info.kind.ToUpper())
                    {
                        case "AUDIO":
                        case "AURA":
                        case "CREATURE":
                        case "EFFECT":
                        case "FILTER":
                        case "TRANSFORM":
                        case "ENCOUNTER":
                            atlasIndex++;
                            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Atlas Index = " + atlasIndex); }
                            stage = "Generate Asset";
                            Data.CreatureType assetMain = new Data.CreatureType();
                            stage = "Generate Asset Id";
                            assetMain.Id = Utility.GuidFromString(info.id).ToString();
                            stage = "Generate Asset Name";
                            assetMain.Name = info.name;
                            stage = "Generate Asset Group";
                            assetMain.GroupTag = info.groupName;
                            stage = "Generate Asset Tags";
                            assetMain.Tags = TrimList((info.tags + ",Category:" + info.category + ",Kind:" + info.kind + ",Prefab:" + ((info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)) + ",AssetBundle:" + assetFile).Split(',').ToList<string>());
                            stage = "Generate Asset Deprecated";
                            assetMain.IsDeprecated = info.isDeprecated;
                            stage = "Generate Asset Base Loader";
                            Data.AssetType baseLoader = new Data.AssetType();
                            stage = "Generate Asset Base Loader Data";
                            baseLoader.LoaderData = ((info.assetBase.ToUpper() == "NONE") ? defaultBase : (info.assetBase.ToUpper() == "DEFAULT") ? defaultBase : new Data.LoaderDataType() { AssetName = System.IO.Path.GetFileNameWithoutExtension(info.assetBase), BundleId = System.IO.Path.GetFileNameWithoutExtension(info.assetBase) });
                            stage = "Generate Asset Base Loader Scale";
                            baseLoader.Scale = ((info.assetBase.ToUpper() == "NONE") ? "0,0,0" : info.size + "," + info.size + "," + info.size);
                            stage = "Generate Asset Base Loader Add";
                            assetMain.BaseAsset = baseLoader;
                            stage = "Generate Asset Loader";
                            Data.AssetType miniLoader = new Data.AssetType();
                            stage = "Generate Asset Loader Data";
                            miniLoader.LoaderData = new Data.LoaderDataType()
                            {
                                BundleId = relativePath,
                                AssetName = (info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)
                            };
                            stage = "Generate Asset Loader Scale";
                            miniLoader.Scale = info.mesh.size;
                            stage = "Generate Asset Loader Position";
                            miniLoader.Position = info.mesh.positionOffset;
                            stage = "Generate Asset Loader Rotation";
                            miniLoader.Rotation = rot[0] + "," + (float.Parse(rot[1]) + 180).ToString() + "," + rot[2];
                            stage = "Generate Asset Loader Add";
                            assetMain.MiniAsset = miniLoader;
                            stage = "Generate Asset Default Scale";
                            assetMain.DefaultScale = info.size;
                            stage = "Generate Asset Icon";
                            assetMain.Icon = new Data.IconType()
                            {
                                AtlasIndex = atlasIndex,
                                Region = new Data.RegionType()
                                {
                                    x = 0,
                                    y = 0,
                                    width = 1,
                                    height = 1
                                }
                            };
                            stage = "Generate Add Asset";
                            index.Creatures.Add(assetMain);
                            stage = "Generate Add To Auras/Effects List";
                            if ("|AURA|EFFECT|".Contains(info.kind.ToUpper()))
                            {
                                effects.Add(assetMain.Id, assetMain.MiniAsset.LoaderData.AssetName + "@" + assetFile);
                            }
                            break;
                        case "MUSIC":
                            index.Music.Add(new Data.MusicType()
                            {
                                Id = Utility.GuidFromString(info.location).ToString(),
                                Name = info.name,
                                GroupTag = info.groupName,
                                Tags = TrimList((info.tags + ",Category:" + info.category + ",Kind:" + info.kind).Split(',').ToList<string>()),
                                IsDeprecated = info.isDeprecated,
                                Assets = new Data.LoaderDataType()
                                {
                                    BundleId = relativePath,
                                    AssetName = (info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)
                                }
                            });
                            break;
                        case "PROP":
                            atlasIndex++;
                            index.Props.Add(new Data.TileAndPropsType()
                            {
                                Id = Utility.GuidFromString(info.location).ToString(),
                                Name = info.name,
                                GroupTag = info.groupName,
                                Tags = TrimList((info.tags + ",Category:" + info.category + ",Kind:" + info.kind + ",Prefab:" + ((info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)) + ",AssetBundle:" + assetFile).Split(',').ToList<string>()),
                                IsDeprecated = info.isDeprecated,
                                Assets = new List<Data.AssetType>()
                            {
                                new Data.AssetType()
                                {
                                    LoaderData = new Data.LoaderDataType()
                                    {
                                        BundleId = relativePath,
                                        AssetName = (info.prefab!="") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)
                                    },
                                    Scale = info.mesh.size,
                                    Position = info.mesh.positionOffset,
                                    Rotation = rot[0]+","+(float.Parse(rot[1])+180).ToString()+","+rot[2]
                                }
                            },
                                IsInteractable = false,
                                ColliderBoundsBound = new Data.BoundsType()
                                {
                                    m_Center = info.collider.center,
                                    m_Extent = info.collider.extent,
                                },
                                Icon = new Data.IconType()
                                {
                                    AtlasIndex = atlasIndex,
                                    Region = new Data.RegionType()
                                    {
                                        x = 0,
                                        y = 0,
                                        width = 1,
                                        height = 1
                                    }
                                }
                            });
                            break;
                        case "TILE":
                        case "SLAB":
                            atlasIndex++;
                            index.Tiles.Add(new Data.TileAndPropsType()
                            {
                                Id = Utility.GuidFromString(info.location).ToString(),
                                Name = info.name,
                                GroupTag = info.groupName,
                                Tags = TrimList((info.tags + ",Category:" + info.category + ",Kind:" + info.kind + ",Prefab:" + ((info.prefab != "") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)) + ",AssetBundle:" + assetFile).Split(',').ToList<string>()),
                                IsDeprecated = info.isDeprecated,
                                Assets = new List<Data.AssetType>()
                            {
                                new Data.AssetType()
                                {
                                    LoaderData = new Data.LoaderDataType()
                                    {
                                        BundleId = relativePath,
                                        AssetName = (info.prefab!="") ? info.prefab : System.IO.Path.GetFileNameWithoutExtension(info.location)
                                    },
                                    Scale = info.mesh.size,
                                    Position = info.mesh.positionOffset,
                                    Rotation = rot[0]+","+(float.Parse(rot[1])+180).ToString()+","+rot[2]
                                }
                            },
                                IsInteractable = false,
                                ColliderBoundsBound = new Data.BoundsType()
                                {
                                    m_Center = info.collider.center,
                                    m_Extent = info.collider.extent,
                                },
                                Icon = new Data.IconType()
                                {
                                    AtlasIndex = atlasIndex,
                                    Region = new Data.RegionType()
                                    {
                                        x = 0,
                                        y = 0,
                                        width = 1,
                                        height = 1
                                    }
                                }
                            });
                            break;
                    }

                    if (info.kind.ToUpper() != "MUSIC")
                    {
                        index.IconsAtlas.Add(new Data.IconsAtlasesType()
                        {
                            Path = "Portraits/" + info.id
                        });
                    }
                }
                catch(Exception x)
                {
                    Debug.LogError("Custom Asset Library Plugin: " + x);
                    Debug.LogError("Custom Asset Library Plugin: While Process Stage " + stage +" Using " + JsonConvert.SerializeObject(info));
                }
            }

            /// <summary>
            /// Method used to generate an asset's portrait file
            /// </summary>
            /// <param name="info">AssetInfo file describing the asset to be registered</param>
            /// <param name="portrait">Texture2D to be converted to a portrait file</param>
            public static void CreatePortrait(Data.AssetInfo info, string singleFolder, Texture2D portrait)
            {
                try
                {
                    if (!System.IO.Directory.Exists(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits")) 
                    {
                        if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Creating Portrait Folder (" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits)"); }
                        System.IO.Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits"); 
                    }
                    System.IO.File.WriteAllBytes(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\\" + info.id.ToString(), portrait.EncodeToPNG());
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Created Portrait File For Asset '" + info.name + "' -> " + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\\" + info.id.ToString()); }
                }
                catch (Exception)
                {
                    System.IO.File.WriteAllBytes(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\\" + info.id.ToString(), defaultPortrait.EncodeToPNG());
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Created Default Portrait File For Asset '" + info.name + "' -> " + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\\" + info.id.ToString()); }
                }
            }

            /// <summary>
            /// Method used to create an entry in the Asset Library for the asset
            /// </summary>
            /// <param name="singleFolder">String name of the pack root folder</param>
            public static void CreateIndexFile(string singleFolder, string assetPackId, bool writeJsonIndexFile = true)
            {
                try
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Renaming CustomData Folder To Assets"); }
                    System.IO.Directory.Move(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\CustomData", BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Assets");
                    System.IO.File.WriteAllText(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Unregister.bat",
                    "@echo off\r\n" +
                    "ren \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Assets\" CustomData\"\r\n" +
                    "del /Q \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\\*.*\"\r\n" +
                    "rmdir \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Portraits\"\r\n" +
                    "del /Q \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\index\"\r\n" +
                    "del /Q \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\index.json\"\r\n" +
                    "del /Q \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\RegisterForVanilla.bat\"\r\n" + 
                    "del /Q \"" + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Unregister.bat\"\r\n");
                    System.IO.File.WriteAllText(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\RegisterForVanilla.bat",
                    "@echo off\r\n" +
                    "@if \"%1\"==\"\" echo Syntax: RegisterForVanilla.bat PathToTaleweaverFolder\r\n" +
                    "@if \"%1\"==\"\" goto Done\r\n" +
                    "cd>RegisterForVanilla.$$$\r\n" +
                    "for /F \"tokens=*\" %%A in (RegisterForVanilla.$$$) do mklink \"%1\\" + assetPackId + "\" \"%%A\"\r\n" +
                    "del RegisterForVanilla.$$$\r\n" + 
                    "\r\n:Done\r\n"); 
                }
                catch (Exception x) 
                {
                    Debug.Log("Custom Asset Library Plugin: Failure Renaming CustomData Folder To Assets: "+x);
                }
                if (writeJsonIndexFile)
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Library Plugin: Writing " + BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Index.Json File"); }
                    System.IO.File.WriteAllText(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\Index.json", JsonConvert.SerializeObject(index, Formatting.Indented));
                }
                CustomAssetsLibrary.CustomAssetLib.Generate(BepInEx.Paths.PluginPath + "\\" + singleFolder);
                if (CustomAssetsPlugin.DeleteJSONIndexFiles() == true) { System.IO.File.Delete(BepInEx.Paths.PluginPath + "\\" + singleFolder + "\\index.json"); }
                atlasIndex = -1;
            }

            public static void GetPluginAssetBundleFiles(string path, ref List<string> files)
            {
                files.AddRange(System.IO.Directory.EnumerateFiles(path,"*."));
                foreach(string folder in System.IO.Directory.EnumerateDirectories(path))
                {
                    GetPluginAssetBundleFiles(folder, ref files);
                }
            }

            public static void GetPluginPacks(string path, ref List<string> folders)
            {
                string[] find = System.IO.Directory.EnumerateDirectories(path).ToArray();
                for(int i=0; i<find.Length; i++)
                {
                    find[i] = find[i].Replace("/", "\\");
                    find[i] = find[i].Substring(path.Length+1)+"\\";
                }
                folders.AddRange(find);
            }

            public static List<String> TrimList(List<String> list)
            {
                List<String> newList = new List<string>();
                foreach(string entry in list)
                {
                    newList.Add(entry.Trim());
                }
                return newList;
            }
        }
    }
}
