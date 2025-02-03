using UnityEngine;

namespace Rover656.Survivors.Client {
    public class DestroyParentBehaviour : MonoBehaviour {
        public void DestroyParent() {
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}