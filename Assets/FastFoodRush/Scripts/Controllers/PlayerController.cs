using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float baseSpeed = 3.0f;
        [SerializeField] private float rotateSpeed = 360f;
        [SerializeField] private int baseCapacity = 5;

        [SerializeField] private AudioClip[] footsteps;

        [SerializeField] private WobblingStack stack;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        public WobblingStack Stack => stack;
        public int Capacity { get; private set; }

        private Animator animator;
        private CharacterController controller;
        private AudioSource audioSource;

        private float moveSpeed;
        private Vector3 movement;
        private Vector3 velocity;
        private bool isGrounded;

        private float IK_Weight;

        const float gravityValue = -9.81f;

        void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            RestaurantManager.Instance.OnUpgrade += UpdateStats;
            UpdateStats();
        }

        void Update()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0f;
            }

            movement.x = SimpleInput.GetAxis("Horizontal");
            movement.z = SimpleInput.GetAxis("Vertical");
            movement = (Quaternion.Euler(0, 45, 0) * movement).normalized;

            controller.Move(movement * Time.deltaTime * moveSpeed);

            if (movement != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
            }

            velocity.y += gravityValue * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            animator.SetBool("IsMoving", movement != Vector3.zero);
        }

        void UpdateStats()
        {
            int speedLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.PlayerSpeed);
            moveSpeed = baseSpeed + (speedLevel * 0.2f);

            int capacityLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.PlayerCapacity);
            Capacity = baseCapacity + (capacityLevel * 3);
        }

        public void OnStep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight < 0.5f) return;

            audioSource.clip = footsteps[Random.Range(0, footsteps.Length)];
            audioSource.Play();
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
