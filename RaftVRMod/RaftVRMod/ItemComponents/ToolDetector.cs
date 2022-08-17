using UnityEngine;

namespace RaftVR.ItemComponents
{
    class ToolDetector : MonoBehaviour
    {
        private BoxCollider triggerCollider;
        private int triggerLayer;

        private void Start()
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerLayer = LayerMask.NameToLayer("Projectiles");
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer == triggerLayer)
            {
                TriggerReceiverTool receiver = collider.gameObject.GetComponent<TriggerReceiverTool>();

                if (receiver)
                    receiver.OnEnter(triggerCollider);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer == triggerLayer)
            {
                TriggerReceiverTool receiver = collider.gameObject.GetComponent<TriggerReceiverTool>();

                if (receiver)
                    receiver.OnExit(triggerCollider);
            }
        }
    }
}
