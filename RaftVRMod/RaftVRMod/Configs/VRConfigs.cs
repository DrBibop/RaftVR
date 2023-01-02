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
        private static float _snapRepeatDelay = 0.3f;
        private static float _turnSpeed = 120;
        private static float _turnAngle = 45;
        private static DirectionOriginType _moveDirectionOrigin;
        private static bool _seatedMode = false;
        private static bool _isLeftHanded = false;
        private static bool _interactionRay = true;
        private static bool _immersiveThrowing = true;
        private static bool _immersiveBow = true;
        private static float _throwForceMultiplier = 1f;
        private static float _uiScale = 1f;
        private static RadialHotbarMode _useRadialHotbar = RadialHotbarMode.Always;
        private static PlayspaceCenterDisplay _playspaceCenterDisplay = PlayspaceCenterDisplay.WhenFar;
        private static bool _visibleBody = false;
        private static bool _underwaterDistortion = false;
        private static float _armScale = 1;
        private static float _legScale = 1;

        internal static Network_Player localNetworkPlayer;
        internal static PlayerAnimator localPlayerAnimator;
        internal static PersonController localPersonController;
        internal static GameObject calibrateCanvas;
        internal static Transform localBowTransform;
        internal static Vector3 origBowPos;
        internal static Quaternion origBowRot;

        public static event Action OnCalibrateSettingsUpdated;
        public static event Action OnFirstSetupDone;

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

        public static float SnapRepeatDelay
        {
            get => _snapRepeatDelay;
            set { _snapRepeatDelay = value; }
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

                if (value == 0 && VRRig.instance)
                    ShowCalibrateCanvas();
            }
        }

        public static float LegScale
        {
            get => _legScale;
            set
            {
                _legScale = value;
                if (localPlayerAnimator != null)
                {
                    // Have to call GetComponent so I don't have a variable of a type from an external library.
                    // Otherwise, it freaks out on first launch when the RootMotion assembly isn't loaded.
                    IKSolverVR solver = localPlayerAnimator.GetComponent<VRIK>().solver;
                    solver.leftLeg.legLengthMlp = value;
                    solver.rightLeg.legLengthMlp = value;
                }

                if (value == 0 && VRRig.instance)
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

        public static bool ImmersiveThrowing
        {
            get => _immersiveThrowing;
            set { _immersiveThrowing = value; }
        }

        public static bool ImmersiveBow
        {
            get => _immersiveBow;
            set {
                _immersiveBow = value;

                if (localNetworkPlayer && localBowTransform)
                {
                    if (value)
                    {
                        origBowPos = localBowTransform.localPosition;
                        origBowRot = localBowTransform.localRotation;

                        localBowTransform.SetParent(localNetworkPlayer.leftHandParent);
                        localBowTransform.localPosition = new Vector3(-0.1444f, 0.0318f, -0.0419f);
                        localBowTransform.localEulerAngles = new Vector3(-59.188f, 179.967f, -90.68101f);
                    }
                    else
                    {
                        localBowTransform.SetParent(localNetworkPlayer.rightHandParent);
                        localBowTransform.localPosition = origBowPos;
                        localBowTransform.localRotation = origBowRot;
                    }
                }
            }
        }

        public static float ThrowForceMultiplier
        {
            get => _throwForceMultiplier;
            set { _throwForceMultiplier = value; }
        }

        public static float UIScale
        {
            get => _uiScale;
            set { _uiScale = value; }
        }

        public static bool VisibleBody
        {
            get => _visibleBody;
            set 
            { 
                _visibleBody = value;

                if (localNetworkPlayer)
                {
                    localNetworkPlayer.currentModel.UpdateArmAndBodyMesh();
                }
            }
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

            if (!calibrateCanvas.activeSelf)
                calibrateCanvas.SetActive(true);
        }

        internal static void RefreshCalibrateSettings()
        {
            if (OnCalibrateSettingsUpdated != null)
                OnCalibrateSettingsUpdated();
        }

        internal static void FinishFirstSetup()
        {
            if (OnFirstSetupDone != null)
                OnFirstSetupDone();
        }

        public static void WriteRuntimeToFile(VRRuntime value)
        {
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
