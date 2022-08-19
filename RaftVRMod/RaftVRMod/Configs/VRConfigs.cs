using RaftVR.Patching;
using RaftVR.Rig;
using RootMotion.FinalIK;
using System;
using System.IO;
using UnityEngine;

namespace RaftVR.Configs
{
    public static class VRConfigs
    {
        private static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string RaftVRDataPath => Path.Combine(AppDataPath, "RaftVR");
        private static string RuntimeConfigPath => Path.Combine(RaftVRDataPath, "vrruntime.txt");

        private static VRRuntime _platform = VRRuntime.None;
        private static bool _snapTurn = false;
        private static float _turnSpeed = 90;
        private static float _turnAngle = 45;
        private static bool _seatedMode = false;
        private static bool _isLeftHanded = false;
        private static bool _interactionRay = true;
        private static PlayspaceCenterDisplay _playspaceCenterDisplay = PlayspaceCenterDisplay.WhenFar;
        private static float _armScale = 1;

        internal static PlayerAnimator localPlayerAnimator;
        internal static Action<float> refreshHiddenSettings;
        internal static GameObject calibrateCanvas;

        public static VRRuntime Runtime
        {
            get => _platform;
            internal set
            {
                string configText;

                switch(value)
                {
                    case VRRuntime.SteamVR:
                        configText = "steamvr";
                        break;
                    case VRRuntime.Oculus:
                        configText = "oculus";
                        break;
                    default:
                        configText = "none";
                        break;
                }

                try
                {
                    File.WriteAllText(RuntimeConfigPath, configText);
                    _platform = value;
                    Debug.Log("[RaftVR] Runtime has been set to: " + configText);
                }
                catch(Exception e)
                {
                    Debug.LogError("[RaftVR] Failed to write platform choice to file:");
                    Debug.LogException(e);
                }
            }
        }

        public static bool SnapTurn
        {
            get => _snapTurn;
            internal set { _snapTurn = value; }
        }

        public static bool IsLeftHanded
        {
            get => _isLeftHanded;
            internal set { _isLeftHanded = value; }
        }

        public static float SmoothTurnSpeed
        {
            get => _turnSpeed;
            internal set { _turnSpeed = value; }
        }

        public static float SnapTurnAngle
        {
            get => _turnAngle;
            internal set { _turnAngle = value; }
        }

        public static PlayspaceCenterDisplay ShowPlayspaceCenter
        {
            get => _playspaceCenterDisplay;
            internal set { _playspaceCenterDisplay = value; }
        }

        public static float ArmScale
        {
            get => _armScale;
            internal set 
            { 
                _armScale = value;
                if (localPlayerAnimator != null)
                {
                    // Have to call GetComponent so I don't have a variable of a type from an external library.
                    // Otherwise, it freaks out on first launch when the RootMotion assembly isn't loaded.
                    IKSolverVR solver = localPlayerAnimator.GetComponent<VRIK>().solver;
                    solver.leftArm.armLengthMlp = value;
                    solver.rightArm.armLengthMlp = value;
                }

                if (value == 0)
                    ShowCalibrateCanvas();
            }
        }

        public static bool SeatedMode
        {
            get => _seatedMode;
            internal set 
            { 
                _seatedMode = value;
                if (!VRRig.instance) return;
                VRRig.instance.SetVerticalOffset(0);
                VRRig.instance.UpdateWorldCanvasesPosition();
            }
        }

        public static bool ShowInteractionRay
        {
            get => _interactionRay;
            internal set { _interactionRay = value; }
        }

        public static void ShowCalibrateCanvas()
        {
            if (Runtime == VRRuntime.None) return;

            if (!calibrateCanvas)
            {
                calibrateCanvas = GameObject.Instantiate(VRAssetsManager.calibrateCanvasPrefab);
                Canvas canvas = calibrateCanvas.GetComponent<Canvas>();
                canvas.worldCamera = VRRig.instance.uiCamera;
                VRRig.instance.AddCameraCanvas(canvas);
                calibrateCanvas.AddComponent<UI.CalibrationController>();
                return;
            }

            calibrateCanvas.SetActive(true);
        }

        internal static void SetShowPlayspaceCenter(int enumIndex)
        {
            ShowPlayspaceCenter = (PlayspaceCenterDisplay)Mathf.Clamp(enumIndex, 0, 2);
        }

        internal static void RefreshHiddenSettings()
        {
            if (refreshHiddenSettings != null)
                refreshHiddenSettings.Invoke(ArmScale);
        }

        internal static VRPatcher.PatchErrorCode RetrievePlatform()
        {
            try
            {
                if (!File.Exists(RuntimeConfigPath))
                {
                    var stream = File.Create(RuntimeConfigPath);
                    stream.Close();
                    return VRPatcher.PatchErrorCode.Success;
                }
                else
                {
                    string platformConfig = File.ReadAllText(RuntimeConfigPath);

                    if (platformConfig.Contains("steamvr")) 
                        _platform = VRRuntime.SteamVR;
                    else if (platformConfig.Contains("oculus")) 
                        _platform = VRRuntime.Oculus;
                    else 
                        _platform = VRRuntime.None;

                    return VRPatcher.PatchErrorCode.AlreadyPatched;
                }
            }
            catch(Exception)
            {
                return VRPatcher.PatchErrorCode.Failed;
            }
        }

        public enum VRRuntime
        {
            None,
            SteamVR,
            Oculus
        }

        public enum PlayspaceCenterDisplay
        {
            Always,
            WhenFar,
            Never
        }
    }
}
