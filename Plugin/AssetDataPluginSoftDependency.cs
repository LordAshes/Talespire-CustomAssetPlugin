using BepInEx;
using System;
using System.Reflection;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsPlugin : BaseUnityPlugin
    {

        public static class AssetDataPluginSoftDependency
        {
            public static MethodInfo SendInfo = null;
            public static MethodInfo SetInfo = null;
            public static MethodInfo ClearInfo = null;

            public static void Initialize()
            {
                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Plugin: Checking For AssetDataPlugin (" + BepInEx.Paths.PluginPath + "\\LordAshes-AssetDataPlugin\\AssetDataPlugin.dll)..."); }
                if (System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\LordAshes-AssetDataPlugin\\AssetDataPlugin.dll"))
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Asset Plugin: AssetDataPlugin Is Present. Remote Mode Enabled."); }
                    Assembly aby = Assembly.LoadFile(BepInEx.Paths.PluginPath + "/LordAshes-AssetDataPlugin/AssetDataPlugin.dll");
                    Type type = null;
                    foreach (Type foundType in aby.GetTypes()) { if (foundType.Name == "AssetDataPlugin") { type = foundType; break; } }
                    if (type != null)
                    {
                        try
                        {
                            if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Plugin: AssetDataPlugin Seems To Be Loaded. Subscribing To " + CustomAssetsPlugin.Guid + ".*"); }
                            type.GetMethod("SubscribeViaReflection").Invoke(null, new object[] { CustomAssetsPlugin.Guid + ".*", "LordAshes.CustomAssetsPlugin, CustomAssetsPlugin", "RemoteRequestRouter" });
                        }
                        catch (Exception x)
                        {
                            Debug.LogWarning("Custom Asset Plugin: Unable to Subscribe To " + CustomAssetsPlugin.Guid + ".*");
                            Debug.LogWarning(x);
                        }
                        foreach (MethodInfo method in type.GetRuntimeMethods())
                        {
                            if (method.Name == "ClearInfo") 
                            { 
                                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Plugin: Getting ClearInfo Reference"); } 
                                ClearInfo = method; 
                            }
                            if (method.Name == "SendInfo" && method.GetParameters()[1].ParameterType.ToString() == "System.String") 
                            {
                                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Plugin: Getting SendInfo Reference"); }
                                SendInfo = method; 
                            }
                            if (method.Name == "SetInfo" && method.GetParameters()[2].ParameterType.ToString() == "System.String")
                            {
                                if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Asset Plugin: Getting SetInfo Reference"); }
                                SetInfo = method; 
                            }
                            if (ClearInfo != null && SendInfo != null && SetInfo != null) { break; }
                        }
                    }
                }
                else
                {
                    if (CustomAssetsPlugin.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Asset Plugin: AssetDataPlugin Is Not Present. Local Mode Enabled."); }
                }
            }
        }
    }
}