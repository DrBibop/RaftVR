using HarmonyLib;
using RaftVR.Configs;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UltimateWater;
using UnityEngine;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    static class CameraPatches
    {
        [HarmonyPatch(typeof(EZCameraShake.CameraShaker), "Update")]
        [HarmonyPrefix]
        //This is just a recipe for motion sickness, so we are getting rid of them.
        static bool CameraShaker_DestroyShaker(EZCameraShake.CameraShaker __instance)
        {
            Object.Destroy(__instance);
            return false;
        }

        [HarmonyPatch(typeof(CharacterModelModifications), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CharacterModelModifications_ReplaceFOVCamera(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            var codes = new List<CodeInstruction>(instructions);

            int labelIndex = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(CharacterModelModifications), "settings"))) - 1;
            int branchIndex = codes.FindIndex(x => x.opcode == OpCodes.Brfalse_S && codes[labelIndex].labels.Contains((Label)x.operand));

            int newLabelIndex = codes.FindLastIndex(x => x.Calls(AccessTools.PropertySetter(typeof(Camera), "fieldOfView"))) + 1;
            int branch2Index = codes.FindIndex(x => x.opcode == OpCodes.Brfalse && codes[newLabelIndex].labels.Contains((Label)x.operand));

            codes[branchIndex].operand = codes[branch2Index].operand;

            codes.RemoveRange(labelIndex, 54);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(Helper), "OnWorldRecievedLate")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnWorldRecievedLate_ReplaceFOVCamera(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.PropertyGetter(typeof(Component), "transform")));

            codes.RemoveRange(index, 4);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(MouseLook), "Update")]
        [HarmonyPrefix]
        //The look logic is handled in the VR rig with the joystick. Let's make sure these don't interfere.
        static bool MouseLook_PreventUpdateIfLocalPlayer(MouseLook __instance)
        {
            if (__instance.transform.parent && Rig.VRRig.instance && __instance.transform.parent == Rig.VRRig.instance.transform)
                return false;

            Network_Player player = __instance.GetComponentInParent<Network_Player>();

            if (!player)
                return true;

            if (player.IsLocalPlayer)
                return false;

            return true;
        }
        
        [HarmonyPatch(typeof(UnderwaterIME), "RenderDistortions")]
        [HarmonyPrefix]
        static bool UnderwaterIME_DisableDistortion(UnderwaterIME __instance)
        {
            ReflectionInfos.waterDistortionField.Invoke(GameManager.Singleton.water.Materials, new object[] { VRConfigs.UnderwaterDistortion ? 0.03f : 0f });

            return true;
        }
    }
}
