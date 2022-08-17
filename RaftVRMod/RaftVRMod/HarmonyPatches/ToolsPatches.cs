using HarmonyLib;
using RaftVR.ItemComponents;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    class ToolsPatches
    {
        [HarmonyPatch(typeof(Paddle), "PaddlePaddle")]
        [HarmonyTranspiler]
        // Why is it not used in the first place?
        static IEnumerable<CodeInstruction> Paddle_UseDirectionArgument(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(Component).GetProperty("transform").GetGetMethod()));

            if (codeIndex != -1)
            {
                codeIndex -= 2;

                codes.RemoveRange(codeIndex, 4);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Ldarg_2));
            }

            return codes;
        }

        [HarmonyPatch(typeof(Paddle), "OnPaddle")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Paddle_UseHandDirection(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(Transform).GetProperty("forward").GetGetMethod()));

            if (codeIndex != -1)
            {
                codeIndex -= 3;

                codes.RemoveRange(codeIndex, 4);

                codes.InsertRange(codeIndex, PatchUtils.dominantControllerForwardFlat);
            }

            return codes;
        }

        [HarmonyPatch(typeof(AI_NetworkBehaviour_BugSwarm), "Start")]
        [HarmonyPrefix]
        static bool BugSwarm_AddSweepNetDetector(AI_NetworkBehaviour_BugSwarm __instance)
        {
            Transform colliderObject = __instance.transform.Find("ItemYield_BeeSwarm");

            if (colliderObject)
                colliderObject.gameObject.AddComponent<ToolDetector>();

            return true;
        }

        [HarmonyPatch(typeof(PickupItem), "Start")]
        [HarmonyPrefix]
        static bool PuckupItem_AddHookOrShovelDetector(PickupItem __instance)
        {
            if (__instance.gameObject.tag == "PickupHook" || __instance.gameObject.tag == "Pickup_Shovel")
                __instance.gameObject.AddComponent<ToolDetector>();

            return true;
        }

        [HarmonyPatch(typeof(UseableItem), "Update")]
        [HarmonyPrefix]
        static bool UsableItem_PreventShovelUse(UseableItem __instance)
        {
            if (__instance is Shovel)
                ReflectionInfos.itemCanChannelField.SetValue(__instance, false);

            return true;
        }

        [HarmonyPatch(typeof(Shovel), "ChannelItem")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Shovel_SkipRaycastAndOtherStuff(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(UseableItem).GetMethod("ChannelItem")));

            if (codeIndex != -1)
            {
                codeIndex -= 2;

                codes.RemoveRange(1, codeIndex);

                codes.Insert(1, new CodeInstruction(OpCodes.Pop));

                codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ret);

                codes.RemoveRange(codeIndex, 7);
            }

            return codes;
        }

        [HarmonyPatch(typeof(MetalDetector), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MetalDetector_ChangeDetectionPoint(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(Network_Player).GetProperty("FeetPosition").GetGetMethod()));

            if (codeIndex != -1)
            {
                codeIndex--;

                codes.RemoveRange(codeIndex, 2);

                var newCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
                    new CodeInstruction(OpCodes.Call, typeof(PatchUtils).GetMethod("GetMetalDetectorPos", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                };

                codes.InsertRange(codeIndex, newCodes);
            }

            return codes;
        }

        [HarmonyPatch(typeof(Hook), "HandleGathering")]
        [HarmonyPrefix]
        static bool Hook_VRGather()
        {
            return HookVR.instance.HandleGathering();
        }

        [HarmonyPatch(typeof(Binoculars), "Update")]
        [HarmonyPrefix]
        static bool Binoculars_DisableUpdate()
        {
            return false;
        }
    }
}
