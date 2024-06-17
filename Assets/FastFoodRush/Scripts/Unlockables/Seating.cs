using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Seating : Unlockable
    {
        [SerializeField] private float baseEatTime = 5f;
        [SerializeField] private float baseTipChance = 0.4f;
        [SerializeField] private List<Transform> seats;
        [SerializeField] private Transform tableTop;
        [SerializeField] private ObjectPile trashPile;
        [SerializeField] private MoneyPile tipMoneyPile;

        public bool IsSemiFull => trashPile.Count == 0 && customers.Count > 0 && customers.Count < seats.Count;
        public bool IsEmpty => trashPile.Count == 0 && customers.Count == 0;

        private List<CustomerController> customers = new List<CustomerController>();
        private Stack<Transform> foods = new Stack<Transform>();

        private float foodStackOffset;
        private float eatTime;
        private float tipChance;
        private int tipLevel;

        void Start()
        {
            foodStackOffset = RestaurantManager.Instance.GetStackOffset(StackType.Food);
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            eatTime = (baseEatTime - (unlockLevel - 1)) * seats.Count;
            tipChance = baseTipChance + ((unlockLevel - 1) * 0.1f);
            tipLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.Profit);
        }

        public Transform Occupy(CustomerController customer)
        {
            customers.Add(customer);

            if (customers.Count >= seats.Count)
                StartCoroutine(BeginEating());

            return seats[customers.Count - 1];
        }

        public void AddFoodOnTable(Transform food)
        {
            foods.Push(food);
            Vector3 heightOffset = new Vector3(0f, (foods.Count - 1) * foodStackOffset, 0f);
            food.transform.position = tableTop.position + heightOffset;
            food.transform.rotation = Quaternion.identity;
        }

        void LeaveTip()
        {
            if (Random.value < tipChance)
            {
                int tipAmount = Random.Range(2, 5 + tipLevel);
                for (int i = 0; i < tipAmount; i++)
                {
                    tipMoneyPile.AddMoney();
                }
            }
        }

        IEnumerator BeginEating()
        {
            yield return new WaitUntil(() => customers.All(customer => customer.ReadyToEat));

            foreach (var customer in customers)
            {
                customer.TriggerEat();
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

            float interval = eatTime / foods.Count;
            int trashCount = 0;
            while (foods.Count > 0)
            {
                yield return new WaitForSeconds(interval);

                PoolManager.Instance.ReturnObject(foods.Pop().gameObject);
                trashCount++;
                LeaveTip();
            }

            while (trashCount > 0)
            {
                var trash = PoolManager.Instance.SpawnObject("Trash");
                trashPile.AddObject(trash);
                trashCount--;

                yield return new WaitForSeconds(0.05f);
            }

            foreach (var customer in customers)
            {
                customer.FinishEating();

                yield return new WaitForSeconds(Random.Range(1, 4) * 0.5f);
            }

            customers.Clear();
        }
    }
}
