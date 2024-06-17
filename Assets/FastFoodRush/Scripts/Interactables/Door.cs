using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class Door : Interactable
    {
        [SerializeField] private Transform doorTransform;
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private float closeDuration = 0.5f;

        Vector3 openAngle = new Vector3(0f, 90f, 0f);

        protected override void OnPlayerEnter()
        {
            OpenDoor(player.transform);
        }

        protected override void OnPlayerExit()
        {
            CloseDoor();
        }

        public void OpenDoor(Transform interactor)
        {
            Vector3 direction = (interactor.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(direction, transform.forward);
            Vector3 targetAngle = openAngle * Mathf.Sign(dotProduct);

            doorTransform.DOLocalRotate(targetAngle, openDuration, RotateMode.LocalAxisAdd);
        }

        public void CloseDoor()
        {
            doorTransform.DOLocalRotate(Vector3.zero, closeDuration).SetEase(Ease.OutBounce);
        }
    }
}
