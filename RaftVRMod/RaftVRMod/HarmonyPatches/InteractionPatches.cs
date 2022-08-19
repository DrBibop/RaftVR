using HarmonyLib;
using RaftVR.Configs;
using RaftVR.Rig;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    static class InteractionPatches
    {
        [HarmonyPatch(typeof(BlockCreator), "GetQuadAtCursor")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GetQuadAtCursor_ReplaceRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            codes[0].opcode = OpCodes.Nop;
            codes[0].operand = null;

            codes.RemoveRange(1, 7);

            codes.InsertRange(1, Enumerable.Concat(PatchUtils.dominantControllerPosition, PatchUtils.dominantControllerForward));

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(BlockCreator), "HandleBlockPick")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HandleBlockPick_ReplaceRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(BlockCreator), "playerNetwork"))) - 1;

            codes.RemoveRange(codeIndex, 8);

            codes.InsertRange(codeIndex, Enumerable.Concat(PatchUtils.dominantControllerPosition, PatchUtils.dominantControllerForward));

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(FillWaterComponent), "AimingAtTarget")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AimingAtTarget_ReplaceRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(AccessTools.PropertyGetter(typeof(Screen), "width"))) - 1;

            codes.RemoveRange(codeIndex, 11);

            codes.InsertRange(codeIndex, PatchUtils.dominantControllerRay);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(Helper), "FindInteractable")]
        [HarmonyPrefix]
        //The raycast started from the camera before. Now that the hand provides way more reach, let's reduce the ray distance to compensate.
        static bool FindInteractable_NerfRange(ref float interactDistance)
        {
            interactDistance /= 2;
            return true;
        }

        [HarmonyPatch(typeof(Helper), "FindInteractable")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FindInteractable_ReplaceRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            newCodes.Add(codes[0]);
            newCodes[0].opcode = OpCodes.Nop;
            newCodes[0].operand = null;

            codes.RemoveRange(0, 11);

            return newCodes.Concat(PatchUtils.dominantControllerRay).Concat(codes);
        }

        [HarmonyPatch(typeof(Sail), "OnIsRayed")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnIsRayed_ReplaceSailRotationAxis(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "Mouse X");

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 2);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Ldstr, "Turn"));
                codes.Insert(codeIndex + 1, new CodeInstruction(OpCodes.Call, typeof(Inputs.VRInput).GetMethod("GetAxis", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(SteeringWheel), "OnIsRayed")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnIsRayed_ReplaceWheelRotationAxis(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "Mouse X");

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 2);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Ldstr, "Turn"));
                codes.Insert(codeIndex + 1, new CodeInstruction(OpCodes.Call, typeof(Inputs.VRInput).GetMethod("GetAxis", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(RotationComponent), "OnIsRayed")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnIsRayed_ReplaceComponentRotationAxis(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "Mouse X");

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 2);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Ldstr, "Turn"));
                codes.Insert(codeIndex + 1, new CodeInstruction(OpCodes.Call, typeof(Inputs.VRInput).GetMethod("GetAxis", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(BlockCreator), "HandleRotationOfSelectedBlock")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HandleRotationOfSelectedBlock_ReplaceRotationAxis(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "Mouse X");

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 2);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Ldstr, "Turn"));
                codes.Insert(codeIndex + 1, new CodeInstruction(OpCodes.Call, typeof(Inputs.VRInput).GetMethod("GetAxis", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(Pickup), "RaycastForRayInteractables")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Pickup_ShowInteractionRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(IRaycastable).GetMethod("OnIsRayed", (BindingFlags)(-1))));

            if (codeIndex != -1)
            {
                codeIndex++;

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
                codes.Insert(codeIndex + 1, new CodeInstruction(OpCodes.Call, typeof(VRRig).GetMethod("ShowInteractionRay", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    static class HitAtCursorPatches
    {
        [HarmonyTargetMethods]
        static List<MethodBase> GetHitAtCursorMethods()
        {
            return typeof(Helper).GetMethods((BindingFlags)(-1)).Cast<MethodBase>().ToList().FindAll((x) => x.Name == "HitAtCursor" || x.Name == "HitAllAtCursor" || x.Name == "SphereHitAtCursor" || x.Name == "SphereHitAllAtCursor").ToList();
        }

        [HarmonyPrefix]
        //The raycast started from the camera before. Now that the hand provides way more reach, let's reduce the ray distance to compensate.
        static bool NerfRange(ref float rayDistance)
        {
            rayDistance /= 2;
            return true;
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceRay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>(PatchUtils.dominantControllerRay);

            int codeIndex = codes.FindIndex(x => x.Calls(AccessTools.PropertyGetter(typeof(Screen), "width"))) - 1;

            newCodes[0].labels = codes[codeIndex].labels;

            codes.RemoveRange(codeIndex, 11);

            codes.InsertRange(codeIndex, newCodes);

            return codes.AsEnumerable();
        }
    }
}
