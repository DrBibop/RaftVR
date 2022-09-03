using RaftVR.Patching;
using RaftVR.Rig;
using RaftVR.Utils;
using RootMotion.FinalIK;
using System;
using System.IO;
using UnityEngine;

namespace RaftVR.Configs
{
    public static class VRConfigs
    {
        private static string GamePath => Environment.CurrentDirectory;
        private static string RuntimeConfigPath => Path.Combine(GamePath, "VRRUNTIME.txt");

        private static VRRuntime _runtime = VRRuntime.None;
        private static bool _snapTurn = false;
        private static float _turnSpeed = 90;
        private static float _turnAngle = 45;
        private static DirectionOriginType _moveDirectionOrigin;
        private static bool _seatedMode = false;
        private static bool _isLeftHanded = false;
        private static bool _interactionRay = true;
        private static RadialHotbarMode _useRadialHotbar = RadialHotbarMode.Always;
        private static PlayspaceCenterDisplay _playspaceCenterDisplay = PlayspaceCenterDisplay.WhenFar;
        private static bool _underwaterDistortion = false;
        private static float _armScale = 1;

        internal static PlayerAnimator localPlayerAnimator;
        internal static PersonController localPersonController;
        internal static GameObject calibrateCanvas;

        public static event Action OnArmScaleChanged;
        public static event Action OnFirstSetupDone;

        internal static Action<float> refreshHiddenSettingsAction;

        public static VRRuntime Runtime
        {
            get => _runtime;
            internal set { _runtime = value; }
        }

        public static bool SnapTurn
        {
            get => _snapTurn;
            set { _snapTurn = value; }
        }

        public static bool IsLeftHanded
        {
            get => _isLeftHanded;
            set { _isLeftHanded = value; }
        }

        public static float SmoothTurnSpeed
        {
            get => _turnSpeed;
            set { _turnSpeed = value; }
        }

        public static float SnapTurnAngle
        {
            get => _turnAngle;
            set { _turnAngle = value; }
        }

        public static DirectionOriginType MoveDirectionOrigin
        {
            get => _moveDirectionOrigin;
            set 
            {
                if (_moveDirectionOrigin == value) return;

                _moveDirectionOrigin = value;

                if (localPersonController)
                    ReflectionInfos.personControllerCamTransformField.SetValue(localPersonController, value == DirectionOriginType.Head ? VRRig.instance.camera.transform : VRRig.instance.LeftController.transform);
            }
        }

        public static PlayspaceCenterDisplay ShowPlayspaceCenter
        {
            get => _playspaceCenterDisplay;
            set { _playspaceCenterDisplay = value; }
        }

        public static float ArmScale
        {
            get => _armScale;
            set 
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
            set 
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
            set { _interactionRay = value; }
        }

        public static RadialHotbarMode UseRadialHotbar
        {
            get => _useRadialHotbar;
            set { _useRadialHotbar = value; }
        }

        public static bool UnderwaterDistortion
        {
            get => _underwaterDistortion;
            set { _underwaterDistortion = value; }
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

        public static void SetMoveDirectionOrigin(int index)
        {
            MoveDirectionOrigin = (DirectionOriginType)Mathf.Clamp(index, 0, 1);
        }

        public static void SetShowPlayspaceCenter(int index)
        {
            ShowPlayspaceCenter = (PlayspaceCenterDisplay)Mathf.Clamp(index, 0, 2);
        }

        public static void SetRadialHotbarMode(int index)
        {
            UseRadialHotbar = (RadialHotbarMode)Mathf.Clamp(index, 0, 1);
        }

        internal static void RefreshHiddenSettings()
        {
            if (OnArmScaleChanged != null)
                OnArmScaleChanged();

            refreshHiddenSettingsAction.Invoke(ArmScale);
        }

        internal static void FinishFirstSetup()
        {
            if (OnFirstSetupDone != null)
                OnFirstSetupDone();
        }

        public static void WriteRuntimeToFile(int index)
        {
            VRRuntime value = (VRRuntime)index;

            if (value == Runtime) return;

            string configText;

            switch (value)
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
                Debug.Log("[RaftVR] Runtime has been set to: " + configText);
            }
            catch (Exception e)
            {
                Debug.LogError("[RaftVR] Failed to write platform choice to file:");
                Debug.LogException(e);
            }
        }

        internal static VRPatcher.PatchErrorCode RetrieveRuntime()
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
                        _runtime = VRRuntime.SteamVR;
                    else if (platformConfig.Contains("oculus")) 
                        _runtime = VRRuntime.Oculus;
                    else 
                        _runtime = VRRuntime.None;

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

        public enum DirectionOriginType
        {
            Head,
            Controller
        }

        public enum RadialHotbarMode
        {
            Always,
            WhenHoldingDirection
        }
    }
}
