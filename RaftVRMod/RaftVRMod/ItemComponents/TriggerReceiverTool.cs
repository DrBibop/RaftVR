using UnityEngine;

namespace RaftVR.ItemComponents
{
    class TriggerReceiverTool : MonoBehaviour
    {
        protected virtual void Start()
        {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            // Required for collision detection to work
            gameObject.layer = LayerMask.NameToLayer("Projectiles");
        }

        internal virtual void OnEnter(Collider collider) { }

        internal virtual void OnExit(Collider collider) { }
    }
}
