using System;
using System.IO;
using System.Reflection;
using UnityEngine;

// Based on AssemblyManager by SunnyBat, the creator of Raftipelago.
static class AssemblyManager
{
    public static bool CopyDependencies(out Type modInitializerType)
    {
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string raftVRDirectory = Path.Combine(appDataDirectory, "RaftVR");
        string assetsToolsAssembly = "AssetsTools.NET.dll";
        string modAssembly = "RaftVRMod.dll";

        if (!Directory.Exists(raftVRDirectory)) Directory.CreateDirectory(raftVRDirectory);

        string assetsToolsTo = Path.Combine(raftVRDirectory, assetsToolsAssembly);
        string modTo = Path.Combine(raftVRDirectory, modAssembly);

        Debug.Log("[RaftVR] Setting up dependencies...");
        if (CopyDLL(assetsToolsAssembly, assetsToolsTo) && CopyDLL(modAssembly, modTo, true))
        {
            Assembly.LoadFrom(assetsToolsTo);
            Assembly mod = Assembly.LoadFrom(modTo);
            modInitializerType = mod.GetType("RaftVR.ModInitializer");
            Debug.Log("[RaftVR] Dependencies ready!");
            return true;
        }
        modInitializerType = null;
        return false;
    }

    private static bool CopyDLL(string assemblyName, string to, bool replace = false)
    {
        if (File.Exists(to) && !replace) return true;
        try
        {
            byte[] assemblyBytes = RaftVRLoader.instance.GetEmbeddedFileCombinedPath("Assemblies", assemblyName + ".copyonly");
            if (assemblyBytes != null && assemblyBytes.Length > 0)
            {
                Debug.Log("[RaftVR] Copying " + assemblyName);
                File.WriteAllBytes(to, assemblyBytes);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}