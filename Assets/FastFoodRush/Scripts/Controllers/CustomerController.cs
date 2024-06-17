using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerController : MonoBehaviour
    {
        [SerializeField] private int maxOrder = 5;

        [SerializeField] private WobblingStack stack;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        public Vector3 ExitPoint { get; set; }
        public bool HasOrder { get; private set; }
        public int OrderCount { get; private set; }
        public bool ReadyToEat { get; private set; }

        private OrderInfo orderInfo => RestaurantManager.Instance.FoodOrderInfo;

        private Animator animator;
        private NavMeshAgent agent;

        private LayerMask entranceLayer;

        private float IK_Weight;

        void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();

            entranceLayer = 1 << LayerMask.NameToLayer("Entrance");
        }

        IEnumerator CheckEntrance()
        {
            RaycastHit hit;

            while (!Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, entranceLayer, QueryTriggerInteraction.Collide))
            {
                yield return null;
            }

            var doors = hit.transform.GetComponentsInChildren<Door>();
            foreach (var door in doors)
            {
                door.OpenDoor(transform);
            }

            yield return new WaitForSeconds(1f);

            foreach (var door in doors)
            {
                door.CloseDoor();
            }
        }

        void Start()
        {
            StartCoroutine(CheckEntrance());
        }

        void Update()
        {
            animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);
        }

        public void UpdateQueue(Transform queuePoint, bool isFirst)
        {
            agent.SetDestination(queuePoint.position);

            if (isFirst) StartCoroutine(PlaceOrder());
        }

        public void FillOrder(Transform food)
        {
            OrderCount--;
            stack.AddToStack(food, StackType.Food);

            orderInfo.ShowInfo(transform, OrderCount);
        }

        public void AssignSeat(Transform seat)
        {
            orderInfo.HideInfo();

            StartCoroutine(WalkToSeat(seat));
        }

        public void TriggerEat()
        {
            animator.SetTrigger("Eat");
        }

        public void FinishEating()
        {
            agent.SetDestination(ExitPoint);
            animator.SetTrigger("Leave");

            StartCoroutine(WalkToExit());
        }

        IEnumerator PlaceOrder()
        {
            yield return new WaitUntil(() => HasArrived());

            OrderCount = Random.Range(1, maxOrder + 1);
            HasOrder = true;

            orderInfo.ShowInfo(transform, OrderCount);
        }

        IEnumerator WalkToSeat(Transform seat)
        {
            yield return new WaitForSeconds(0.3f); // Wait for the last food to land on customer's tray

            agent.SetDestination(seat.position);

            yield return new WaitUntil(() => HasArrived());

            while (Vector3.Angle(transform.forward, seat.forward) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, seat.rotation, Time.deltaTime * 270f);
                yield return null;
            }

            var seating = seat.GetComponentInParent<Seating>();
            while (stack.Count > 0)
            {
                seating.AddFoodOnTable(stack.RemoveFromStack());
                yield return new WaitForSeconds(0.05f);
            }

            ReadyToEat = true;

            animator.SetTrigger("Sit");
        }

        IEnumerator WalkToExit()
        {
            StartCoroutine(CheckEntrance());

            yield return new WaitUntil(() => HasArrived());

            Destroy(gameObject);
        }

        private bool HasArrived()
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void OnAnimatorIK()
        {
            IK_Weight = Mathf.MoveTowards(IK_Weight, Mathf.Clamp01(stack.Height), Time.deltaTime * 3.5f);

            if (leftHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_Weight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IK_Weight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            if (rightHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_Weight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, IK_Weight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
        }
    }
}
