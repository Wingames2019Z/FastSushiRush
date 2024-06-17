using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private Vector3 offset;

        void Start()
        {
            if (target == null) target = FindObjectOfType<PlayerController>().transform;

            offset = transform.position - target.position;
        }

        void LateUpdate()
        {
            transform.position = offset + target.position;
        }
    }
}
