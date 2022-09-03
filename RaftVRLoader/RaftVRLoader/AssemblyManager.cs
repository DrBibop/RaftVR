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

        if (!Directory.Exists(raftVRDirectory)) Directory.CreateDirectory(raftVRDirectory);

        string[] libraryNames = new string[]
        {
            "RootMotion.dll",
            "AssetsTools.NET.dll",
            "SteamVR.dll",
            "SteamVR_Actions.dll",
            "RaftVRMod.dll"
        };

        modInitializerType = null;
        Assembly modAssembly = null;

        Debug.Log("[RaftVR] Setting up dependencies...");

        for (int i = 0; i < libraryNames.Length; i++)
        {
            string destination = Path.Combine(raftVRDirectory, libraryNames[i]);
            if (CopyDLL(libraryNames[i], destination, i >= 2))
            {
                Assembly resultingAssembly = Assembly.LoadFrom(@destination);

                if (i == 4) modAssembly = resultingAssembly;
            }
            else
            {
                return false;
            }
        }

        modInitializerType = modAssembly.GetType("RaftVR.ModInitializer");
        Debug.Log("[RaftVR] Dependencies ready!");
        return true;
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