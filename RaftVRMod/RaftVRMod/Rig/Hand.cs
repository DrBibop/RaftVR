using RaftVR.Configs;
using RaftVR.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace RaftVR.Rig
{
    public class Hand : MonoBehaviour
    {
        public Ray Ray => new Ray(transform.position, transform.forward);

        public float MotionMagnitude { get; private set; }

        public Vector3 MotionForward => motionDirection * Vector3.forward;

        public Vector3 MotionRight => motionDirection * Vector3.right;

        public Vector3 MotionUp => motionDirection * Vector3.up;

        public Transform UIHand => uiHand.transform;

        public Transform UIWorldHand
        {
            get
            {
                if (uiWorldHand == null)
                {
                    uiWorldHand = new GameObject((node == XRNode.RightHand ? "Right" : "Left") + " World UI");

                    if (LoadSceneManager.IsGameSceneLoaded)
                        SceneManager.MoveGameObjectToScene(uiWorldHand, SceneManager.GetSceneByName(Raft_Network.GameSceneName));
                }

                return uiWorldHand.transform;
            }
        }

        private Quaternion motionDirection;

        private XRNode node;

        private Vector3[] lastPositions = new Vector3[3];

        private GameObject uiHand;

        private GameObject uiWorldHand;

        internal void Init(XRNode node)
        {
            this.node = node;

            uiHand = new GameObject((node == XRNode.RightHand ? "Right" : "Left") + " UI");
            DontDestroyOnLoad(uiHand);
        }

        internal void UpdateHand()
        {
            InputDevice hand = InputDevices.GetDeviceAtXRNode(node);

            if (hand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                //Oculus and SteamVR provide different values. This adjustment keeps them relatively the same.
                if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR)
                {
                    rotation *= Quaternion.Euler(Vector3.right * 40);
                }

                transform.localRotation = rotation;

                Quaternion deriv = new Quaternion(); ;
                Quaternion smoothRotation = uiHand.transform.rotation.SmoothDamp(rotation, ref deriv, 0.03f);

                uiHand.transform.rotation = smoothRotation;

                if (uiWorldHand)
                {
                    uiWorldHand.transform.rotation = smoothRotation;
                }
            }

            if (hand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            {
                //Oculus and SteamVR provide different values. This adjustment keeps them relatively the same.
                if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR)
                {
                    position += (rotation * Vector3.down * 0.03f) + (rotation * Vector3.back * 0.05f);
                }

                transform.localPosition = position;
                uiHand.transform.position = position;

                if (uiWorldHand)
                {
                    uiWorldHand.transform.position = position;
                }
            }

            InputDevice device = InputDevices.GetDeviceAtXRNode(node);

            if (device == null) return;

            if (!device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 motion)) return;
            if (!device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 angular)) return;

            motionDirection = motion == Vector3.zero ? transform.parent.rotation : transform.parent.rotation * Quaternion.LookRotation(motion);

            MotionMagnitude = motion.magnitude + angular.magnitude / 10;
        }

        public Vector3 GetForwardFlat()
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            return forward.normalized;
        }
    }
}
