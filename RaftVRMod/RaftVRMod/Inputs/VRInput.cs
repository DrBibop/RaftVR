using RaftVR.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

namespace RaftVR.Inputs
{
    static class VRInput
    {
        private static IPlatformInput currentPlatformInput;

        private static Dictionary<string, ButtonState> buttonStates;
        private static Dictionary<string, Func<float>> axisValues;

        private static int lastUpdateFrame = -1;

        private static bool hasUpdatedThisFrame => Time.frameCount == lastUpdateFrame;

        internal static string lastIdentifierKeyRetrieved;

        internal static GameObject selectedInputField;

        public static void Init(VRConfigs.VRRuntime platform)
        {
            if (platform == VRConfigs.VRRuntime.SteamVR)
            {
                currentPlatformInput = new SteamVRInput();
                SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
            }
            else
            {
                currentPlatformInput = new OculusInput();
            }

            buttonStates = new Dictionary<string, ButtonState>()
            {
                { "LMB", new ButtonState(currentPlatformInput.GetPrimaryAction) },
                { "RMB", new ButtonState(currentPlatformInput.GetSecondaryAction) },
                { "MMB", new ButtonState(currentPlatformInput.GetRepair) },
                { "Interact", new ButtonState(currentPlatformInput.GetInteract) },
                { "Cancel", new ButtonState(currentPlatformInput.GetCancel) },
                { "Sprint", new ButtonState(currentPlatformInput.GetSprint) },
                { "Jump", new ButtonState(currentPlatformInput.GetJump) },
                { "Crouch", new ButtonState(currentPlatformInput.GetCrouch) },
                { "Inventory", new ButtonState(currentPlatformInput.GetInventory) },
                { "Drop", new ButtonState(currentPlatformInput.GetDrop) },
                { "BlockPick", new ButtonState(currentPlatformInput.GetBlockPick) },
                { "Rotate", new ButtonState(currentPlatformInput.GetRotate) },
                { "Remove", new ButtonState(currentPlatformInput.GetRemove) },
                { "Pause", new ButtonState(currentPlatformInput.GetPause) },
                { "Click", new ButtonState(currentPlatformInput.GetClick) },
                { "NextItem", new ButtonState(currentPlatformInput.GetNextItem) },
                { "PrevItem", new ButtonState(currentPlatformInput.GetPreviousItem) },
                { "NoteBookToggle", new ButtonState(currentPlatformInput.GetNotebook) },
                { "LeftControl", new ButtonState(currentPlatformInput.GetCrouch) },
                { "Calibrate", new ButtonState(currentPlatformInput.GetCalibrate) },
                { "RadialHotbar", new ButtonState(currentPlatformInput.GetRadialHotbar) }
            };

            axisValues = new Dictionary<string, Func<float>>()
            {
                { "Walk", currentPlatformInput.GetWalk },
                { "Strafe", currentPlatformInput.GetStrafe },
                { "Turn", currentPlatformInput.GetTurn }
            };
        }

        private static void OnKeyboardClosed(VREvent_t args)
        {
            if (!selectedInputField) return;

            InputField inputField = selectedInputField.GetComponent<InputField>();

            if (inputField)
            {
                StringBuilder keyboardText = inputField.characterLimit == 0 ? new StringBuilder(256) : new StringBuilder(inputField.characterLimit);
                SteamVR.instance.overlay.GetKeyboardText(keyboardText, inputField.characterLimit == 0 ? 256 : (uint)inputField.characterLimit);
                inputField.text = keyboardText.ToString();
            }
            else
            {
                TMP_InputField inputFieldTMP = selectedInputField.GetComponent<TMP_InputField>();

                if (inputFieldTMP)
                {
                    StringBuilder keyboardText = inputFieldTMP.characterLimit == 0 ? new StringBuilder(256) : new StringBuilder(inputFieldTMP.characterLimit);
                    SteamVR.instance.overlay.GetKeyboardText(keyboardText, inputFieldTMP.characterLimit == 0 ? 256 : (uint)inputFieldTMP.characterLimit);
                    inputFieldTMP.text = keyboardText.ToString();
                }
            }
        }

        public static void Update()
        {
            if (currentPlatformInput == null || hasUpdatedThisFrame || !currentPlatformInput.IsReady()) return;

            lastUpdateFrame = Time.frameCount;

            currentPlatformInput.Update();

            foreach (ButtonState buttonState in buttonStates.Values)
            {
                buttonState.Update();
            }
        }

        public static bool TryGetAxis(string identifier, out float value)
        {
            Update();
            Func<float> axisRetriever;
            if (axisValues.TryGetValue(identifier, out axisRetriever))
            {
                value = axisRetriever.Invoke();
                return true;
            }
            value = 0f;
            return false;
        }

        public static float GetAxis(string identifier)
        {
            Update();
            Func<float> axisRetriever;
            if (axisValues.TryGetValue(identifier, out axisRetriever))
            {
                return axisRetriever.Invoke();
            }
            return 0f;
        }

        public static bool TryGetButton(string identifier, out bool value)
        {
            Update();
            ButtonState buttonState;
            if (TryGetButtonState(identifier, out buttonState))
            {
                value = buttonState.state;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryGetButtonDown(string identifier, out bool value)
        {
            Update();
            ButtonState buttonState;
            if (TryGetButtonState(identifier, out buttonState))
            {
                value = buttonState.justPressed;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryGetButtonUp(string identifier, out bool value)
        {
            Update();
            ButtonState buttonState;
            if (TryGetButtonState(identifier, out buttonState))
            {
                value = buttonState.justReleased;
                return true;
            }
            value = false;
            return false;
        }

        public static bool IsAnyButtonDown()
        {
            Update();
            foreach (ButtonState buttonState in buttonStates.Values)
            {
                if (buttonState.state) return true;
            }

            return Input.anyKeyDown;
        }

        private static bool TryGetButtonState(string identifier, out ButtonState value)
        {
            Update();
            return (buttonStates.TryGetValue(identifier, out value));
        }

        public static string GetLocalizedBind(string identifier)
        {
            return currentPlatformInput.GetBindString(identifier);
        }
    }
}
