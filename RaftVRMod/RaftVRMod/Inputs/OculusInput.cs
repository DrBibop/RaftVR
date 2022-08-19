using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RaftVR.Inputs
{
    class OculusInput : IPlatformInput
    {
        InputDevice leftHand;
        InputDevice rightHand;

        Vector2 leftJoystick;
        Vector2 rightJoystick;

        float leftTrigger;
        float rightTrigger;

        HybridButton rClick = new HybridButton(KeyCode.JoystickButton9);
        HybridButton xButton = new HybridButton(KeyCode.JoystickButton2);
        HybridButton yButton = new HybridButton(KeyCode.JoystickButton3);

        Dictionary<string, string> buttonStrings = new Dictionary<string, string>()
        {
            { "LMB", "R Trigger" },
            { "RMB", "L Trigger" },
            { "MMB", "R Stick Click" },
            { "Interact", "R Grip" },
            { "Cancel", "Menu Button" },
            { "Sprint", "L Stick Click" },
            { "Jump", "A" },
            { "Crouch", "B" },
            { "Inventory", "X" },
            { "Drop", "Hold X" },
            { "BlockPick", "Hold Y" },
            { "Rotate", "L Grip + R Stick" },
            { "Remove", "Hold R Stick" },
            { "Pause", "Menu Button" },
            { "Click", "R Trigger" },
            { "NextItem", "R Stick Up" },
            { "PrevItem", "R Stick Down" },
            { "NoteBookToggle", "Y" },
            { "LeftControl", "B" }
        };

        public void Update()
        {
            leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (leftHand != null)
            {
                leftJoystick = GetJoystickValue(leftHand);
                leftTrigger = GetAxisValue(leftHand, CommonUsages.trigger);
            }

            if (rightHand != null)
            {
                rightJoystick = GetJoystickValue(rightHand);
                rightTrigger = GetAxisValue(rightHand, CommonUsages.trigger);
            }

            rClick.Update();
            xButton.Update();
            yButton.Update();
        }

        private Vector2 GetJoystickValue(InputDevice device)
        {
            Vector2 value = Vector2.zero;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out value))
            {
                if (value.magnitude < 0.1f) return Vector2.zero;
            }
            return value;
        }

        private float GetAxisValue(InputDevice device, InputFeatureUsage<float> axisSource)
        {
            float value = 0f;
            device.TryGetFeatureValue(axisSource, out value);
            return value;
        }

        public bool GetBlockPick()
        {
            //Hold Y
            return yButton.LongPress;
        }

        public bool GetCancel()
        {
            return GetPause();
        }

        public bool GetCrouch()
        {
            //Button B
            return Input.GetKey(KeyCode.JoystickButton1);
        }

        public bool GetDrop()
        {
            //Hold X
            return xButton.LongPress;
        }

        public bool GetInteract()
        {
            //R Grip
            return Input.GetKey(KeyCode.JoystickButton5);
        }

        public bool GetInventory()
        {
            //Button X
            return xButton.ShortPress;
        }

        public bool GetJump()
        {
            //Button A
            return Input.GetKey(KeyCode.JoystickButton0);
        }

        public bool GetPrimaryAction()
        {
            //R Trigger
            return rightTrigger >= 0.8f;
        }

        public bool GetRemove()
        {
            //Hold R Stick Click
            return rClick.LongPress;
        }

        public bool GetClick()
        {
            //R Trigger
            return GetPrimaryAction();
        }

        public bool GetRotate()
        {
            //LGrip
            return Input.GetKey(KeyCode.JoystickButton4);
        }

        public bool GetSecondaryAction()
        {
            //L Trigger
            return leftTrigger >= 0.8f;
        }

        public bool GetSprint()
        {
            //L Stick Click
            return Input.GetKey(KeyCode.JoystickButton8);
        }

        public float GetStrafe()
        {
            return leftJoystick.x;
        }

        public bool GetRepair()
        {
            //R Stick Click
            return rClick.ShortPress;
        }

        public float GetWalk()
        {
            return leftJoystick.y;
        }

        public float GetTurn()
        {
            return rightJoystick.x;
        }

        public bool GetPause()
        {
            //Menu Button
            return Input.GetKey(KeyCode.JoystickButton6);
        }

        public bool GetNextItem()
        {
            return rightJoystick.y < -0.8f;
        }

        public bool GetPreviousItem()
        {
            return rightJoystick.y > 0.8f;
        }

        public bool GetNotebook()
        {
            //Button Y
            return yButton.ShortPress;
        }

        public string GetBindString(string identifier)
        {
            if (buttonStrings.TryGetValue(identifier, out string result))
            {
                return result;
            }
            return "?";
        }

        public bool IsReady()
        {
            return true;
        }

        public bool GetPaintOneSide()
        {
            //Who cares about this anyway?
            return false;
        }

        public bool GetCalibrate()
        {
            return GetPrimaryAction() && GetSecondaryAction();
        }

        public class HybridButton
        {
            public bool ShortPress { get; private set; }
            public bool LongPress { get; private set; }

            private float holdTime;

            private KeyCode buttonCode;

            public HybridButton(KeyCode buttonCode)
            {
                this.buttonCode = buttonCode;
            }

            public void Update()
            {
                ShortPress = false;
                LongPress = false;

                bool pressing = Input.GetKey(buttonCode);

                if (pressing)
                {
                    holdTime += Time.deltaTime;

                    if (holdTime > 0.5f)
                    {
                        LongPress = true;
                    }
                }
                else
                {
                    if (holdTime > 0f && holdTime <= 0.5f)
                    {
                        ShortPress = true;
                    }

                    holdTime = 0f;
                }
            }
        }
    }
}
