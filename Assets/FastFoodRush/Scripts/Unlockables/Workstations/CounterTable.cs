using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CounterTable : Workstation
    {
        [SerializeField] private float baseInterval = 1.5f;
        [SerializeField] private int basePrice = 5;
        [SerializeField] private float priceIncrementRate = 1.25f;
        [SerializeField] private int baseStack = 30;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform despawnPoint;
        [SerializeField] private Waypoints queuePoints;
        [SerializeField] private CustomerController customerPrefab;
        [SerializeField] private ObjectStack foodStack;
        [SerializeField] private MoneyPile moneyPile;

        private Queue<CustomerController> customers = new Queue<CustomerController>();
        private CustomerController firstCustomer => customers.Peek();
        private List<Seating> seatings;

        private float spawnInterval;
        private float serveInterval;
        private int sellPrice;
        private float spawnTimer;
        private float serveTimer;

        const int maxCustomers = 10;

        void Start()
        {
            seatings = FindObjectsOfType<Seating>(true).ToList();
        }

        void Update()
        {
            HandleCustomerSpawn();
            HandleFoodServing();
        }

        protected override void UpdateStats()
        {
            spawnInterval = (baseInterval * 3) - unlockLevel;
            serveInterval = baseInterval / unlockLevel;

            foodStack.MaxStack = baseStack + unlockLevel * 5;

            int profitLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.Profit);
            sellPrice = Mathf.RoundToInt(Mathf.Pow(priceIncrementRate, profitLevel) * basePrice);
        }

        void HandleCustomerSpawn()
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval && customers.Count < maxCustomers)
            {
                spawnTimer = 0f;

                var newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
                newCustomer.ExitPoint = despawnPoint.position;

                customers.Enqueue(newCustomer);

                AssignQueuePoint(newCustomer, customers.Count - 1);
            }
        }

        void AssignQueuePoint(CustomerController customer, int index)
        {
            Transform queuePoint = queuePoints.GetPoint(index);
            bool isFirst = index == 0;

            customer.UpdateQueue(queuePoint, isFirst);
        }

        void HandleFoodServing()
        {
            if (customers.Count == 0 || !firstCustomer.HasOrder) return;

            serveTimer += Time.deltaTime;

            if (serveTimer >= serveInterval)
            {
                serveTimer = 0f;

                if (hasWorker && foodStack.Count > 0 && firstCustomer.OrderCount > 0)
                {
                    var food = foodStack.RemoveFromStack();
                    firstCustomer.FillOrder(food);
                    CollectPayment();
                }

                if (firstCustomer.OrderCount == 0 && CheckAvailableSeating(out Transform seat))
                {
                    var servedCustomer = customers.Dequeue();
                    servedCustomer.AssignSeat(seat);
                    UpdateQueuePositions();
                }
            }
        }

        void CollectPayment()
        {
            for (int i = 0; i < sellPrice; i++)
            {
                moneyPile.AddMoney();
            }
        }

        private bool CheckAvailableSeating(out Transform seat)
        {
            var semiFullSeating = seatings.Where(seating => seating.gameObject.activeInHierarchy && seating.IsSemiFull).FirstOrDefault();
            if (semiFullSeating != null)
            {
                seat = semiFullSeating.Occupy(firstCustomer);
                return true;
            }

            var emptySeatings = seatings.Where(seating => seating.gameObject.activeInHierarchy && seating.IsEmpty).ToList();
            if (emptySeatings.Count > 0)
            {
                int randomIndex = Random.Range(0, emptySeatings.Count);
                seat = emptySeatings[randomIndex].Occupy(firstCustomer);
                return true;
            }

            seat = null;
            return false;
        }

        void UpdateQueuePositions()
        {
            int index = 0;
            foreach (var customer in customers)
            {
                AssignQueuePoint(customer, index);
                index++;
            }
        }
    }
}
