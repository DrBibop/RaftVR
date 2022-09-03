using HarmonyLib;
using RaftVR.UI;
using RaftVR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    class UIPatches
    {
        [HarmonyPatch(typeof(StartMenuScreen), "Start")]
        [HarmonyPostfix]
        static void MoveMenuToWorld()
        {
            UIHelper.InitVRMenu();
            UIHelper.MoveSettingsCanvasToFront();
        }

        [HarmonyPatch(typeof(Helper), "OnWorldRecievedLate")]
        [HarmonyPostfix]
        static void MoveSettingsToHand()
        {
            UIHelper.MoveSettingsCanvasToHand();
            UIHelper.TryInitItemSpawnerCanvas();
        }

        [HarmonyPatch(typeof(Helper), "SetCursorVisible")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetCursorVisible_AlwaysHideAimSprite(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(Helper), "canvas")));

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 16);
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(OptionsMenuBox), "Close")]
        [HarmonyPrefix]
        static bool OptionsMenuBox_FixSettingsRef(OptionsMenuBox __instance)
        {
            if (ReflectionInfos.optionsMenuSettingsField.GetValue(__instance) == null)
                ReflectionInfos.optionsMenuSettingsField.SetValue(__instance, ComponentManager<Settings>.Value);

            return true;
        }

        [HarmonyPatch(typeof(Storage_Small), "Open")]
        [HarmonyPostfix]
        static void MoveStorageToLeftSide(Storage_Small __instance, Network_Player player)
        {
            if (!player.IsLocalPlayer) return;

            Inventory storageInventory = (Inventory)ReflectionInfos.storageInventoryRefField.GetValue(__instance);

            if (storageInventory != null)
            {
                RectTransform inventoryRect = storageInventory.transform as RectTransform;

                inventoryRect.pivot = Vector2.one;
                Vector3 newPos = inventoryRect.localPosition;
                newPos.x = -192;
                inventoryRect.localPosition = newPos;
            }
        }

        [HarmonyPatch(typeof(CraftingMenu), "OnEnable")]
        [HarmonyPostfix]
        static void MoveCraftingMenuToRightSide(CraftingMenu __instance)
        {
            RectTransform craftMenuRect = __instance.transform as RectTransform;
            craftMenuRect.anchoredPosition = new Vector3(218, -179, 0);
            craftMenuRect.offsetMin = new Vector2(193, -204);
            craftMenuRect.offsetMax = new Vector2(243, -154);
        }
    }
}
