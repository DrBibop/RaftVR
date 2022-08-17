using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace RaftVR.Patching
{
    // Based on AssemblyManager by SunnyBat, the creator of Raftipelago.
    static class AssemblyManager
    {
        public static bool CopyDependencies()
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string raftVRDirectory = Path.Combine(appDataDirectory, "RaftVR");
            string assemblyName = "AssetsTools.NET.dll";

            if (!Directory.Exists(raftVRDirectory)) Directory.CreateDirectory(raftVRDirectory);

            string to = Path.Combine(raftVRDirectory, assemblyName);

            Debug.Log("[RaftVR] Setting up dependencies...");
            if (CopyDLL(assemblyName, to))
            {
                Assembly assembly = Assembly.LoadFrom(to);
                Wrappers.AssetsToolsWrapper.Init(assembly);
                Debug.Log("[RaftVR] Dependencies ready!");
                return true;
            }
            return false;
        }

        private static bool CopyDLL(string assemblyName, string to)
        {
            if (File.Exists(to)) return true;
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
}
