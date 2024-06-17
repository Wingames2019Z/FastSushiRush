using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Interactable : MonoBehaviour
    {
        protected PlayerController player { get; private set; }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = other.GetComponent<PlayerController>();
                if (player != null) OnPlayerEnter();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerExit();
                player = null;
            }
        }

        protected virtual void OnPlayerEnter() { }
        protected virtual void OnPlayerExit() { }
    }
}
