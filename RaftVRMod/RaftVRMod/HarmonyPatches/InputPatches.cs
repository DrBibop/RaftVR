using HarmonyLib;
using RaftVR.Inputs;
using RaftVR.Configs;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    static class InputPatches
    {
        [HarmonyPatch(typeof(MyInput), "GetAxis")]
        [HarmonyPrefix]
        static bool GetVRAxis(string identifier, ref float __result)
        {
            if (VRInput.TryGetAxis(identifier, out __result))
                return false;

            return true;
        }

        [HarmonyPatch(typeof(MyInput), "GetButton")]
        [HarmonyPrefix]
        static bool GetVRButton(string identifier, ref bool __result)
        {
            if (VRInput.TryGetButton(identifier, out __result))
                return false;

            return true;
        }

        [HarmonyPatch(typeof(MyInput), "GetButtonDown")]
        [HarmonyPrefix]
        static bool GetVRButtonDown(string identifier, ref bool __result)
        {
            if (VRInput.TryGetButtonDown(identifier, out __result))
                return false;

            return true;
        }

        [HarmonyPatch(typeof(MyInput), "GetButtonUp")]
        [HarmonyPrefix]
        static bool GetVRButtonUp(string identifier, ref bool __result)
        {
            if (VRInput.TryGetButtonUp(identifier, out __result))
                return false;

            return true;
        }

        [HarmonyPatch(typeof(PauseMenu), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplacePauseBind(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(Input), "GetKeyDown", new System.Type[] { typeof(KeyCode) })));

            if (codeIndex != -1)
            {
                codeIndex--;

                codes[codeIndex].opcode = OpCodes.Ldstr;
                codes[codeIndex].operand = "Pause";

                codeIndex++;

                codes[codeIndex].operand = AccessTools.Method(typeof(MyInput), "GetButtonDown");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(MenuBox), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceMenuCloseBind(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(Input), "GetKeyDown", new System.Type[] { typeof(KeyCode) })));

            if (codeIndex != -1)
            {
                codeIndex--;

                codes[codeIndex].opcode = OpCodes.Ldstr;
                codes[codeIndex].operand = "Pause";

                codeIndex++;

                codes[codeIndex].operand = AccessTools.Method(typeof(MyInput), "GetButtonDown");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(InputField), "ActivateInputField")]
        [HarmonyPostfix]
        static void EnableKeyboard(InputField __instance)
        {
            VRInput.selectedInputField = __instance.gameObject;

            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR && Valve.VR.SteamVR.initializedState == Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
                Valve.VR.SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, __instance.name, __instance.characterLimit == 0 ? 256 : (uint)__instance.characterLimit, __instance.text, 0);
        }

        [HarmonyPatch(typeof(InputField), "DeactivateInputField")]
        [HarmonyPostfix]
        static void DisableKeyboard(InputField __instance)
        {
            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR && Valve.VR.SteamVR.initializedState == Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
                Valve.VR.SteamVR.instance.overlay.HideKeyboard();
        }

        [HarmonyPatch(typeof(TMPro.TMP_InputField), "ActivateInputField")]
        [HarmonyPostfix]
        static void EnableKeyboardTMP(TMPro.TMP_InputField __instance)
        {
            VRInput.selectedInputField = __instance.gameObject;

            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR && Valve.VR.SteamVR.initializedState == Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
                Valve.VR.SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, __instance.name, __instance.characterLimit == 0 ? 256 : (uint)__instance.characterLimit, __instance.text, 0);
        }

        [HarmonyPatch(typeof(TMPro.TMP_InputField), "DeactivateInputField")]
        [HarmonyPostfix]
        static void DisableKeyboardTMP(TMPro.TMP_InputField __instance)
        {
            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR && Valve.VR.SteamVR.initializedState == Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
                Valve.VR.SteamVR.instance.overlay.HideKeyboard();
        }

        [HarmonyPatch(typeof(DisplayText), "Show", typeof(string), typeof(KeyCode), typeof(int))]
        [HarmonyPostfix]
        static void ShowVRBind(DisplayText __instance, KeyCode key, int priority)
        {
            if (priority < (int)ReflectionInfos.displayTextPriorityField.GetValue(__instance))
            {
                return;
            }

            Text buttonText = (Text)ReflectionInfos.displayTextButtonTextField.GetValue(__instance);

            try
            {
                string identifier = VRInput.lastIdentifierKeyRetrieved == "" ? MyInput.Keybinds.First(x => x.Value.MainKey == key).Key : VRInput.lastIdentifierKeyRetrieved;

                buttonText.text = VRInput.GetLocalizedBind(identifier);

                VRInput.lastIdentifierKeyRetrieved = "";
            }
            catch { };
        }

        [HarmonyPatch(typeof(Keybind), "MainKey", MethodType.Getter)]
        [HarmonyPostfix]
        static void RegisterLastKeybind(Keybind __instance)
        {
            VRInput.lastIdentifierKeyRetrieved = __instance.Identifier;
        }
    }

    [HarmonyPatch]
    class RespawnCoroutinePatch
    {
        // Thank you Fynikoto for helping me with transpiling this IEnumerator!
        static MethodBase TargetMethod()
        {
            return typeof(Player).GetNestedType("<RespawnWait>d__40", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Player_ReplaceRespawnAnyKey(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(typeof(Input).GetProperty("anyKeyDown").GetGetMethod()));

            if (codeIndex != -1)
            {
                codes.RemoveAt(codeIndex);

                codes.Insert(codeIndex, new CodeInstruction(OpCodes.Call, typeof(VRInput).GetMethod("IsAnyButtonDown", (BindingFlags)(-1))));
            }

            return codes.AsEnumerable();
        }
    }
}
