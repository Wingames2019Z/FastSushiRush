using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class TravelingObject : MonoBehaviour
    {
        [SerializeField] private Waypoints waypoints;

        private MeshRenderer meshRenderer;
        private Vector3 originalPosition;

        IEnumerator Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalPosition = transform.position;

            meshRenderer.enabled = false;

            float delay = transform.GetSiblingIndex();
            yield return new WaitForSeconds(delay);

            meshRenderer.enabled = true;

            Travel();
        }

        void Travel()
        {
            transform.DOPath(waypoints.GetPoints().ToArray(), 5f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    transform.position = originalPosition;
                    Travel();
                });

            transform.DOLocalRotate(new Vector3(360, 0, 0), 0.3f, RotateMode.LocalAxisAdd)
                .SetDelay(2.5f);
        }
    }
}
