using UnityEngine;

namespace RaftVR
{
    internal static class VRAssetsManager
    {
        private static AssetBundle vrassets;

        internal static AnimationClip[] vrClips;
        internal static Material lineMaterial;
        internal static GameObject handCanvasPrefab;
        internal static GameObject playspaceCenterIndicatorPrefab;
        internal static GameObject calibrateCanvasPrefab;
        internal static GameObject interactionRayPrefab;

        internal static void Init(AssetBundle vrAssetsBundle)
        {
            vrassets = vrAssetsBundle;
            vrClips = vrAssetsBundle.LoadAllAssets<AnimationClip>();
            lineMaterial = vrAssetsBundle.LoadAsset<Material>("Line");
            handCanvasPrefab = vrAssetsBundle.LoadAsset<GameObject>("HandCanvas");
            playspaceCenterIndicatorPrefab = vrAssetsBundle.LoadAsset<GameObject>("PlayspaceCenterIndicator");
            calibrateCanvasPrefab = vrAssetsBundle.LoadAsset<GameObject>("CalibrateCanvas");
            interactionRayPrefab = vrAssetsBundle.LoadAsset<GameObject>("InteractionRay");
        }

        internal static void Unload()
        {
            vrassets.Unload(true);
        }
    }
}
