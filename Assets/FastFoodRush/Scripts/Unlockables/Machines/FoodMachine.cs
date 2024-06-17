using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class FoodMachine : Unlockable
    {
        [SerializeField] private float baseInterval = 1.5f;
        [SerializeField] private int baseCapacity = 6;
        [SerializeField] private ObjectPile foodPile;

        private float productionInterval;
        private int productionCapacity;
        private float productionTimer;

        protected override void UpdateStats()
        {
            productionInterval = baseInterval / unlockLevel;
            productionCapacity = baseCapacity * unlockLevel;
        }

        void Update()
        {
            if (foodPile.Count >= productionCapacity) return;

            productionTimer += Time.deltaTime;

            if (productionTimer >= productionInterval)
            {
                productionTimer = 0f;

                var food = PoolManager.Instance.SpawnObject("Food");
                foodPile.AddObject(food);
            }
        }
    }
}
