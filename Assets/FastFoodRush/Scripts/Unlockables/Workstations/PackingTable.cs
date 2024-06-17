using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class PackingTable : Workstation
    {
        [SerializeField] private float baseInterval = 1.5f;
        [SerializeField] private int baseStack = 30;
        [SerializeField] private Transform packageBox;
        [SerializeField] private ObjectStack foodStack;
        [SerializeField] private ObjectPile packagePile;

        private float packingInterval;
        private int packingCapacity;
        private float packingTimer;
        private int packingProgress;

        const int maxFoodInPackage = 4;

        protected override void UpdateStats()
        {
            packingInterval = baseInterval / unlockLevel;

            int maxStack = baseStack + unlockLevel * 5;
            packingCapacity = foodStack.MaxStack = maxStack;
        }

        void Update()
        {
            HandlePacking();
        }

        void HandlePacking()
        {
            packingTimer += Time.deltaTime;

            if (packingTimer >= packingInterval && packagePile.Count < packingCapacity)
            {
                packingTimer = 0f;

                if (hasWorker && foodStack.Count > 0)
                {
                    Transform food = foodStack.RemoveFromStack();

                    food.DOJump(packageBox.GetChild(packingProgress).position, 5f, 1, 0.3f)
                        .OnComplete(() =>
                        {
                            packingProgress++;

                            PoolManager.Instance.ReturnObject(food.gameObject);

                            packageBox.GetChild(packingProgress - 1).gameObject.SetActive(true);

                            if (packingProgress == maxFoodInPackage)
                            {
                                packingProgress = 0;

                                StartCoroutine(FinishPacking());
                            }
                        });
                }
            }
        }

        IEnumerator FinishPacking()
        {
            yield return new WaitForSeconds(0.1f);

            packageBox.gameObject.SetActive(false);

            foreach (Transform child in packageBox)
            {
                child.gameObject.SetActive(false);
            }

            var package = PoolManager.Instance.SpawnObject("Package");
            package.transform.position = packageBox.position;
            yield return package.transform.DOJump(packagePile.PeakPoint, 5f, 1, 0.5f).WaitForCompletion();

            packagePile.AddObject(package);

            packageBox.gameObject.SetActive(true);
        }
    }
}
