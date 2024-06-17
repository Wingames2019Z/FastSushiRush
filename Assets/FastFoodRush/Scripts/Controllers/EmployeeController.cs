using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EmployeeController : MonoBehaviour
    {
        [SerializeField] private float baseSpeed = 2.5f;
        [SerializeField] private int baseCapacity = 3;

        [SerializeField] private WobblingStack stack;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        private Animator animator;
        private NavMeshAgent agent;

        private float IK_Weight;
        private int capacity;
        private StackType currentActivity;

        void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            RestaurantManager.Instance.OnUpgrade += UpdateStats;
            UpdateStats();
        }

        void Update()
        {
            animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);

            HandleActivity();
        }

        void UpdateStats()
        {
            float speedLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.EmployeeSpeed);
            agent.speed = baseSpeed + (speedLevel * 0.1f);

            int capacityLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.EmployeeCapacity);
            capacity = baseCapacity + capacityLevel;
        }

        void HandleActivity()
        {
            if (currentActivity != StackType.None) return;

            switch (Random.Range(0, 3))
            {
                case 0:
                    StartCoroutine(CleanTable());
                    break;
                case 1:
                    StartCoroutine(RefillFood());
                    break;
                case 2:
                    StartCoroutine(RefillPackage());
                    break;
            }
        }

        IEnumerator CleanTable()
        {
            currentActivity = StackType.Trash;

            var validTrashPiles = RestaurantManager.Instance.TrashPiles.Where(x => x.Count > 0).ToList();

            if (validTrashPiles.Count == 0)
            {
                currentActivity = StackType.None;
                yield break;
            }

            var trashPile = validTrashPiles[Random.Range(0, validTrashPiles.Count)];

            agent.SetDestination(trashPile.transform.position);

            while (!HasArrived())
            {
                if (trashPile.Count == 0)
                {
                    agent.SetDestination(transform.position);
                    currentActivity = StackType.None;
                    yield break;
                }

                yield return null;
            }

            while (trashPile.Count > 0 && stack.Height < capacity)
            {
                trashPile.RemoveAndStackObject(stack);

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            var trashBin = RestaurantManager.Instance.TrashBin;
            agent.SetDestination(trashBin.transform.position);

            yield return new WaitUntil(() => HasArrived());

            while (stack.Count > 0)
            {
                trashBin.ThrowToBin(stack);

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            currentActivity = StackType.None;
        }

        IEnumerator RefillFood()
        {
            currentActivity = StackType.Food;

            var validFoodStacks = RestaurantManager.Instance.FoodStacks.Where(x => x.gameObject.activeInHierarchy && !x.IsFull).ToList();
            if (validFoodStacks.Count == 0)
            {
                currentActivity = StackType.None;
                yield break;
            }
            var foodStack = validFoodStacks[Random.Range(0, validFoodStacks.Count)];

            var validFoodPiles = RestaurantManager.Instance.FoodPiles.Where(x => x.Count > 0).ToList();
            if (validFoodPiles.Count == 0)
            {
                currentActivity = StackType.None;
                yield break;
            }
            var foodPile = validFoodPiles[Random.Range(0, validFoodPiles.Count)];

            agent.SetDestination(foodPile.transform.position);

            while (!HasArrived())
            {
                if (foodPile.Count == 0)
                {
                    agent.SetDestination(transform.position);
                    currentActivity = StackType.None;
                    yield break;
                }

                yield return null;
            }

            while (foodPile.Count > 0 && stack.Height < capacity)
            {
                foodPile.RemoveAndStackObject(stack);

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            agent.SetDestination(foodStack.transform.position);

            yield return new WaitUntil(() => HasArrived());

            while (stack.Count > 0)
            {
                if (!foodStack.IsFull)
                {
                    var food = stack.RemoveFromStack();
                    foodStack.AddToStack(food.gameObject);
                }

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            currentActivity = StackType.None;
        }

        IEnumerator RefillPackage()
        {
            currentActivity = StackType.Package;

            var packageStack = RestaurantManager.Instance.PackageStack;
            if (packageStack == null || !packageStack.gameObject.activeInHierarchy || packageStack.IsFull)
            {
                currentActivity = StackType.None;
                yield break;
            }

            var packagePile = RestaurantManager.Instance.PackagePile;
            if (packagePile == null || !packagePile.gameObject.activeInHierarchy || packagePile.Count == 0)
            {
                currentActivity = StackType.None;
                yield break;
            }

            agent.SetDestination(packagePile.transform.position);

            while (!HasArrived())
            {
                if (packagePile.Count == 0)
                {
                    agent.SetDestination(transform.position);
                    currentActivity = StackType.None;
                    yield break;
                }

                yield return null;
            }

            while (packagePile.Count > 0 && stack.Height < capacity)
            {
                packagePile.RemoveAndStackObject(stack);

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            agent.SetDestination(packageStack.transform.position);

            yield return new WaitUntil(() => HasArrived());

            while (stack.Count > 0)
            {
                if (!packageStack.IsFull)
                {
                    var food = stack.RemoveFromStack();
                    packageStack.AddToStack(food.gameObject);
                }

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.5f);

            currentActivity = StackType.None;
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

        public void OnStep() { }

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
