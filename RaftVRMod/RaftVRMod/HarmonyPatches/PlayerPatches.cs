using HarmonyLib;
using RaftVR.ItemComponents;
using RaftVR.Rig;
using RaftVR.Configs;
using RaftVR.UI;
using RaftVR.Utils;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.PostProcessing;
using RaftVR.Inputs;

namespace RaftVR.HarmonyPatches
{
    [HarmonyPatch]
    static class PlayerPatches
    {
        [HarmonyPatch(typeof(Network_Player), "Start")]
        [HarmonyPostfix]
        static void SetupPlayer(Network_Player __instance)
        {
            if (!__instance.IsLocalPlayer) return;

            //I'd like to have separate methods for these, but Harmony doesn't like that, so regions will do.
            #region Setup held objects
            List<Transform> objectsToTurn = new List<Transform>()
            {
                __instance.rightHandParent.Find("PaintBrush"),
                __instance.rightHandParent.Find("Machete"),
                __instance.rightHandParent.Find("Hammer"),
                __instance.rightHandParent.Find("Sword_Titanium"),
                __instance.rightHandParent.Find("Axe_Stone"),
                __instance.rightHandParent.Find("Axe_Scrap"),
                __instance.rightHandParent.Find("Axe_Titanium")
            };

            foreach (Transform child in __instance.rightHandParent)
            {
                if (child.gameObject.name.Contains("Throwable, "))
                {
                    objectsToTurn.Add(child);

                    Throwable throwable = child.gameObject.GetComponent<Throwable>();

                    if (throwable)
                    {
                        ReflectionInfos.throwableRotationField.SetValue(throwable, (Vector3)ReflectionInfos.throwableRotationField.GetValue(throwable) + new Vector3(10, 0, 0));
                    }
                }
            }

            foreach (Transform objectToTurn in objectsToTurn)
            {
                if (objectToTurn == null) continue;

                objectToTurn.localEulerAngles += new Vector3(objectToTurn.gameObject.name.Contains("Throwable, ") ? 10 : -10, 0, 0);
            }

            MeleeWeapon[] meleeWeapons = __instance.rightHandParent.GetComponentsInChildren<MeleeWeapon>(true);

            for (int i = 0; i < meleeWeapons.Length; i++)
            {
                MeleeWeapon meleeWeapon = meleeWeapons[i];

                if (meleeWeapon is Machete)
                {
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

            Transform woodenSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Wood").transform;
            Transform scrapSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Scrap").transform;
            Transform devSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Dev").transform;

            woodenSpear.localPosition = scrapSpear.localPosition = devSpear.localPosition = new Vector3(0.555f, 0.036f, 0.003f);
            woodenSpear.localRotation = scrapSpear.localRotation = devSpear.localRotation = Quaternion.Euler(0.061f, 179.886f, 86.84f);

            NoteBookUI noteBookUI = __instance.rightHandParent.GetComponentInChildren<NoteBookUI>(true);

            if (noteBookUI)
            {
                Canvas noteBookCanvas = noteBookUI.GetComponentInChildren<Canvas>(true);

                if (noteBookCanvas)
                {
                    noteBookCanvas.gameObject.layer = LayerMask.NameToLayer("UI");

                    BoxCollider pagesCollider = noteBookCanvas.gameObject.AddComponent<BoxCollider>();
                    pagesCollider.size = new Vector3(525, 425, 0);

                    BoxCollider backCollider = noteBookCanvas.gameObject.AddComponent<BoxCollider>();
                    backCollider.size = new Vector3(800, 425, 0);
                    backCollider.center = new Vector3(-160, 0, 20);

                    noteBookCanvas.gameObject.AddComponent<UICollider>().Init(noteBookCanvas);
                    noteBookCanvas.gameObject.AddComponent<WorldToUI>();
                }

                Transform noteBook = noteBookUI.transform;

                if (!VRConfigs.IsLeftHanded)
                {
                    noteBook.parent.SetParent(__instance.leftHandParent);
                    noteBook.parent.localPosition = Vector3.zero;
                    noteBook.parent.localRotation = Quaternion.identity;

                    noteBook.localPosition = new Vector3(-0.154614f, 0.3439946f, 0.4126883f);
                    noteBook.localRotation = Quaternion.Euler(50.229f, 28.461f, 109.054f);
                }
            }

            ConsumeComponent[] foods = __instance.rightHandParent.GetComponentsInChildren<ConsumeComponent>(true); 

            for (int i = 0; i < foods.Length; i++)
            {
                foods[i].gameObject.AddComponent<ConsumableVR>();
            }

            Transform paddle = __instance.rightHandParent.GetComponentInChildren<Paddle>(true).transform;

            paddle.localPosition = new Vector3(0.594f, 0.025f, -0.006f);
            paddle.localRotation = Quaternion.Euler(170f, 0f, -90f);
            paddle.localScale = Vector3.one * 1.2f;

            paddle.gameObject.AddComponent<WaterPointGetter>();
            paddle.gameObject.AddComponent<PaddleVR>();
            paddle.gameObject.AddComponent<TwoHandedToolVR>();

            Transform sweepNet = __instance.rightHandParent.GetComponentInChildren<SweepNet>(true).transform;

            sweepNet.localRotation = Quaternion.Euler(-80f, -1.974f, 90.23801f);

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

            #endregion

            #region Setup camera rig
            Camera camera = __instance.Camera;

            Camera uiCamera = __instance.HandCamera;
            uiCamera.gameObject.name = "UI Camera";
            GameObject.Destroy(uiCamera.GetComponent<PostProcessingBehaviour>());

            GameObject rigTarget = new GameObject("Rig Target");
            rigTarget.transform.SetParent(__instance.transform);
            rigTarget.transform.localRotation = Quaternion.identity;
            rigTarget.transform.localPosition = new Vector3(0, -__instance.GetComponent<CharacterController>().height / 2, 0);

            VRRig.instance.camera = camera;
            VRRig.instance.uiCamera = uiCamera;
            VRRig.instance.player = __instance.transform;
            VRRig.instance.positionTarget = rigTarget.transform;

            VRRig.instance.InitHUD();
            #endregion

            #region Setup animation clips
            Animator anim = __instance.Animator.anim;

            AnimatorOverrideController fpController = anim.runtimeAnimatorController as AnimatorOverrideController;

            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(fpController.overridesCount);
            fpController.GetOverrides(overrides);

            List<AnimationClip> vrOverrideClips = VRAssetsManager.vrClips.ToList();

            List<KeyValuePair<AnimationClip, AnimationClip>> vrOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            foreach (var overridePair in overrides)
            {
                if (overridePair.Value == null)
                {
                    vrOverrides.Add(overridePair);
                    continue;
                }

                try
                {
                    AnimationClip vrOverrideClip = vrOverrideClips.First(x => x.name == overridePair.Value.name);

                    if (vrOverrideClip)
                        vrOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(overridePair.Key, vrOverrideClip));
                    else
                        vrOverrides.Add(overridePair);
                }
                catch
                {
                    vrOverrides.Add(overridePair);
                }
            }

            fpController.ApplyOverrides(vrOverrides);

            //This little part is to remove the hook throwing delay.
            List<AnimationConnection> connections = (List<AnimationConnection>)ReflectionInfos.animationConnectionsField.GetValue(__instance.Animator.GetComponent<AnimationEventCaller>());
            connections.RemoveAll((connection) => connection.name.Contains("Animation_HookThrow"));

            #endregion

            #region Setup VRIK
            VRIK vrik = __instance.Animator.gameObject.AddComponent<VRIK>();

            vrik.fixTransforms = false;
            vrik.solver.locomotion.stepSpeed = 10f;
            vrik.solver.locomotion.rootSpeed = 30f;

            VRIK.References references = new VRIK.References();
            Transform root = __instance.currentModel.transform;

            Transform[] bones = new Transform[]
            {
                root,
                root.Find("Skeleton/ORG-hips"),
                root.Find("Skeleton/ORG-hips/ORG-spine"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-neck"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-neck/ORG-head"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_L"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_L/ORG-upper_arm_L"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_L/ORG-upper_arm_L/ORG-forearm_L"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_L/ORG-upper_arm_L/ORG-forearm_L/ORG-hand_L"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_R"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_R/ORG-upper_arm_R"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_R/ORG-upper_arm_R/ORG-forearm_R"),
                root.Find("Skeleton/ORG-hips/ORG-spine/ORG-chest/ORG-shoulder_R/ORG-upper_arm_R/ORG-forearm_R/ORG-hand_R"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_L"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_L/ORG-shin_L"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_L/ORG-shin_L/ORG-foot_L"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_L/ORG-shin_L/ORG-foot_L/ORG-toe_L"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_R"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_R/ORG-shin_R"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_R/ORG-shin_R/ORG-foot_R"),
                root.Find("Skeleton/ORG-hips/ORG-thigh_R/ORG-shin_R/ORG-foot_R/ORG-toe_R")
            };

            references.root = bones[0];
            references.pelvis = bones[1];
            references.spine = bones[2];
            references.chest = bones[3];
            references.neck = bones[4];
            references.head = bones[5];
            references.leftShoulder = bones[6];
            references.leftUpperArm = bones[7];
            references.leftForearm = bones[8];
            references.leftHand = bones[9];
            references.rightShoulder = bones[10];
            references.rightUpperArm = bones[11];
            references.rightForearm = bones[12];
            references.rightHand = bones[13];
            references.leftThigh = bones[14];
            references.leftCalf = bones[15];
            references.leftFoot = bones[16];
            references.leftToes = bones[17];
            references.rightThigh = bones[18];
            references.rightCalf = bones[19];
            references.rightFoot = bones[20];
            references.rightToes = bones[21];

            vrik.solver.SetToReferences(references);

            vrik.solver.leftArm.target = VRRig.instance.LeftHandIKTarget;
            vrik.solver.leftArm.armLengthMlp = VRConfigs.ArmScale;

            vrik.solver.rightArm.target = VRRig.instance.RightHandIKTarget;
            vrik.solver.rightArm.armLengthMlp = VRConfigs.ArmScale;

            vrik.solver.spine.headTarget = VRRig.instance.HeadIKTarget;

            vrik.solver.locomotion.footDistance = 0.2f;
            vrik.solver.leftLeg.swivelOffset = -45;
            vrik.solver.rightLeg.swivelOffset = 45;

            vrik.solver.plantFeet = false;

            Keyframe[] stretchKeyframes = new Keyframe[]
            {
                new Keyframe(0, 0, 0, 0),
                new Keyframe(1, 0, 0, 1),
                new Keyframe(2, 1, 1, 0)
            };
            stretchKeyframes[1].weightedMode = WeightedMode.None;
            stretchKeyframes[2].weightedMode = WeightedMode.None;

            AnimationCurve armStretchCurve = new AnimationCurve(stretchKeyframes);

            vrik.solver.leftArm.stretchCurve = armStretchCurve;
            vrik.solver.rightArm.stretchCurve = armStretchCurve;

            TwistRelaxer twistRelaxer = __instance.Animator.gameObject.AddComponent<TwistRelaxer>();
            twistRelaxer.ik = vrik;

            TwistSolver leftArmSolver = new TwistSolver();
            leftArmSolver.weight = 0.5f;
            leftArmSolver.transform = references.leftForearm;
            leftArmSolver.parent = references.leftUpperArm;
            leftArmSolver.children = new Transform[] { references.leftHand };

            TwistSolver rightArmSolver = new TwistSolver();
            rightArmSolver.weight = 0.5f;
            rightArmSolver.transform = references.rightForearm;
            rightArmSolver.parent = references.rightUpperArm;
            rightArmSolver.children = new Transform[] { references.rightHand };

            twistRelaxer.twistSolvers = new TwistSolver[] { leftArmSolver, rightArmSolver };

            VRConfigs.localPlayerAnimator = __instance.Animator;
            #endregion
        }

        [HarmonyPatch(typeof(PersonController), "GroundControll")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveGroundNormalize(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int codeIndex = codes.FindIndex((x) => x.Calls(AccessTools.Method(typeof(Vector3), "Normalize")));

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
                __instance.playerNetwork.BlockCreator.IsRotating)
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
            }
        }
    }
}
