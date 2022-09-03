using HarmonyLib;
using RaftVR.Patching;
using RaftVR.Rig;
using RaftVR.Configs;
using RaftVR.UI;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace RaftVR
{
    public class ModInitializer : MonoBehaviour
    {
        private static Harmony harmonyInstance;
        internal static ModInitializer instance;
        private bool loaded = false;

        public void Init(Action<float> refreshHiddenSettingsAction)
        {
            VRConfigs.refreshHiddenSettingsAction = refreshHiddenSettingsAction;
            VRAssetsManager.Init(AssetBundle.LoadFromMemory(Properties.Resources.vrassets));
            Patch();
        }

        public void OnSettingsAPILoaded()
        {
            VRFirstLaunchBox.apiLoaded = true;
            VRFirstLaunchBox.alreadyChoseSettings = VRConfigs.Runtime != VRConfigs.VRRuntime.None;

            if (!loaded) return;

            if (VRFirstLaunchBox.instance)
                VRFirstLaunchBox.instance.StartDialog();
            else if (VRConfigs.Runtime == VRConfigs.VRRuntime.None) 
                OpenDialog();
        }

        private void Patch()
        {
            instance = this;

            VRPatcher.PatchErrorCode result = VRPatcher.PatchVR();

            if (result != VRPatcher.PatchErrorCode.Failed)
            {
                VRPatcher.PatchErrorCode result2 = VRConfigs.RetrieveRuntime();

                if (result2 != VRPatcher.PatchErrorCode.AlreadyPatched)
                    result = result2;
            }

            switch (result)
            {
                case VRPatcher.PatchErrorCode.AlreadyPatched:
                    loaded = true;
                    VRFirstLaunchBox.alreadyPatched = true;
                    if (VRConfigs.Runtime == VRConfigs.VRRuntime.None)
                        OpenDialog();
                    else
                        StartMod();
                    break;
                case VRPatcher.PatchErrorCode.Success:
                    loaded = true;
                    Debug.Log("[RaftVR] VR has been patched in! Restart the game for the patch to take effect.");
                    OpenDialog();
                    break;
                default:
                    Debug.LogError("[RaftVR] Due to an error in the patching process, the mod cannot be loaded.");
                    break;
            }
        }

        internal void StartMod()
        {
            try
            {
                FixUILayer();

                Inputs.VRInput.Init(VRConfigs.Runtime);

                harmonyInstance = new Harmony("com.DrBibop.RaftVR");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                StartCoroutine(EnableVR());

                GameObject rigObject = new GameObject("VR Camera Rig");
                rigObject.AddComponent<VRRig>();
                DontDestroyOnLoad(rigObject);

                UIHelper.InitVRMenu();

                Canvas rmlMenu = RaftModLoader.MainMenu.instance.GetComponent<Canvas>();
                VRRig.instance.AddWorldCanvas(rmlMenu, Quaternion.identity);
                UIHelper.AddPermanentCanvas(rmlMenu);

                UIHelper.InitSettingsCanvas();
                UIHelper.InitLoadingScreen();

                EventSystem.current.gameObject.AddComponent<VRPointerInputModule>();
                Destroy(EventSystem.current.GetComponent<StandaloneInputModule>());

                Debug.Log("[RaftVR] The VR mod has been loaded!");
            }
            catch (Exception e)
            {
                Debug.LogError("[RaftVR] An error occured while activating VR");
                Debug.LogException(e);
            }
        }

        private void FixUILayer()
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            int[] layersToIgnore = new int[]
            {
                LayerMask.NameToLayer("Debris"),
                LayerMask.NameToLayer("Particles"),
                LayerMask.NameToLayer("LocalPlayer"),
                LayerMask.NameToLayer("RemotePlayer"),
                LayerMask.NameToLayer("MapIgnore"),
                LayerMask.NameToLayer("Item"),
                LayerMask.NameToLayer("Block"),
                uiLayer
            };

            for (int i = 0; i < layersToIgnore.Length; i++)
            {
                Physics.IgnoreLayerCollision(uiLayer, layersToIgnore[i], true);
            }
        }

        private void OpenDialog()
        {
            StartMenuScreen menuScreen = FindObjectOfType<StartMenuScreen>();

            if (menuScreen)
            {
                MenuBox[] boxes = (MenuBox[])Traverse.Create(menuScreen).Field("menuBoxes").GetValue();

                if (boxes != null)
                {
                    ExitGameBox exitBox = boxes.FirstOrDefault(box => box is ExitGameBox) as ExitGameBox;

                    if (exitBox)
                    {
                        GameObject boxInstance = Instantiate(exitBox.gameObject, exitBox.transform.parent);
                        boxInstance.name = "VR Box";

                        Destroy(boxInstance.GetComponent<ExitGameBox>());
                        boxInstance.AddComponent<VRFirstLaunchBox>();

                        boxInstance.SetActive(true);
                    }
                }
            }
        }

        private IEnumerator EnableVR()
        {
            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR)
            {
                Valve.VR.SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
                Valve.VR.SteamVR_Settings.instance.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding;
                Valve.VR.SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;

                Valve.VR.SteamVR_Actions.PreInitialize();
                Valve.VR.SteamVR.Initialize(true);
                Valve.VR.SteamVR_Actions.ui.Activate();
            }
            else if (VRConfigs.Runtime == VRConfigs.VRRuntime.Oculus)
            {
                XRSettings.LoadDeviceByName("Oculus");
                yield return null;
                if (XRSettings.loadedDeviceName != "Oculus") yield break;

                XRSettings.enabled = true;

                // Ignore the deprecated warning. The alternative requires the new XR system which isn't used here.
                XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
            }
        }

        //If I REALLY want to make this mod unloadable, there's a lot more to do than this.
        //For now, at least, the mod will be set as permanent.
        public void Unload()
        {
            if (XRSettings.enabled)
            {
                XRSettings.LoadDeviceByName("None");
                XRSettings.enabled = false;
            }

            if (harmonyInstance != null)
                harmonyInstance.UnpatchAll(harmonyInstance.Id);

            VRAssetsManager.Unload();

            Debug.Log("[RaftVR] The VR mod has been unloaded.");
        }
    }
}
