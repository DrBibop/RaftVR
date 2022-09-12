using HarmonyLib;
using RaftVR.Configs;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    class ThrowablePatches
    {
        [HarmonyPatch(typeof(Throwable), "Initialize")]
        [HarmonyPostfix]
        //Since the events were deleted to remove the throwing delay, we have to disable this field.
        static void UncheckAnimationEvent(Throwable __instance)
        {
            __instance.thrownByAnimationEvent = false;
        }

        [HarmonyPatch(typeof(Throwable), "HandleLocalClient")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Throwable_UseHandDirection(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(ChargeMeter), "Charge")));

            if (codeIndex != -1)
            {
                Label immersiveLabel = ilGen.DefineLabel();
                Label postLabel = ilGen.DefineLabel();

                List<CodeInstruction> conditionCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, typeof(VRConfigs).GetProperty("ImmersiveThrowing", (BindingFlags)(-1)).GetGetMethod()),
                    new CodeInstruction(OpCodes.Brtrue_S, immersiveLabel)
                };

                List<CodeInstruction> immersiveThrowCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Br_S, postLabel),
                    new CodeInstruction(OpCodes.Call, typeof(PatchUtils).GetMethod("SetThrowChargeToMotion", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                };

                immersiveThrowCodes[1].labels.Add(immersiveLabel);

                codes[codeIndex + 1].labels.Add(postLabel);

                codes.InsertRange(codeIndex + 1, immersiveThrowCodes);

                codes.InsertRange(codeIndex, conditionCodes);
            }

            codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.PropertyGetter(typeof(Camera), "main")));

            if (codeIndex != -1)
            {
                Label immersiveLabel = ilGen.DefineLabel();
                Label postLabel = ilGen.DefineLabel();

                List<CodeInstruction> conditionCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, typeof(VRConfigs).GetProperty("ImmersiveThrowing", (BindingFlags)(-1)).GetGetMethod()),
                    new CodeInstruction(OpCodes.Brtrue_S, immersiveLabel)
                };

                List<CodeInstruction> immersiveThrowCodes = new List<CodeInstruction>(PatchUtils.dominantMotionForward);

                immersiveThrowCodes.Insert(0, new CodeInstruction(OpCodes.Br_S, postLabel));

                immersiveThrowCodes[1].labels.Add(immersiveLabel);

                codes[codeIndex + 3].labels.Add(postLabel);

                codes.InsertRange(codeIndex + 3, immersiveThrowCodes);

                codes.RemoveRange(codeIndex, 3);

                codes.InsertRange(codeIndex, PatchUtils.dominantControllerForward);

                codes.InsertRange(codeIndex, conditionCodes);
            }

            codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.PropertyGetter(typeof(Camera), "main")));

            if (codeIndex != -1)
            {
                Label immersiveLabel = ilGen.DefineLabel();
                Label postLabel = ilGen.DefineLabel();

                List<CodeInstruction> conditionCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, typeof(VRConfigs).GetProperty("ImmersiveThrowing", (BindingFlags)(-1)).GetGetMethod()),
                    new CodeInstruction(OpCodes.Brtrue_S, immersiveLabel)
                };

                List<CodeInstruction> immersiveThrowCodes = new List<CodeInstruction>(PatchUtils.dominantMotionRight);

                immersiveThrowCodes.Insert(0, new CodeInstruction(OpCodes.Br_S, postLabel));

                immersiveThrowCodes[1].labels.Add(immersiveLabel);

                codes[codeIndex + 3].labels.Add(postLabel);

                codes.InsertRange(codeIndex + 3, immersiveThrowCodes);

                codes.RemoveRange(codeIndex, 3);

                codes.InsertRange(codeIndex, PatchUtils.dominantControllerRight);

                codes.InsertRange(codeIndex, conditionCodes);
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(ThrowableComponent), "StartThrow")]
        [HarmonyPrefix]
        //Delay bad >:(
        static bool ThrowableComponent_RemoveDelay(ThrowableComponent __instance, ref float throwDelay)
        {
            throwDelay = 0;
            return true;
        }

        [HarmonyPatch(typeof(ThrowableComponent), "HandleLocalClient")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ThrowableComponent_UseMotionForce(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(ChargeMeter), "Charge")));

            if (codeIndex != -1)
            {
                Label postLabel = ilGen.DefineLabel();
                Label motionLabel = ilGen.DefineLabel();
                Label normalLabel = ilGen.DefineLabel();

                codes[codeIndex].labels.Add(normalLabel);
                codes[codeIndex + 1].labels.Add(postLabel);

                var newPostCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Br_S, postLabel),
                    new CodeInstruction(OpCodes.Call, typeof(PatchUtils).GetMethod("SetThrowChargeToMotion", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                };

                newPostCodes[1].labels.Add(motionLabel);

                var newPreCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, typeof(VRConfigs).GetProperty("ImmersiveThrowing", (BindingFlags)(-1)).GetGetMethod()),
                    new CodeInstruction(OpCodes.Brfalse_S, normalLabel),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Isinst, typeof(ThrowableComponent_Bow)),
                    new CodeInstruction(OpCodes.Brfalse_S, motionLabel)
                };

                codes.InsertRange(codeIndex + 1, newPostCodes);

                codes.InsertRange(codeIndex, newPreCodes);
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class ThrowableComponentCoroutinePatch
    {
        // Thank you Fynikoto for helping me with transpiling this IEnumerator!
        static MethodBase TargetMethod()
        {
            return typeof(ThrowableComponent).GetNestedType("<StartThrow>d__34", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ThrowableComponent_UseHandDirection(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instructions);

            List<CodeInstruction> currentTransformForward = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
                new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("forward").GetGetMethod())
            };

            List<CodeInstruction> currentTransformRight = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
                new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("right").GetGetMethod())
            };

            List<CodeInstruction> currentTransformBackward = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
                new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("forward").GetGetMethod()),
                new CodeInstruction(OpCodes.Call, typeof(Vector3).GetMethod("op_UnaryNegation", (BindingFlags)(-1)))
            };

            List<CodeInstruction> currentTransformLeft = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, typeof(Component).GetProperty("transform").GetGetMethod()),
                new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetProperty("right").GetGetMethod()),
                new CodeInstruction(OpCodes.Call, typeof(Vector3).GetMethod("op_UnaryNegation", (BindingFlags)(-1)))
            };

            for (int i = 0; i < 4; i++)
            {
                int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.PropertyGetter(typeof(Network_Player), "Camera")));

                if (codeIndex != -1)
                {
                    codeIndex -= 2;

                    Label motionLabel = ilGen.DefineLabel();
                    Label postLabel = ilGen.DefineLabel();
                    Label postLabel2 = ilGen.DefineLabel();
                    Label immersiveLabel = ilGen.DefineLabel();

                    List<CodeInstruction> newCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Isinst, i < 2 ? typeof(ThrowableComponent_Bow) : typeof(ThrowableComponent_NetGun)),
                        new CodeInstruction(OpCodes.Brfalse_S, motionLabel)
                    };

                    // Transform direction (bow/net gun)
                    newCodes.AddRange(new List<CodeInstruction>(i < 2 ? (i % 2 == 0 ? currentTransformBackward : currentTransformLeft) : (i % 2 == 0 ? currentTransformForward : currentTransformRight)));

                    newCodes.Add(new CodeInstruction(OpCodes.Br_S, postLabel));

                    // Motion/controller direction (everything else)
                    int motionIndex = newCodes.Count;

                    newCodes.AddRange(new List<CodeInstruction>() 
                    {
                        new CodeInstruction(OpCodes.Call, typeof(VRConfigs).GetProperty("ImmersiveThrowing", (BindingFlags)(-1)).GetGetMethod()),
                        new CodeInstruction(OpCodes.Brtrue_S, immersiveLabel)
                    });

                    newCodes.AddRange(new List<CodeInstruction>(i % 2 == 0 ? PatchUtils.dominantControllerForward : PatchUtils.dominantControllerRight));

                    newCodes.Add(new CodeInstruction(OpCodes.Br_S, postLabel2));

                    int immersiveIndex = newCodes.Count;

                    newCodes.AddRange(new List<CodeInstruction>(i % 2 == 0 ? PatchUtils.dominantMotionForward : PatchUtils.dominantMotionRight));

                    newCodes[immersiveIndex].labels.Add(immersiveLabel);
                    newCodes[motionIndex].labels.Add(motionLabel);

                    // Transfer entry point label
                    if (i % 2 == 0)
                    {
                        newCodes[0].labels.AddRange(codes[codeIndex].labels);
                    }

                    codes.RemoveRange(codeIndex, 5);

                    // Add post codes label
                    codes[codeIndex].labels.Add(postLabel);
                    codes[codeIndex].labels.Add(postLabel2);

                    codes.InsertRange(codeIndex, newCodes);
                }
            }

            return codes.AsEnumerable();
        }
    }
}
