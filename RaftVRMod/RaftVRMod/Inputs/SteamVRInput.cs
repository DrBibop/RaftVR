using RaftVR.Configs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace RaftVR.Inputs
{
    class SteamVRInput : IPlatformInput
    {
        Dictionary<string, ISteamVR_Action_In_Source> identifierBinds;
        Dictionary<string, ISteamVR_Action_In_Source> holdIdentifierBinds;
        ISteamVR_Action_In_Source[] turnActions;

        HybridButton inventoryNotebookButton;
        HybridButton pauseDropButton;
        HybridButton repairRemoveButton;
        HybridButton blockPickPaintOneSideButton;

        Dictionary<string, string> localizedPartsToKeep = new Dictionary<string, string>()
        {
            { "Left", "Left"},
            { "Right", "Right"},
            { "A", "A"},
            { "B", "B"},
            { "X", "X"},
            { "Y", "Y"},
            { "Joystick", "Stick"},
            { "Stick", "Stick"},
            { "Trackpad", "Pad"},
            { "Grip", "Grip"},
            { "Trigger", "Trigger"},
            { "Menu", "Menu" }
        };

        public void Init()
        {
            identifierBinds = new Dictionary<string, ISteamVR_Action_In_Source>()
            {
                { "LMB", SteamVR_Actions.gameplay_primary_action },
                { "RMB", SteamVR_Actions.gameplay_secondary_action },
                { "MMB", SteamVR_Actions.gameplay_repair_hold_remove },
                { "Interact", SteamVR_Actions.gameplay_interact },
                { "Cancel", SteamVR_Actions.gameplay_pause_hold_drop },
                { "Sprint", SteamVR_Actions.gameplay_sprint },
                { "Jump", SteamVR_Actions.gameplay_jump },
                { "Crouch", SteamVR_Actions.gameplay_crouch },
                { "Inventory", SteamVR_Actions.gameplay_inventory_hold_notebook },
                { "Rotate", SteamVR_Actions.gameplay_rotate },
                { "Pause", SteamVR_Actions.gameplay_pause_hold_drop },
                { "Click", SteamVR_Actions.gameplay_primary_action },
                { "NextItem", SteamVR_Actions.gameplay_next_item },
                { "PrevItem", SteamVR_Actions.gameplay_previous_item },
                { "LeftControl", SteamVR_Actions.gameplay_crouch },
                { "BlockPick", SteamVR_Actions.gameplay_blockpick_hold_paintoneside },
                { "Drop", SteamVR_Actions.gameplay_drop },
                { "Remove", SteamVR_Actions.gameplay_remove },
                { "NoteBookToggle", SteamVR_Actions.gameplay_notebook },
                { "PaintOneSide", SteamVR_Actions.gameplay_paintoneside },
                { "Move", SteamVR_Actions.gameplay_paintoneside },
                { "RenameAnimal", SteamVR_Actions.gameplay_rotate }
            };

            holdIdentifierBinds = new Dictionary<string, ISteamVR_Action_In_Source>()
            {
                { "Drop", SteamVR_Actions.gameplay_pause_hold_drop },
                { "Remove", SteamVR_Actions.gameplay_repair_hold_remove },
                { "NoteBookToggle", SteamVR_Actions.gameplay_inventory_hold_notebook },
                { "PaintOneSide", SteamVR_Actions.gameplay_blockpick_hold_paintoneside }
            };

            turnActions = new ISteamVR_Action_In_Source[]
            {
                SteamVR_Actions.gameplay_turn_axis,
                SteamVR_Actions.gameplay_turn_left,
                SteamVR_Actions.gameplay_turn_right
            };

            inventoryNotebookButton = new HybridButton(SteamVR_Actions.gameplay_inventory_hold_notebook);
            pauseDropButton = new HybridButton(SteamVR_Actions.gameplay_pause_hold_drop);
            repairRemoveButton = new HybridButton(SteamVR_Actions.gameplay_repair_hold_remove);
            blockPickPaintOneSideButton = new HybridButton(SteamVR_Actions.gameplay_blockpick_hold_paintoneside);
        }

        public bool GetBlockPick()
        {
            return blockPickPaintOneSideButton.ShortPress;
        }

        public bool GetCancel()
        {
            return pauseDropButton.ShortPress;
        }

        public bool GetCrouch()
        {
            return SteamVR_Actions.gameplay_crouch.state;
        }

        public bool GetDrop()
        {
            return pauseDropButton.LongPress || SteamVR_Actions.gameplay_drop.state;
        }

        public bool GetInteract()
        {
            return SteamVR_Actions.gameplay_interact.state;
        }

        public bool GetInventory()
        {
            return inventoryNotebookButton.ShortPress;
        }

        public bool GetJump()
        {
            return SteamVR_Actions.gameplay_jump.state;
        }

        public bool GetNextItem()
        {
            return SteamVR_Actions.gameplay_next_item.state;
        }

        public bool GetNotebook()
        {
            return inventoryNotebookButton.LongPress || SteamVR_Actions.gameplay_notebook.state;
        }

        public bool GetPause()
        {
            return pauseDropButton.ShortPress;
        }

        public bool GetPreviousItem()
        {
            return SteamVR_Actions.gameplay_previous_item.state;
        }

        public bool GetPrimaryAction()
        {
            return SteamVR_Actions.gameplay_primary_action.state;
        }

        public bool GetRemove()
        {
            return repairRemoveButton.LongPress || SteamVR_Actions.gameplay_remove.state;
        }

        public bool GetRepair()
        {
            return repairRemoveButton.ShortPress;
        }

        public bool GetClick()
        {
            return VRConfigs.IsLeftHanded ? SteamVR_Actions.ui_left_hand_click.state : SteamVR_Actions.ui_right_hand_click.state;
        }

        public bool GetRotate()
        {
            return SteamVR_Actions.gameplay_rotate.state;
        }

        public bool GetSecondaryAction()
        {
            return SteamVR_Actions.gameplay_secondary_action.state;
        }

        public bool GetSprint()
        {
            return SteamVR_Actions.gameplay_sprint.state;
        }

        public bool GetPaintOneSide()
        {
            return blockPickPaintOneSideButton.LongPress || SteamVR_Actions.gameplay_paintoneside.state;
        }

        public float GetStrafe()
        {
            return SteamVR_Actions.gameplay_move.axis.x;
        }

        public float GetTurn()
        {
            float axis = SteamVR_Actions.gameplay_turn_axis.axis.x;
            if (SteamVR_Actions.gameplay_turn_left.state) axis -= 1f;
            if (SteamVR_Actions.gameplay_turn_right.state) axis += 1f;
            return Mathf.Clamp(axis, -1f, 1f);
        }

        public float GetWalk()
        {
            return SteamVR_Actions.gameplay_move.axis.y;
        }

        public void Update() 
        {
            if (identifierBinds == null && IsReady())
                Init();

            inventoryNotebookButton.Update();
            pauseDropButton.Update();
            repairRemoveButton.Update();
            blockPickPaintOneSideButton.Update();
        }

        public bool IsReady()
        {
            return SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess;
        }

        public string GetBindString(string identifier)
        {
            string bindingString = "";
            ISteamVR_Action_In_Source action = null;
            bool bindFound = false;

            if (identifier == "Turn")
            {
                foreach (ISteamVR_Action_In_Source turnAction in turnActions)
                {
                    if (turnAction.activeBinding)
                    {
                        action = turnAction;
                        bindFound = true;
                        break;
                    }
                }
            }
            else if (identifierBinds.TryGetValue(identifier, out action) && action.activeBinding)
            {
                bindFound = true;
            }
            else if (holdIdentifierBinds.TryGetValue(identifier, out action) && action.activeBinding)
            {
                bindFound = true;
                bindingString = "Hold ";
            }

            if (bindFound)
                bindingString += GetBindString(action);
            else
                bindingString = "?";

            if (identifier == "Rotate")
                return bindingString + " + " + GetBindString("Turn");

            return bindingString;
        }

        private string GetBindString(ISteamVR_Action_In_Source action)
        {
            string fullSource = action.localizedOriginName;

            List<string> parts = fullSource.Split(' ').ToList();

            parts.RemoveAll(part => !localizedPartsToKeep.Keys.ToArray().Contains(part));

            List<string> replacedParts = new List<string>();

            foreach (string part in parts)
            {
                if (localizedPartsToKeep.TryGetValue(part, out string replacement) && replacement != part)
                    replacedParts.Add(replacement);
                else
                    replacedParts.Add(part);
            }

            return string.Join(" ", replacedParts);
        }

        public bool GetCalibrate()
        {
            return SteamVR_Actions.ui_left_hand_click.state && SteamVR_Actions.ui_right_hand_click.state;
        }

        public bool GetRadialHotbar()
        {
            return SteamVR_Actions.gameplay_radial_hotbar.state;
        }

        public class HybridButton
        {
            public bool ShortPress { get; private set; }
            public bool LongPress { get; private set; }

            private float holdTime = 0f;

            private SteamVR_Action_Boolean action;

            public HybridButton(SteamVR_Action_Boolean action)
            {
                this.action = action;
            }

            public void Update()
            {
                ShortPress = false;
                LongPress = false;

                bool pressing = action.state;

                if (pressing)
                {
                    holdTime = Time.realtimeSinceStartup - action.changedTime;

                    if (holdTime > 0.5f)
                        LongPress = true;
                }
                else
                {
                    if (holdTime > 0f && holdTime <= 0.5f)
                        ShortPress = true;

                    holdTime = 0f;
                }
            }
        }
    }
}
