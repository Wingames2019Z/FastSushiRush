using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class MovingObject : MonoBehaviour
    {
        private Vector3 originalPosition;

        void Start()
        {
            originalPosition = transform.position;
            Move();
        }

        void Move()
        {
            transform.position = originalPosition;

            transform.DOLocalMoveZ(0.7f, 0.5f)
                .SetEase(Ease.Linear)
                .OnComplete(() => Move());
        }
    }
}
