using UnityEngine;

namespace DefaultNamespace
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 3f, -6f);
        
        private void LateUpdate()
        {
            transform.position = target.position + offset;
            transform.rotation = target.rotation;
        }
    }

}