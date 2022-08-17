using HarmonyLib;
using RaftVR.Rig;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RaftVR.Utils
{
    static class PatchUtils
    {
        private static Vector3 localMetalDetectorPos = new Vector3(0f, -0.7f, 0.43f);

        internal static List<CodeInstruction> dominantControllerPosition = new List<CodeInstruction>() 
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("position").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantControllerForward = new List<CodeInstruction>() 
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("forward").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantControllerRight = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("right").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantControllerRay = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Hand).GetProperty("Ray").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantMotionForward = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Hand).GetProperty("MotionForward").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantMotionRight = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Hand).GetProperty("MotionRight").GetGetMethod())
        };

        internal static List<CodeInstruction> dominantControllerForwardFlat = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, typeof(VRRig).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(VRRig).GetProperty("DominantController").GetGetMethod()),
            new CodeInstruction(OpCodes.Callvirt, typeof(Hand).GetMethod("GetForwardFlat"))
        };

        internal static void SetThrowChargeToMotion(ChargeMeter meter)
        {
            float throwSpeed = VRRig.instance.DominantController.MotionMagnitude / Time.deltaTime;

            float minCharge = (float)ReflectionInfos.chargeMeterMinChargeField.GetValue(meter);
            float maxCharge = (float)ReflectionInfos.chargeMeterMaxChargeField.GetValue(meter);

            ReflectionInfos.chargeMeterCurrentChargeField.SetValue(meter, Mathf.Clamp((throwSpeed / 6) * maxCharge, minCharge, maxCharge));
        }

        internal static Vector3 GetMetalDetectorPos(Transform transform)
        {
            return transform.TransformPoint(localMetalDetectorPos);
        }
    }
}
