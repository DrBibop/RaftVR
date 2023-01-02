using RaftVR.Configs;
using RaftVR.Rig;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaftVR.UI
{
    public class UIHelper
    {
        private static Canvas settingsCanvas;

        private static List<Canvas> permanentCanvases = new List<Canvas>();

        private static Dictionary<Transform, Vector3> scalableUIElement = new Dictionary<Transform, Vector3>();

        private static bool initializedItemSpawner;

        /// <summary>
        /// Send canvases that were set to not destroy on scene load here. This will make sure the worldCamera field is set properly when changing scenes.
        /// Otherwise, the canvas will probably not be interactable with the pointer.
        /// </summary>
        /// <param name="canvas">The canvas to set as permanent.</param>
        public static void AddPermanentCanvas(Canvas canvas)
        {
            if (!permanentCanvases.Contains(canvas))
            {
                permanentCanvases.Add(canvas);
                canvas.worldCamera = VRRig.instance.uiCamera;
            }
        }

        /// <summary>
        /// Send UI elements to be scaled from the UI scale setting here. When the scale setting is changed, it will multiply with the base scale.
        /// </summary>
        /// <param name="uiElement">The UI element to scale.</param>
        /// <param name="baseScale">The base scale of the UI element.</param>
        public static void AddScalableUIElement(Transform uiElement, Vector3 baseScale)
        {
            if (!scalableUIElement.ContainsKey(uiElement))
            {
                scalableUIElement.Add(uiElement, baseScale);
            }
        }

        public static void UpdateUIScale()
        {
            foreach (Transform key in scalableUIElement.Keys.ToArray().Where((key) => key == null))
            {
                scalableUIElement.Remove(key);
            }

            foreach (Transform key in scalableUIElement.Keys)
            {
                if (scalableUIElement.TryGetValue(key, out Vector3 baseScale))
                    key.localScale = baseScale * VRConfigs.UIScale;
            }

            if (settingsCanvas != null && settingsCanvas.transform.parent != null)
                settingsCanvas.transform.localPosition = new Vector3(0, 0.2f, 0.1f) * VRConfigs.UIScale;

            if (VRRig.instance && VRRig.instance.nonDominantCanvasHolder != null)
                VRRig.instance.nonDominantCanvasHolder.localPosition = new Vector3(0, 0.2f, 0.1f) * VRConfigs.UIScale;
        }

        internal static void TryInitItemSpawnerCanvas()
        {
            if (initializedItemSpawner) return;

            GameObject itemSpawner = GameObject.Find("ItemSpawnerCanvas(Clone)");

            if (itemSpawner)
            {
                Canvas itemSpawnerCanvas = itemSpawner.GetComponentInChildren<Canvas>();

                VRRig.instance.AddWorldCanvas(itemSpawnerCanvas, Quaternion.identity);
                AddPermanentCanvas(itemSpawnerCanvas);
                initializedItemSpawner = true;
            }
        }

        internal static void InitVRMenu()
        {
            GameObject cameraTargetLocation = new GameObject("Menu Camera Position");
            cameraTargetLocation.transform.position = new Vector3(-4.79f, 0f, 0.61f);

            Camera camera = Camera.main;

            Camera uiCamera = new GameObject("UI Camera").AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.nearClipPlane = 0.05f;
            uiCamera.depth = 2;

            VRRig.instance.camera = camera;
            VRRig.instance.uiCamera = uiCamera;
            VRRig.instance.positionTarget = cameraTargetLocation.transform;
            VRRig.instance.transform.rotation = Quaternion.Euler(new Vector3(0, 60, 0));
            VRRig.instance.AddWorldCanvas(Object.FindObjectOfType<StartMenuScreen>().GetComponent<Canvas>(), Quaternion.identity);
        }

        internal static void InitSettingsCanvas()
        {
            settingsCanvas = ComponentManager<Settings>.Value.GetComponentInChildren<Canvas>(true);
            MoveSettingsCanvasToFront();
            AddPermanentCanvas(settingsCanvas);
        }

        internal static void InitLoadingScreen()
        {
            Canvas loadingScreen = ComponentManager<Raft_Network>.Value.gameObject.GetComponentInChildren<Canvas>(true);

            VRRig.instance.AddWorldCanvas(loadingScreen, Quaternion.identity);
            AddPermanentCanvas(loadingScreen);
        }

        internal static void MoveSettingsCanvasToFront()
        {
            scalableUIElement.Remove(settingsCanvas.transform);

            VRRig.instance.AddWorldCanvas(settingsCanvas, Quaternion.identity);
        }

        internal static void MoveSettingsCanvasToHand()
        {
            scalableUIElement.Remove(settingsCanvas.transform);

            settingsCanvas.transform.SetParent(VRRig.instance.NonDominantController.UIHand);
            settingsCanvas.transform.localPosition = new Vector3(0, 0.2f, 0.1f) * VRConfigs.UIScale;
            settingsCanvas.transform.localRotation = Quaternion.Euler(15, -20, 0);
            settingsCanvas.transform.localScale = Vector3.one * 0.0003f * VRConfigs.UIScale;

            VRRig.instance.worldCanvases.Remove(settingsCanvas);
            AddScalableUIElement(settingsCanvas.transform, Vector3.one * 0.0003f);
        }

        internal static void RefreshPermanentCanvases(Camera newUICamera)
        {
            foreach (Canvas canvas in permanentCanvases)
            {
                canvas.worldCamera = newUICamera;
            }
        }
    }
}
