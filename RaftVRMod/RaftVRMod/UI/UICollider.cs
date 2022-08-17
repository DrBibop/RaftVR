using RaftVR.Rig;
using UnityEngine;

namespace RaftVR.UI
{
    class UICollider : MonoBehaviour
    {
        private Canvas canvas;
        private BoxCollider[] colliders;

        public void Init(Canvas canvas)
        {
            this.canvas = canvas;
            colliders = GetComponents<BoxCollider>();
        }

        private void Update()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].enabled != canvas.isActiveAndEnabled)
                {
                    colliders[i].enabled = canvas.isActiveAndEnabled;
                }
            }
        }
    }
}
