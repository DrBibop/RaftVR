using UnityEngine;
using System;

public class RaftVRLoader : Mod
{
    public static RaftVRLoader instance { get; private set; }
    public static SettingsAPI ExtraSettingsAPI_Settings;

    private Component initializer;
    private Type initializerType;

    public void Start()
    {
        instance = this;

        if (AssemblyManager.CopyDependencies(out initializerType))
        {
            initializer = gameObject.AddComponent(initializerType);

            ExtraSettingsAPI_Settings = new SettingsAPI(initializer, initializerType, initializerType.Assembly.GetType("RaftVR.Configs.VRConfigs"));

            initializerType.GetMethod("Init").Invoke(initializer, new object[] { new Action<float>(ExtraSettingsAPI_Settings.RefreshHiddenSettings) });
        }
        else
        {
            Debug.LogError("[RaftVR] Failed to copy dependency assemblies. RaftVR will not load.");
            return;
        }
    }

    public byte[] GetEmbeddedFileCombinedPath(params string[] paths)
    {
        string combinedString = paths[0];
        for (int i = 1; i < paths.Length; i++)
        {
            combinedString += "/" + paths[i];
        }
        return GetEmbeddedFileBytes(combinedString);
    }

    public void OnModUnload()
    {
        initializerType.GetMethod("Unload").Invoke(initializer, null);
    }
}