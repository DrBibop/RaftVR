using RaftVR.Configs;
using RaftVR.Utils;
using RootMotion.FinalIK;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaftVR.Rig
{
    class Network_VRIK : MonoBehaviour_Network
    {
        internal const Messages MSG_HEAD = (Messages)(-130);
        internal const Messages MSG_LEFT_HAND = (Messages)(-131);
        internal const Messages MSG_RIGHT_HAND = (Messages)(-132);

        private const int SEND_RATE = 15;

        private float sendDelay;

        private bool initialized = false;

        private Network_Player playerNetwork;

        private Transform headIK;
        private Transform leftHandIK;
        private Transform rightHandIK;

        private Transform headTarget;
        private Transform leftHandTarget;
        private Transform rightHandTarget;

        private VRIK vrik;

        private static Dictionary<Network_Player, Network_VRIK> playerDictionary = new Dictionary<Network_Player, Network_VRIK>();

        private Vector3 origBowPos;
        private Quaternion origBowRot;

        internal static bool DeserializeOnPlayer(Network_Player player, Message_NetworkBehaviour msg, CSteamID remoteID)
        {
            if (playerDictionary.TryGetValue(player, out Network_VRIK vrik))
            {
                return vrik.Deserialize(msg, remoteID);
            }

            return false;
        }

        private void Start()
        {
            NetworkIDManager.AddNetworkID(this);

            playerNetwork = GetComponent<Network_Player>();

            if (playerNetwork)
            {
                if (playerNetwork.IsLocalPlayer)
                {
                    headIK = VRRig.instance.HeadIKTarget;
                    leftHandIK = VRRig.instance.LeftHandIKTarget;
                    rightHandIK = VRRig.instance.RightHandIKTarget;

                    Initialize(VRConfigs.ArmScale, VRConfigs.LegScale, VRConfigs.IsLeftHanded, true);
                }
                else
                {
                    playerDictionary.Add(playerNetwork, this);

                    headIK = new GameObject("Head IK").transform;
                    headIK.parent = transform;
                    headIK.ResetTransform();

                    leftHandIK = new GameObject("Left Hand IK").transform;
                    leftHandIK.parent = transform;
                    leftHandIK.ResetTransform();

                    rightHandIK = new GameObject("Right Hand IK").transform;
                    rightHandIK.parent = transform;
                    rightHandIK.ResetTransform();

                    headTarget = new GameObject("Head Target").transform;
                    headTarget.parent = transform;
                    headTarget.ResetTransform();

                    leftHandTarget = new GameObject("Left Hand Target").transform;
                    leftHandTarget.parent = transform;
                    leftHandTarget.ResetTransform();

                    rightHandTarget = new GameObject("Right Hand Target").transform;
                    rightHandTarget.parent = transform;
                    rightHandTarget.ResetTransform();
                }
            }
        }

        protected override void OnDestroy()
        {
            NetworkIDManager.RemoveNetworkID(this);
            base.OnDestroy();
        }

        private void LateUpdate()
        {
            if (!initialized) return;

            if (vrik.enabled == (playerNetwork.PlayerScript.IsDead || playerNetwork.BedComponent.Sleeping))
                vrik.enabled = !playerNetwork.PlayerScript.IsDead && !playerNetwork.BedComponent.Sleeping;

            if (playerNetwork.IsLocalPlayer)
            {
                if (sendDelay >= 0)
                {
                    sendDelay -= Time.unscaledDeltaTime;
                    return;
                }
                sendDelay = 1f / SEND_RATE;

                Message_AxeHit headMessage = new Message_AxeHit(MSG_HEAD, playerNetwork, playerNetwork.steamID);
                headMessage.HitPoint = playerNetwork.transform.InverseTransformPoint(headIK.position);
                headMessage.HitNormal = (Quaternion.Inverse(playerNetwork.transform.rotation) * headIK.rotation).eulerAngles;
                headMessage.treeObjectIndex = (int)(Mathf.Round(VRConfigs.ArmScale * 100f) * 1000f) + Mathf.RoundToInt(VRConfigs.LegScale * 100f);
                if (VRConfigs.IsLeftHanded)
                    headMessage.treeObjectIndex += 1000000;

                Message_AxeHit leftMessage = new Message_AxeHit(MSG_LEFT_HAND, playerNetwork, playerNetwork.steamID);
                leftMessage.HitPoint = playerNetwork.transform.InverseTransformPoint(leftHandIK.position);
                leftMessage.HitNormal = (Quaternion.Inverse(playerNetwork.transform.rotation) * leftHandIK.rotation).eulerAngles;

                Message_AxeHit rightMessage = new Message_AxeHit(MSG_RIGHT_HAND, playerNetwork, playerNetwork.steamID);
                rightMessage.HitPoint = playerNetwork.transform.InverseTransformPoint(rightHandIK.position);
                rightMessage.HitNormal = (Quaternion.Inverse(playerNetwork.transform.rotation) * rightHandIK.rotation).eulerAngles;

                List<Message> messages = new List<Message>() { headMessage, leftMessage, rightMessage };

                if (Raft_Network.IsHost)
                {
                    playerNetwork.Network.RPC(messages, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                }
                else
                {
                    playerNetwork.Network.SendP2P(playerNetwork.Network.HostID, messages, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                }
            }
            else
            {
                Vector3 vel = Vector3.zero;
                Quaternion der = Quaternion.identity;

                headIK.position = Vector3.SmoothDamp(headIK.position, headTarget.position, ref vel, 0.04f, float.MaxValue, Time.unscaledDeltaTime);
                headIK.rotation = headIK.rotation.SmoothDamp(headTarget.rotation, ref der, 0.05f);

                leftHandIK.position = Vector3.SmoothDamp(leftHandIK.position, leftHandTarget.position, ref vel, 0.04f, float.MaxValue, Time.unscaledDeltaTime);
                leftHandIK.rotation = leftHandIK.rotation.SmoothDamp(leftHandTarget.rotation, ref der, 0.05f);

                rightHandIK.position = Vector3.SmoothDamp(rightHandIK.position, rightHandTarget.position, ref vel, 0.04f, float.MaxValue, Time.unscaledDeltaTime);
                rightHandIK.rotation = rightHandIK.rotation.SmoothDamp(rightHandTarget.rotation, ref der, 0.05f);
            }
        }

        public override bool Deserialize(Message_NetworkBehaviour msg, CSteamID remoteID)
        {
            if (!(msg is Message_AxeHit message)) return false;

            Vector3 localPos = message.HitPoint;
            Quaternion localRot = Quaternion.Euler(message.HitNormal);

            switch (msg.Type)
            {
                case MSG_HEAD:
                    float armScale = Mathf.Round(message.treeObjectIndex / 1000f) / 100f;
                    float legScale = (message.treeObjectIndex / 100f) % 10;
                    if (!initialized)
                    {
                        bool leftHanded = (message.treeObjectIndex / 1000000) == 1;
                        Initialize(armScale, legScale, leftHanded);
                    }
                    else
                    {
                        if (vrik.solver.leftArm.armLengthMlp != armScale)
                        {
                            vrik.solver.leftArm.armLengthMlp = armScale;
                            vrik.solver.rightArm.armLengthMlp = armScale;
                        }

                        if (vrik.solver.leftLeg.legLengthMlp != legScale)
                        {
                            vrik.solver.leftLeg.legLengthMlp = legScale;
                            vrik.solver.rightLeg.legLengthMlp = legScale;
                        }
                    }
                    headTarget.localPosition = localPos;
                    headTarget.localRotation = localRot;
                    return false;
                case MSG_LEFT_HAND:
                    leftHandTarget.localPosition = localPos;
                    leftHandTarget.localRotation = localRot;
                    return false;
                case MSG_RIGHT_HAND:
                    rightHandTarget.localPosition = localPos;
                    rightHandTarget.localRotation = localRot;
                    return false;
                default:
                    return base.Deserialize(msg, remoteID);
            }
        }

        private void Initialize(float armScale, float legScale, bool leftHanded, bool local = false)
        {
            SetupVRIK(armScale, legScale, local);
            SetupAnimationController();
            SetupObjectTransforms(leftHanded);

            initialized = true;
        }

        private void SetupVRIK(float armScale, float legScale, bool local)
        {
            VRIK vrik = playerNetwork.Animator.gameObject.AddComponent<VRIK>();

            vrik.fixTransforms = false;

            VRIK.References references = new VRIK.References();
            Transform root = playerNetwork.currentModel.transform;

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

            if (local)
            {
                VRRig.instance.LeftHandPlayerBone = bones[9];
                VRRig.instance.RightHandPlayerBone = bones[13];
            }

            vrik.solver.SetToReferences(references);
            ReflectionInfos.ikSolverAnimatorSetMethod.Invoke(vrik.solver, new object[] { playerNetwork.Animator.anim });

            vrik.solver.leftArm.target = leftHandIK;
            vrik.solver.leftArm.armLengthMlp = armScale;

            vrik.solver.rightArm.target = rightHandIK;
            vrik.solver.rightArm.armLengthMlp = armScale;

            vrik.solver.spine.headTarget = headIK;

            vrik.solver.locomotion.weight = 1f;
            vrik.solver.locomotion.mode = IKSolverVR.Locomotion.Mode.Animated;
            vrik.solver.locomotion.moveThreshold = 0.3f;
            vrik.solver.locomotion.maxRootOffset = 0.4f;
            vrik.solver.locomotion.standOffset = new Vector2(0f, -0.2f);

            vrik.solver.leftLeg.swivelOffset = -45;
            vrik.solver.leftLeg.legLengthMlp = legScale;
            vrik.solver.rightLeg.swivelOffset = 45;
            vrik.solver.rightLeg.legLengthMlp = legScale;
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

            TwistRelaxer twistRelaxer = playerNetwork.Animator.gameObject.AddComponent<TwistRelaxer>();
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

            this.vrik = vrik;
        }

        private void SetupAnimationController()
        {
            Animator anim = playerNetwork.Animator.anim;

            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            AnimatorOverrideController fpController = playerNetwork.currentModel.firstPersonController as AnimatorOverrideController;

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

                if (overridePair.Key.name.StartsWith("Crouch") || overridePair.Key.name.StartsWith("Jump") || overridePair.Key.name.StartsWith("Run") || overridePair.Key.name.StartsWith("Strafe") || overridePair.Key.name.StartsWith("Swimming"))
                {
                    vrOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(overridePair.Key, null));
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

            AnimatorOverrideController newController = new AnimatorOverrideController(playerNetwork.currentModel.thirdPersonController);
            newController.ApplyOverrides(vrOverrides);
            anim.runtimeAnimatorController = newController;
        }

        private void SetupObjectTransforms(bool leftHanded)
        {
            List<Transform> objectsToTurn = new List<Transform>()
            {
                playerNetwork.rightHandParent.Find("PaintBrush"),
                playerNetwork.rightHandParent.Find("Machete"),
                playerNetwork.rightHandParent.Find("Hammer"),
                playerNetwork.rightHandParent.Find("Sword_Titanium"),
                playerNetwork.rightHandParent.Find("Axe_Stone"),
                playerNetwork.rightHandParent.Find("Axe_Scrap"),
                playerNetwork.rightHandParent.Find("Axe_Titanium")
            };

            foreach (Transform child in playerNetwork.rightHandParent)
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

            MeleeWeapon[] meleeWeapons = playerNetwork.rightHandParent.GetComponentsInChildren<MeleeWeapon>(true);

            Transform woodenSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Wood").transform;
            Transform scrapSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Scrap").transform;
            Transform devSpear = meleeWeapons.First(x => x.gameObject.name == "Spear_Dev").transform;

            woodenSpear.localPosition = scrapSpear.localPosition = devSpear.localPosition = new Vector3(0.555f, 0.036f, 0.003f);
            woodenSpear.localRotation = scrapSpear.localRotation = devSpear.localRotation = Quaternion.Euler(0.061f, 179.886f, 86.84f);

            NoteBookUI noteBookUI = playerNetwork.rightHandParent.GetComponentInChildren<NoteBookUI>(true);

            if (noteBookUI)
            {
                Transform noteBook = noteBookUI.transform;

                if (!leftHanded)
                {
                    noteBook.parent.SetParent(playerNetwork.leftHandParent);
                    noteBook.parent.localPosition = Vector3.zero;
                    noteBook.parent.localRotation = Quaternion.identity;

                    noteBook.localPosition = new Vector3(-0.154614f, 0.3439946f, 0.4126883f);
                    noteBook.localRotation = Quaternion.Euler(50.229f, 28.461f, 109.054f);
                }
            }

            Transform paddle = playerNetwork.rightHandParent.GetComponentInChildren<Paddle>(true).transform;

            paddle.localPosition = new Vector3(0.594f, 0.025f, -0.006f);
            paddle.localRotation = Quaternion.Euler(170f, 0f, -90f);
            paddle.localScale = Vector3.one * 1.2f;

            Transform sweepNet = playerNetwork.rightHandParent.GetComponentInChildren<SweepNet>(true).transform;

            sweepNet.localRotation = Quaternion.Euler(-80f, -1.974f, 90.23801f);

            ThrowableComponent_Bow bow = playerNetwork.rightHandParent.GetComponentInChildren<ThrowableComponent_Bow>(true);

            if (bow)
            {
                if (!leftHanded)
                {
                    origBowPos = bow.transform.localPosition;
                    origBowRot = bow.transform.localRotation;

                    bow.transform.SetParent(playerNetwork.leftHandParent);
                    bow.transform.localPosition = new Vector3(-0.1444f, 0.0318f, -0.0419f);
                    bow.transform.localEulerAngles = new Vector3(-59.188f, 179.967f, -90.68101f);
                }
            }
        }
    }
}
