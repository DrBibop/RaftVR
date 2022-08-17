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
    }
}
