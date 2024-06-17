using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class DriveThruCounter : Workstation
    {
        [SerializeField] private float baseInterval = 1.5f;
        [SerializeField] private int basePrice = 15;
        [SerializeField] private float priceIncrementRate = 1.25f;
        [SerializeField] private int baseStack = 30;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform despawnPoint;
        [SerializeField] private Waypoints queuePoints;
        [SerializeField] private CarController[] carPrefabs;
        [SerializeField] private ObjectStack packageStack;
        [SerializeField] private MoneyPile moneyPile;

        private Queue<CarController> cars = new Queue<CarController>();
        private CarController firstCar => cars.Peek();

        private float spawnInterval;
        private float serveInterval;
        private int sellPrice;
        private float spawnTimer;
        private float serveTimer;
        private bool isFinishingService;

        const int maxCars = 10;

        void Update()
        {
            HandleCarSpawn();
            HandlePackageServing();
        }

        protected override void UpdateStats()
        {
            spawnInterval = (baseInterval * 3) - unlockLevel;
            serveInterval = baseInterval / unlockLevel;

            packageStack.MaxStack = baseStack + unlockLevel * 5;

            int profitLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.Profit);
            sellPrice = Mathf.RoundToInt(Mathf.Pow(priceIncrementRate, profitLevel) * basePrice);
        }

        void HandleCarSpawn()
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval && cars.Count < maxCars)
            {
                spawnTimer = 0f;

                int rand = Random.Range(0, carPrefabs.Length);
                var newCar = Instantiate(carPrefabs[rand], spawnPoint.position, spawnPoint.rotation);
                cars.Enqueue(newCar);
                newCar.Init(queuePoints, despawnPoint, cars.Count - 1);
            }
        }

        void HandlePackageServing()
        {
            if (cars.Count == 0 || !firstCar.HasOrder) return;

            serveTimer += Time.deltaTime;

            if (serveTimer >= serveInterval)
            {
                serveTimer = 0f;

                if (hasWorker && packageStack.Count > 0 && firstCar.OrderCount > 0)
                {
                    var package = packageStack.RemoveFromStack();
                    firstCar.FillOrder(package);

                    CollectPayment();
                }

                if (firstCar.OrderCount == 0 && !isFinishingService)
                    StartCoroutine(FinishServing());
            }
        }

        void CollectPayment()
        {
            for (int i = 0; i < sellPrice; i++)
            {
                moneyPile.AddMoney();
            }
        }

        IEnumerator FinishServing()
        {
            isFinishingService = true;

            yield return new WaitForSeconds(0.5f);

            var servedCar = cars.Dequeue();
            servedCar.Leave();

            foreach (var car in cars)
            {
                car.UpdateQueue();
            }

            serveTimer = 0f;

            isFinishingService = false;
        }
    }
}
