using HarmonyLib;
using RaftVR.ItemComponents;
using RaftVR.Rig;
using RaftVR.Configs;
using RaftVR.UI;
using RaftVR.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.PostProcessing;
using RaftVR.Inputs;
using Steamworks;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    static class PlayerPatches
    {
        [HarmonyPatch(typeof(Network_Player), "Start")]
        [HarmonyPostfix]
        static void SetupPlayer(Network_Player __instance)
        {
            __instance.gameObject.AddComponent<Network_VRIK>();

            if (!__instance.IsLocalPlayer) return;

            VRConfigs.localNetworkPlayer = __instance;
            VRConfigs.localPlayerAnimator = __instance.Animator;

            //I'd like to have separate methods for these, but Harmony doesn't like that, so regions will do.
            #region Setup held objects
            MeleeWeapon[] meleeWeapons = __instance.rightHandParent.GetComponentsInChildren<MeleeWeapon>(true);
            MeleeWeapon machete = meleeWeapons.First(x => x.gameObject.name.Contains("Machete"));

            for (int i = 0; i < meleeWeapons.Length; i++)
            {
                MeleeWeapon meleeWeapon = meleeWeapons[i];

                if (meleeWeapon is Machete)
                {
                    if (meleeWeapon.attackMask != machete.attackMask)
                        meleeWeapon.attackMask = machete.attackMask;

                    CapsuleCollider collider = meleeWeapon.gameObject.AddComponent<CapsuleCollider>();

                    collider.center = new Vector3(0f, 0.35f, 0f);
                    collider.radius = 0.05f;
                    collider.height = 0.5f;
                    collider.direction = 1;
                }
                //Spears
                else
                {
                    CapsuleCollider collider = meleeWeapon.gameObject.AddComponent<CapsuleCollider>();

                    collider.center = new Vector3(0f, 0.7f, 0f);
                    collider.radius = 0.03f;
                    collider.height = 0.2f;
                    collider.direction = 1;

                    meleeWeapon.gameObject.AddComponent<TwoHandedToolVR>();
                }

                meleeWeapon.gameObject.AddComponent<MeleeWeaponVR>();
            }

            NoteBookUI noteBookUI = __instance.rightHandParent.GetComponentInChildren<NoteBookUI>(true);

            if (noteBookUI)
            {
                Canvas noteBookCanvas = noteBookUI.GetComponentInChildren<Canvas>(true);

                if (noteBookCanvas)
                {
                    noteBookCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
                    noteBookCanvas.gameObject.SetLayerRecursivly(LayerMask.NameToLayer("UI"));

                    BoxCollider pagesCollider = noteBookCanvas.gameObject.AddComponent<BoxCollider>();
                    pagesCollider.size = new Vector3(525, 425, 0);

                    BoxCollider backCollider = noteBookCanvas.gameObject.AddComponent<BoxCollider>();
                    backCollider.size = new Vector3(800, 425, 0);
                    backCollider.center = new Vector3(-160, 0, 20);

                    noteBookCanvas.gameObject.AddComponent<UICollider>().Init(noteBookCanvas);
                    noteBookCanvas.gameObject.AddComponent<WorldToUI>();
                }
            }

            ConsumeComponent[] foods = __instance.rightHandParent.GetComponentsInChildren<ConsumeComponent>(true); 

            for (int i = 0; i < foods.Length; i++)
            {
                foods[i].gameObject.AddComponent<ConsumableVR>();
            }

            Transform paddle = __instance.rightHandParent.GetComponentInChildren<Paddle>(true).transform;

            paddle.gameObject.AddComponent<WaterPointGetter>();
            paddle.gameObject.AddComponent<PaddleVR>();
            paddle.gameObject.AddComponent<TwoHandedToolVR>();

            Transform sweepNet = __instance.rightHandParent.GetComponentInChildren<SweepNet>(true).transform;

            sweepNet.gameObject.AddComponent<SweepNetVR>();
            sweepNet.gameObject.AddComponent<TwoHandedToolVR>().SetHoldDirection(Vector3.up, Quaternion.Euler(2.979f, 0.253f, 5.106f));

            Axe[] axes = __instance.rightHandParent.GetComponentsInChildren<Axe>(true);

            for (int i = 0; i < axes.Length; i++)
            {
                Axe axe = axes[i];
                string name = axe.gameObject.name;

                if (name.Contains("Stone"))
                {
                    BoxCollider collider = axe.gameObject.AddComponent<BoxCollider>();

                    collider.size = new Vector3(0.03f, 0.1f, 0.1f);
                    collider.center = new Vector3(0f, 0.43f, 0.1f);
                }
                else if (name.Contains("Titanium"))
                {
                    BoxCollider collider = axe.gameObject.AddComponent<BoxCollider>();

                    collider.size = new Vector3(0.03f, 0.1f, 0.1f);
                    collider.center = new Vector3(0f, 0.43f, 0.07f);
                }
                else
                {
                    BoxCollider collider = axe.gameObject.AddComponent<BoxCollider>();

                    collider.size = new Vector3(0.1f, 0.1f, 0.03f);
                    collider.center = new Vector3(0.13f, 0.43f, 0f);
                }

                axe.gameObject.AddComponent<AxeVR>();
            }

            Shovel shovel = __instance.rightHandParent.GetComponentInChildren<Shovel>(true);

            if (shovel)
            {
                shovel.gameObject.AddComponent<TwoHandedToolVR>().SetHoldDirection(new Vector3(0f, 0.031f, -1.043f).normalized, Quaternion.Euler(-26.814f, -183.797f, 162.765f));
                shovel.gameObject.AddComponent<ShovelVR>();
            }

            Hook[] hooks = __instance.rightHandParent.GetComponentsInChildren<Hook>(true);

            for (int i = 0; i < hooks.Length; i++)
            {
                hooks[i].gameObject.AddComponent<HookVR>();
            }

            
            ThrowableComponent_Bow bow = __instance.rightHandParent.GetComponentInChildren<ThrowableComponent_Bow>(true);

            if (bow)
            {
                VRConfigs.localBowTransform = bow.transform;

                if (VRConfigs.ImmersiveBow)
                {
                    // This looks redundant, but I just wanna call the "set" method that contains all the re-parenting code.
                    VRConfigs.ImmersiveBow = true;
                }
                
                bow.gameObject.AddComponent<BowVR>();
            }

            #endregion

            #region Setup camera rig
            Camera camera = __instance.Camera;
            camera.nearClipPlane = 0.02f;

            Camera uiCamera = __instance.HandCamera;
            uiCamera.gameObject.name = "UI Camera";
            GameObject.Destroy(uiCamera.GetComponent<PostProcessingBehaviour>());

            GameObject rigTarget = new GameObject("Rig Target");
            rigTarget.transform.SetParent(__instance.transform);
            rigTarget.transform.localRotation = Quaternion.identity;
            rigTarget.transform.localPosition = new Vector3(0, -__instance.GetComponent<CharacterController>().height / 2, 0);

            VRRig.instance.camera = camera;
            VRRig.instance.uiCamera = uiCamera;
            VRRig.instance.player = __instance;
            VRRig.instance.positionTarget = rigTarget.transform;

            VRRig.instance.InitHUD(__instance);
            #endregion

            //This little part is to remove the hook throwing delay.
            List<AnimationConnection> connections = (List<AnimationConnection>)ReflectionInfos.animationConnectionsField.GetValue(__instance.Animator.GetComponent<AnimationEventCaller>());
            connections.RemoveAll((connection) => connection.name.Contains("Animation_HookThrow"));
        }

        [HarmonyPatch(typeof(PersonController), "GroundControll")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveGroundNormalize(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(Vector3), "Normalize")));

            if (codeIndex != -1)
            {
                codeIndex -= 2;
                var labels = codes[codeIndex].labels;
                codes[codeIndex + 3].labels = labels;
                codes.RemoveRange(codeIndex, 3);
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(PersonController), "WaterControll")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveWaterNormalize(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(Vector3), "Normalize")));

            if (codeIndex != -1)
            {
                codeIndex += 1;
                Label label = ilGen.DefineLabel();
                codes[codeIndex].labels = new List<Label>() { label };

                // What this IL code does: if (this.moveDirection.y == 1f || this.moveDirection.y == -1f)
                // Basically, only normalize when holding jump or crouch.
                var newCodes = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(PersonController), "moveDirection")),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector3), "y")),
                    new CodeInstruction(OpCodes.Ldc_R4, 1f),
                    new CodeInstruction(OpCodes.Ceq),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(PersonController), "moveDirection")),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector3), "y")),
                    new CodeInstruction(OpCodes.Ldc_R4, -1f),
                    new CodeInstruction(OpCodes.Ceq),
                    new CodeInstruction(OpCodes.Or),
                    new CodeInstruction(OpCodes.Brfalse_S, label)
                };

                codeIndex -= 3;
                codes.InsertRange(codeIndex, newCodes);
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(PersonController), "Update")]
        [HarmonyPostfix]
        static void UpdateCrouch(PersonController __instance)
        {
            if (!((Network_Player)ReflectionInfos.personControllerNetworkPlayerField.GetValue(__instance)).IsLocalPlayer) return;

            VRRig.instance.SetVerticalOffset(__instance.crouching ? -__instance.controller.height : 0);
        }

        [HarmonyPatch(typeof(PersonController), "Start")]
        [HarmonyPostfix]
        static void SetMovementTransform(PersonController __instance)
        {
            if (!((Network_Player)ReflectionInfos.personControllerNetworkPlayerField.GetValue(__instance)).IsLocalPlayer) return;

            VRConfigs.localPersonController = __instance;

            if (VRConfigs.MoveDirectionOrigin == VRConfigs.DirectionOriginType.Controller)
                ReflectionInfos.personControllerCamTransformField.SetValue(__instance, VRRig.instance.LeftController.transform);
        }

        [HarmonyPatch(typeof(Player), "SetMouseLookScripts")]
        [HarmonyPostfix]
        static void SetTurnEnabled(Player __instance, bool canLook)
        {
            VRRig.instance.SetCanTurn(canLook);
        }

        [HarmonyPatch(typeof(Player), "SetLockMouseLook")]
        [HarmonyPostfix]
        //Quite the redundancy. Not sure why there are two methods that do basically the same thing but we have to patch both since they're both used.
        static void SetTurnEnabled2(Player __instance, bool isLocked)
        {
            VRRig.instance.SetCanTurn(!isLocked);
        }

        [HarmonyPatch(typeof(Hotbar), "HandleHotbarSelection")]
        [HarmonyPostfix]
        static void ChangeItem(Hotbar __instance)
        {
            if (CanvasHelper.ActiveMenu != MenuType.None ||
                __instance.playerNetwork.PlayerItemManager == null ||
                Player.LocalPlayerIsDead ||
                __instance.playerNetwork.RessurectComponent.IsCarrying ||
                !__instance.playerNetwork.PlayerItemManager.CanSwitch() ||
                __instance.playerNetwork.BlockCreator.IsRotating || 
                VRConfigs.UseRadialHotbar == VRConfigs.RadialHotbarMode.Always)
            {
                return;
            }

            int slotIndex = __instance.GetSelectedSlotIndex();
            if (VRInput.TryGetButtonDown("NextItem", out bool next))
            {
                if (next)
                    slotIndex++;
            }
            if (VRInput.TryGetButtonDown("PrevItem", out bool prev))
            {
                if (prev)
                    slotIndex--;
            }
            if (next || prev)
            {
                int hotslotCount = __instance.playerNetwork.Inventory.hotslotCount;
                if (slotIndex > hotslotCount - 1)
                {
                    slotIndex = 0;
                }
                else if (slotIndex < 0)
                {
                    slotIndex = hotslotCount - 1;
                }
                __instance.SetSelectedSlotIndex(slotIndex);
                Slot newSlot = __instance.playerNetwork.Inventory.GetSlot(slotIndex);
                __instance.SelectHotslot(newSlot);
                HotbarController.instance.RefreshVisibility();
                RadialHotbar.instance.SetSelectedIndex(slotIndex);
            }
        }

        [HarmonyPatch(typeof(ThirdPerson), "SetThirdPersonState")]
        [HarmonyPrefix]
        static bool PreventThirdPerson()
        {
            return false;
        }

        [HarmonyPatch(typeof(CharacterModelModifications), "UpdateArmAndBodyMesh")]
        [HarmonyPostfix]
        static void CheckVisibleBodyConfigOnUpdate(CharacterModelModifications __instance)
        {
            if (!((Network_Player)ReflectionInfos.characterModelPlayerNetworkField.GetValue(__instance)).IsLocalPlayer) return;

            __instance.SetFullBodyMeshState(VRConfigs.VisibleBody);
            __instance.SetArmMeshState(!VRConfigs.VisibleBody);
        }

        [HarmonyPatch(typeof(CharacterModelModifications), "Start")]
        [HarmonyPostfix]
        static void CheckVisibleBodyConfigOnStart(CharacterModelModifications __instance)
        {
            if (!((Network_Player)ReflectionInfos.characterModelPlayerNetworkField.GetValue(__instance)).IsLocalPlayer) return;

            __instance.SetFullBodyMeshState(VRConfigs.VisibleBody);
            __instance.SetArmMeshState(!VRConfigs.VisibleBody);
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
