using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class CarController : MonoBehaviour
    {
        [SerializeField, Range(5f, 10f)] private float movingSpeed = 5.0f;
        [SerializeField, Range(15f, 45f)] private float turningSpeed = 15.0f;
        [SerializeField] private int maxOrder = 5;

        public bool HasOrder { get; private set; }
        public int OrderCount { get; private set; }

        private List<Vector3> waypoints;
        private Transform exitPoint;
        private int queueNumber;
        private int currentWaypointIndex;

        private OrderInfo orderInfo => RestaurantManager.Instance.PackageOrderInfo;

        public void Init(Waypoints waypoints, Transform exitPoint, int queueNumber)
        {
            this.waypoints = waypoints.GetPoints();
            this.exitPoint = exitPoint;
            this.queueNumber = queueNumber;
            currentWaypointIndex = this.waypoints.Count - 1;

            MoveToNextWayPoint();
        }

        public void UpdateQueue()
        {
            queueNumber--;
            currentWaypointIndex = queueNumber;
            MoveToNextWayPoint();
        }

        private void MoveToNextWayPoint()
        {
            Vector3 targetPoint = waypoints[currentWaypointIndex];

            System.Action onComplete = () =>
            {
                if (currentWaypointIndex > queueNumber)
                {
                    currentWaypointIndex--;
                    MoveToNextWayPoint();
                }
                else if (queueNumber == 0)
                {
                    PlaceOrder();
                }
            };

            Move(targetPoint, onComplete);
        }

        private void Move(Vector3 targetPoint, System.Action onComplete)
        {
            if (DOTween.IsTweening(transform)) return;

            float distance = Vector3.Distance(transform.position, targetPoint);
            Vector3 direction = targetPoint - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            float movingDuration = distance / movingSpeed;
            float turningDuration = distance / turningSpeed;

            var sequence = DOTween.Sequence();

            sequence.Append(transform.DOMove(targetPoint, movingDuration).SetEase(Ease.Linear));
            sequence.Join(transform.DORotateQuaternion(targetRotation, turningDuration).SetEase(Ease.Linear));
            sequence.OnComplete(() => onComplete?.Invoke());
        }

        private void PlaceOrder()
        {
            OrderCount = Random.Range(1, maxOrder);
            HasOrder = true;
            orderInfo.ShowInfo(transform, OrderCount);
        }

        public void FillOrder(Transform package)
        {
            package.DOJump(transform.position + Vector3.up * 3, 5f, 1, 0.5f)
                .OnComplete(() => PoolManager.Instance.ReturnObject(package.gameObject));

            OrderCount--;

            if (OrderCount == 0) orderInfo.HideInfo();
            else orderInfo.ShowInfo(transform, OrderCount);
        }

        public void Leave()
        {
            Move(exitPoint.position, () => Destroy(gameObject));
        }
    }
}
