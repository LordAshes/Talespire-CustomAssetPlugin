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
        public static void RemoteRequestRouter(string action, string source, string key, string previous, string value)
        {
            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Plugin: Remote " + action + " Request For " + key + " (For " + source + ") " + previous + "->" + value); }
            if (action != "invalid")
            {
                key = key.Substring(CustomAssetsPlugin.Guid.Length + 1);
                key = key.ToLower() + "." + action.ToLower();
                value = value.Substring(0, value.LastIndexOf("@"));
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Plugin: Processing " +key+" -> "+value); }
                switch (key)
                {
                    case "animate.add":
                    case "animate.modify":
                        RequestHandler.Animate(new CreatureGuid(source), int.Parse(value));
                        break;
                    case "animate.remove":
                        RequestHandler.Animate(new CreatureGuid(source), -1);
                        break;
                    case "audio.add":
                    case "audio.modify":
                        RequestHandler.Audio(new CreatureGuid(source), 1);
                        break;
                    case "audio.remove":
                        RequestHandler.Audio(new CreatureGuid(source), -1);
                        break;
                    case "stop.add":
                    case "stop.modify":
                    case "stop.remove":
                        RequestHandler.Animate(new CreatureGuid(source), -1);
                        RequestHandler.Audio(new CreatureGuid(source), -1);
                        break;
                    case "analyze.add":
                    case "analyze.modify":
                    case "anazyle.remove":
                        RequestHandler.Analyze(new CreatureGuid(source));
                        break;
                }
            }
        }

        public static class RequestHandler
        {
            public static void Animate(CreatureGuid cid, int selection)
            {
                try
                {
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        Animation anim = asset.GetComponentInChildren<Animation>();
                        if (anim != null)
                        {
                            if (selection == -1)
                            {
                                Debug.Log("Custom Asset Plugin: Stopping Animation On " + asset.name);
                                anim.Stop();
                            }
                            else if (selection == 0)
                            {
                                SystemMessage.AskForTextInput("Animation", "Animation Name:", "OK", (animName) =>
                                {
                                    Debug.Log("Custom Asset Plugin: Starting Animation '" + animName + "' On " + asset.name);
                                    anim.Play(animName);
                                }, null, "Cancel", null, "");
                            }
                            else
                            {
                                Debug.Log("Custom Asset Plugin: Starting Animation '" + "Anim" + selection.ToString("d2") + "' On " + asset.name);
                                anim.Play("Anim" + selection.ToString("d2"));
                            }
                        }
                        else
                        {
                            Debug.Log("Custom Asset Plugin: Unable To Find Animation Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Asset Plugin: No Selected Asset To Animate");
                    }
                }
                catch(Exception x)
                {
                    Debug.Log("Custom Asset Plugin: Error Processing Animate "+selection+" On "+cid);
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void Audio(CreatureGuid cid, int selection)
            {
                try
                {
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        AudioSource audio = asset.GetComponentInChildren<AudioSource>();
                        if (audio != null)
                        {
                            if (selection == -1)
                            {
                                Debug.Log("Custom Asset Plugin: Stopping Audio On " + asset.name);
                                audio.Stop();
                            }
                            else
                            {
                                Debug.Log("Custom Asset Plugin: Starting Audio On " + asset.name);
                                audio.Play();
                            }
                        }
                        else
                        {
                            Debug.Log("Custom Asset Plugin: Unable To Find AudioSource Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Asset Plugin: No Selected Asset For Audio Function");
                    }
                }
                catch(Exception x)
                {
                    Debug.Log("Custom Asset Plugin: Error Processing Audio " + selection + " On " + cid);
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void Analyze(CreatureGuid cid)
            {
                try
                {
                    GameObject go = Utility.GetAssetLoader(cid);
                    Debug.Log("Custom Asset Plugin: Analyzing Mini " + go.name);
                    AnalyzeGameObject(go);
                }
                catch (Exception x)
                {
                    Debug.Log("Custom Asset Plugin: Error Processing Analyze On " + cid);
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }
        }
    }
}
