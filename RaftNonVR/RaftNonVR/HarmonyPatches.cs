using HarmonyLib;
using Steamworks;

namespace RaftNonVR
{
    [HarmonyPatch]
    static class HarmonyPatches
    {
        [HarmonyPatch(typeof(Network_Player), "Start")]
        [HarmonyPostfix]
        static void SetupPlayer(Network_Player __instance)
        {
            if (__instance.IsLocalPlayer) return;

            __instance.gameObject.AddComponent<Network_VRIK>();
        }

        [HarmonyPatch(typeof(Network_Player), "Deserialize")]
        [HarmonyPrefix]
        static bool DeserializeVRIKMessages(Network_Player __instance, ref bool __result, Message_NetworkBehaviour msg, CSteamID remoteID)
        {
            if (msg.Type == Network_VRIK.MSG_HEAD || msg.Type == Network_VRIK.MSG_LEFT_HAND || msg.Type == Network_VRIK.MSG_RIGHT_HAND)
            {
                __result = Network_VRIK.DeserializeOnPlayer(__instance, msg, remoteID);
                return false;
            }

            return true;
        }
    }
}
