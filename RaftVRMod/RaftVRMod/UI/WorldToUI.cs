using RaftVR.Rig;
using UnityEngine;

namespace RaftVR.UI
{
    class WorldToUI : MonoBehaviour
    {
        Transform worldRef;

        private void Awake()
        {
            worldRef = new GameObject(gameObject.name + " (Ref)").transform;
            worldRef.SetParent(transform.parent);
            worldRef.localPosition = transform.localPosition;
            worldRef.localRotation = transform.localRotation;

            transform.SetParent(null);
        }

        private void LateUpdate()
        {
            if (!worldRef)
                Destroy(gameObject);

            if (gameObject.activeSelf != worldRef.gameObject.activeInHierarchy)
                gameObject.SetActive(worldRef.gameObject.activeInHierarchy);

            transform.position = VRRig.instance.transform.InverseTransformPoint(worldRef.position);
            transform.rotation = Quaternion.Inverse(VRRig.instance.transform.rotation) * worldRef.rotation;
        }
    }
}
