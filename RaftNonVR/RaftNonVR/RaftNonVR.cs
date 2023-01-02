using RaftModLoader;
﻿using UnityEngine;
using HMLLibrary;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace RaftNonVR
{
    public class RaftNonVR : Mod
    {
        private static Harmony harmonyInstance;
        private static AssetBundle clipsBundle;
        internal static AnimationClip[] clips;

        public IEnumerator Start()
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes("animclips"));
            yield return request;
            clipsBundle = request.assetBundle;
            clips = clipsBundle.LoadAllAssets<AnimationClip>();

            harmonyInstance = new Harmony("com.DrBibop.RaftNonVR");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Raft_Network net = ComponentManager<Raft_Network>.Value;

            foreach (Network_Player player in net.remoteUsers.Values.ToList())
            {
                if (player.IsLocalPlayer) continue;

                player.gameObject.AddComponent<Network_VRIK>();
            }

            Debug.Log("Mod RaftVR for flatscreen has been loaded!");
        }

        public void OnModUnload()
        {
            Network_VRIK.UnloadAll();

            clipsBundle.Unload(true);

            if (harmonyInstance != null)
                harmonyInstance.UnpatchAll(harmonyInstance.Id);

            Debug.Log("Mod RaftVR for flatscreen has been unloaded!");
        }
    }
}