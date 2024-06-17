using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class MoneyPile : ObjectPile
    {
        [SerializeField] private int maxPile = 120;
        [SerializeField, Range(1, 8)] private int collectMultiplier = 2;

        private int hiddenMoney;
        private bool isCollectingMoney;
        private int collectRate => objects.Count > 8 ? collectMultiplier : 1;

        protected override void Start()
        {
            // Leave as blank intentionally
            // We don't want to override the spacing.y (stack height) for money
        }

        protected override void Drop()
        {
            if (!isCollectingMoney) return;

            var moneyObj = PoolManager.Instance.SpawnObject("Money");
            moneyObj.transform.position = objects.Peek().transform.position;

            moneyObj.transform.DOJump(player.transform.position + Vector3.up * 2, 3f, 1, 0.5f)
                .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));

            AudioManager.Instance.PlaySFX(AudioID.Money);
        }

        protected override void OnPlayerEnter()
        {
            StartCoroutine(CollectMoney());
        }

        IEnumerator CollectMoney()
        {
            isCollectingMoney = true;

            RestaurantManager.Instance.AdjustMoney(hiddenMoney);
            hiddenMoney = 0;

            while (player != null && objects.Count > 0)
            {
                for (int i = 0; i < collectRate; i++)
                {
                    if (objects.Count == 0)
                    {
                        isCollectingMoney = false;
                        break;
                    }

                    var removedMoney = objects.Pop();
                    PoolManager.Instance.ReturnObject(removedMoney);
                    RestaurantManager.Instance.AdjustMoney(1);
                }

                if (collectRate > 1) yield return null;
                else yield return new WaitForSeconds(0.03f);
            }

            isCollectingMoney = false;
        }

        public void AddMoney()
        {
            if (objects.Count < maxPile)
            {
                var moneyObj = PoolManager.Instance.SpawnObject("Money");
                AddObject(moneyObj);
            }
            else
            {
                hiddenMoney++;
            }

            if (!isCollectingMoney && player != null)
                StartCoroutine(CollectMoney());
        }
    }
}
