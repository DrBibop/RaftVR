using HarmonyLib;
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
        static IEnumerable<CodeInstruction> Throwable_UseHandDirection(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(ChargeMeter), "Charge")));

            if (codeIndex != -1)
            {
                codes.RemoveAt(codeIndex);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Call, typeof(PatchUtils).GetMethod("SetThrowChargeToMotion", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)));
            }

            codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.PropertyGetter(typeof(Camera), "main")));

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 3);

                codes.InsertRange(codeIndex, PatchUtils.dominantMotionForward);
            }

            codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.PropertyGetter(typeof(Camera), "main")));

            if (codeIndex != -1)
            {
                codes.RemoveRange(codeIndex, 3);

                codes.InsertRange(codeIndex, PatchUtils.dominantMotionRight);
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

        [HarmonyPatch(typeof(ThrowableComponent), "Throw")]
        [HarmonyPrefix]
        static bool ThrowableComponent_DebugForce(Vector3 force)
        {
            Debug.LogWarning(force);
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

                codes[codeIndex + 1].labels.Add(postLabel);

                var newPostCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Br_S, postLabel),
                    new CodeInstruction(OpCodes.Call, typeof(PatchUtils).GetMethod("SetThrowChargeToMotion", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                };

                newPostCodes[1].labels.Add(motionLabel);

                var newPreCodes = new List<CodeInstruction>()
                {
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

                    Label normalLabel = ilGen.DefineLabel();
                    Label postLabel = ilGen.DefineLabel();

                    List<CodeInstruction> newCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Isinst, i < 2 ? typeof(ThrowableComponent_Bow) : typeof(ThrowableComponent_NetGun)),
                        new CodeInstruction(OpCodes.Brfalse_S, normalLabel)
                    };

                    // Transform direction (bow/net gun)
                    newCodes.AddRange(new List<CodeInstruction>(i < 2 ? (i % 2 == 0 ? currentTransformBackward : currentTransformLeft) : (i % 2 == 0 ? currentTransformForward : currentTransformRight)));

                    newCodes.Add(new CodeInstruction(OpCodes.Br_S, postLabel));

                    // Motion direction (everything else)
                    int normalIndex = newCodes.Count;

                    newCodes.AddRange(new List<CodeInstruction>(i % 2 == 0 ? PatchUtils.dominantMotionForward : PatchUtils.dominantMotionRight));

                    newCodes[normalIndex].labels.Add(normalLabel);

                    // Transfer entry point label
                    if (i % 2 == 0)
                    {
                        newCodes[0].labels.AddRange(codes[codeIndex].labels);
                    }

                    codes.RemoveRange(codeIndex, 5);

                    // Add post codes label
                    codes[codeIndex].labels.Add(postLabel);

                    codes.InsertRange(codeIndex, newCodes);
                }
            }

            return codes.AsEnumerable();
        }
    }
}
