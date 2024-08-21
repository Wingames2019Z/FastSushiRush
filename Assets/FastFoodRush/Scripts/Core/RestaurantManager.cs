using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class RestaurantManager : MonoBehaviour
    {
        public static RestaurantManager Instance { get; private set; }

        [SerializeField] private int baseUpgradePrice = 250;
        [SerializeField, Range(1.01f, 1.99f)] private float upgradeGrowthFactor = 1.5f;
        [SerializeField] private int baseUnlockPrice = 75;
        [SerializeField, Range(1.01f, 1.99f)] private float unlockGrowthFactor = 1.1f;
        [SerializeField] private long startingMoney = 1000;

        [Header("Stack Offset")]
        [SerializeField] private float foodOffset = 0.35f;
        [SerializeField] private float trashOffset = 0.18f;
        [SerializeField] private float packageOffset = 0.3f;

        [Header("Employee")]
        [SerializeField] private Transform employeePoint;
        [SerializeField] private EmployeeController employeePrefab;
        [SerializeField] private float employeeSpawnRadius = 3f;

        [Header("User Interface")]
        [SerializeField] private TMP_Text moneyDisplay;
        [SerializeField] private OrderInfo foodOrderInfo;
        [SerializeField] private OrderInfo packageOrderInfo;
        [SerializeField] private ScreenFader screenFader;

        [Header("Effects")]
        [SerializeField] private ParticleSystem unlockParticle;
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Unlockable")]
        [SerializeField] private UnlockableBuyer unlockableBuyer;
        [SerializeField] private List<Unlockable> unlockables;

        #region Reference Properties
        public OrderInfo FoodOrderInfo => foodOrderInfo;
        public OrderInfo PackageOrderInfo => packageOrderInfo;

        public List<ObjectPile> TrashPiles { get; private set; } = new List<ObjectPile>();
        public TrashBin TrashBin { get; private set; }

        public List<ObjectPile> FoodPiles { get; private set; } = new List<ObjectPile>();
        public List<ObjectStack> FoodStacks { get; private set; } = new List<ObjectStack>();

        public ObjectPile PackagePile { get; private set; }
        public ObjectStack PackageStack { get; private set; }
        #endregion

        public event System.Action OnUpgrade;
        public event System.Action<float> OnUnlock;

        public int PaidAmount
        {
            get => data.PaidAmount;
            set => data.PaidAmount = value;
        }

        private int unlockCount
        {
            get => data.UnlockCount;
            set => data.UnlockCount = value;
        }

        private RestaurantData data;

        private string restaurantID;

        void Awake()
        {
            Instance = this;

            restaurantID = SceneManager.GetActiveScene().name;

            data = SaveSystem.LoadData<RestaurantData>(restaurantID);

            if (data == null) data = new RestaurantData(restaurantID, startingMoney);

            AdjustMoney(0);

            for (int i = 0; i < data.EmployeeAmount; i++) SpawnEmployee();
        }

        void Start()
        {
            Debug.Log(Application.persistentDataPath);
            SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

            screenFader.FadeOut();

            var objectPiles = FindObjectsOfType<ObjectPile>(true);

            foreach (var pile in objectPiles)
            {
                if (pile.StackType == StackType.Trash) TrashPiles.Add(pile);
                else if (pile.StackType == StackType.Food) FoodPiles.Add(pile);
                else if (pile.StackType == StackType.Package) PackagePile = pile;
            }

            TrashBin = FindObjectOfType<TrashBin>(true);

            var objectStacks = FindObjectsOfType<ObjectStack>(true);

            foreach (var stack in objectStacks)
            {
                if (stack.StackType == StackType.Food) FoodStacks.Add(stack);
                else if (stack.StackType == StackType.Package) PackageStack = stack;
            }

            // Init unlocked Unlockables
            for (int i = 0; i < unlockCount; i++)
            {
                unlockables[i].Unlock(false);
            }

            UpdateUnlockableBuyer();

            AudioManager.Instance.PlayBGM(backgroundMusic);
        }

        void SpawnEmployee()
        {
            var randomCircle = Random.insideUnitCircle * employeeSpawnRadius;
            var randomPos = employeePoint.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            Instantiate(employeePrefab, randomPos, employeePoint.rotation);
        }

        void Update()
        {
            // DEBUG
            if (SimpleInput.GetButtonDown("DebugMoney"))
            {
                AdjustMoney(10000);
                SaveSystem.SaveData<RestaurantData>(data, restaurantID);
            }
        }

        public float GetStackOffset(StackType stackType) => stackType switch
        {
            StackType.Food => foodOffset,
            StackType.Trash => trashOffset,
            StackType.Package => packageOffset,
            StackType.None => 0f,
            _ => 0f
        };

        public void BuyUnlockable()
        {
            unlockables[unlockCount].Unlock();

            // Effects
            unlockParticle.transform.position = unlockables[unlockCount].transform.position;
            unlockParticle.Play();
            AudioManager.Instance.PlaySFX(AudioID.Magical);

            unlockCount++;
            PaidAmount = 0;
            UpdateUnlockableBuyer();

            SaveSystem.SaveData<RestaurantData>(data, restaurantID);
        }

        void UpdateUnlockableBuyer()
        {
            if (unlockables?.Count(unlockable => unlockable != null) == 0)
            {
                Debug.LogWarning("There are no unlockables present in the scene! Please add the necessary unlockable items to proceed.");
                return;
            }

            if (unlockCount < unlockables.Count)
            {
                var unlockable = unlockables[unlockCount];
                unlockableBuyer.transform.position = unlockable.GetBuyingPoint();

                int price = Mathf.RoundToInt(Mathf.Round(baseUnlockPrice * Mathf.Pow(unlockGrowthFactor, unlockCount)) / 5f) * 5;

                unlockableBuyer.Initialize(unlockable, price, PaidAmount);
            }
            else
            {
                data.IsUnlocked = true;

                unlockableBuyer.gameObject.SetActive(false);
            }

            float progress = data.UnlockCount / (float)unlockables.Count;
            OnUnlock?.Invoke(progress);
        }

        public void AdjustMoney(int change)
        {
            data.Money += change;
            moneyDisplay.text = GetFormattedMoney(data.Money);
        }

        public long GetMoney()
        {
            return data.Money;
        }

        public string GetFormattedMoney(long money)
        {
            if (money < 1000) return money.ToString();
            else if (money < 1000000) return (money / 1000f).ToString("0.##") + "k";
            else if (money < 1000000000) return (money / 1000000f).ToString("0.##") + "m";
            else if (money < 1000000000000) return (money / 1000000000f).ToString("0.##") + "b";
            else return (money / 1000000000000f).ToString("0.##") + "t";
        }

        public void PurchaseUpgrade(Upgrade upgrade)
        {
            int price = GetUpgradePrice(upgrade);
            AdjustMoney(-price);

            switch (upgrade)
            {
                case Upgrade.EmployeeSpeed:
                    data.EmployeeSpeed++;
                    break;

                case Upgrade.EmployeeCapacity:
                    data.EmployeeCapacity++;
                    break;

                case Upgrade.EmployeeAmount:
                    data.EmployeeAmount++;
                    SpawnEmployee();
                    break;

                case Upgrade.PlayerSpeed:
                    data.PlayerSpeed++;
                    break;

                case Upgrade.PlayerCapacity:
                    data.PlayerCapacity++;
                    break;

                case Upgrade.Profit:
                    data.Profit++;
                    break;

                default:
                    break;
            }

            AudioManager.Instance.PlaySFX(AudioID.Kaching);

            SaveSystem.SaveData<RestaurantData>(data, restaurantID);

            OnUpgrade?.Invoke();
        }

        public int GetUpgradePrice(Upgrade upgrade)
        {
            int currentLevel = GetUpgradeLevel(upgrade);
            return Mathf.RoundToInt(Mathf.Round(baseUpgradePrice * Mathf.Pow(upgradeGrowthFactor, currentLevel)) / 50f) * 50;
        }

        public int GetUpgradeLevel(Upgrade upgrade) => upgrade switch
        {
            Upgrade.EmployeeSpeed => data.EmployeeSpeed,
            Upgrade.EmployeeCapacity => data.EmployeeCapacity,
            Upgrade.EmployeeAmount => data.EmployeeAmount,
            Upgrade.PlayerSpeed => data.PlayerSpeed,
            Upgrade.PlayerCapacity => data.PlayerCapacity,
            Upgrade.Profit => data.Profit,
            _ => 0
        };

        public void LoadRestaurant(int index)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (index == currentSceneIndex) return;

            screenFader.FadeIn(() =>
            {
                SaveSystem.SaveData<RestaurantData>(data, restaurantID);

                SceneManager.LoadScene(index);
            });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            SaveSystem.SaveData<RestaurantData>(data, restaurantID);
        }

        void OnDisable()
        {
            DOTween.KillAll();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (employeePoint == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(employeePoint.position, employeeSpawnRadius);
        }
#endif
    }

    public enum Upgrade
    {
        EmployeeSpeed, EmployeeCapacity, EmployeeAmount,
        PlayerSpeed, PlayerCapacity, Profit
    }
}
