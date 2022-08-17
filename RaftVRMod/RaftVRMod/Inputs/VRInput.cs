using RaftVR.Configs;
using System;
using System.Collections.Generic;
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

        private static bool listenForKeyboard = false;

        private static string[] keyCodes;

        internal static string lastIdentifierKeyRetrieved;

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
                keyCodes = new string[]
                {
                    "backspace","delete","tab","clear","return","pause","escape","space","[0]","[1]","[2]","[3]","[4]","[5]","[6]","[7]","[8]","[9]",
                    "[.]","[/]","[*]","[-]","[+]","equals","enter","up","down","right","left","insert","home","end","page up","page down",
                    "f1","f2","f3","f4","f5","f6","f7","f8","f9","f10","f11","f12","f13","f14","f15","0","1","2","3","4","5","6","7","8","9",
                    "-","=","!","@","#","$","%","^", "&","*","(",")","_","+","[","]","`","{","}","~",";","'","\\",":","\"","|",",",".","/","<",">","?",
                    "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"
                };
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
                { "Calibrate", new ButtonState(currentPlatformInput.GetCalibrate) }
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
            if (!EventSystem.current.currentSelectedGameObject) return;

            InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();

            if (inputField)
            {
                StringBuilder keyboardText = inputField.characterLimit == 0 ? new StringBuilder(256) : new StringBuilder(inputField.characterLimit);
                SteamVR.instance.overlay.GetKeyboardText(keyboardText, inputField.characterLimit == 0 ? 256 : (uint)inputField.characterLimit);
                inputField.text = keyboardText.ToString();
            }
            else
            {
                TMP_InputField inputFieldTMP = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();

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

            //I'm not a big fan of this method, but I have no idea why the keyboard just won't work otherwise.
            if (listenForKeyboard)
            {
                if (!EventSystem.current.currentSelectedGameObject)
                {
                    listenForKeyboard = false;
                    return;
                }

                InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();

                if (inputField)
                {
                    foreach (string code in keyCodes)
                    {
                        if (Input.GetKeyDown(code))
                        {
                            inputField.ProcessEvent(Event.KeyboardEvent(code));
                            inputField.ForceLabelUpdate();
                        }
                    }
                }
                else
                {
                    listenForKeyboard = false;
                }
            }
        }

        public static void SetListenForKeyboard(bool listen)
        {
            listenForKeyboard = listen;
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
