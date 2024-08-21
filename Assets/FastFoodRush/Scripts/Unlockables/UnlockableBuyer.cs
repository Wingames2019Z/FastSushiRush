using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class UnlockableBuyer : Interactable
    {
        [SerializeField] private float payingInterval = 0.03f;
        [SerializeField] private float payingTime = 3f;
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private ProgressDisplay progressDisplay;
        private long playerMoney => RestaurantManager.Instance.GetMoney();

        private Unlockable unlockable;
        private int unlockPrice;
        private int paidAmount;

        public void Initialize(Unlockable unlockable, int unlockPrice, int paidAmount)
        {
            this.unlockable = unlockable;
            this.unlockPrice = unlockPrice;
            this.paidAmount = paidAmount;

            UpdatePayment(0);
        }

        void UpdatePayment(int amount)
        {
            paidAmount += amount;
            RestaurantManager.Instance.PaidAmount = paidAmount;

            progressFill.fillAmount = (float)paidAmount / unlockPrice;
            progressDisplay.UpdateProgress(progressFill.fillAmount);
            priceLabel.text = RestaurantManager.Instance.GetFormattedMoney(unlockPrice - paidAmount);
        }

        protected override void OnPlayerEnter()
        {
            StartCoroutine(Pay());
        }

        IEnumerator Pay()
        {
            while (player != null && paidAmount < unlockPrice && playerMoney > 0)
            {
                float paymentRate = unlockPrice * payingInterval / payingTime;
                paymentRate = Mathf.Min(playerMoney, paymentRate);
                int payment = Mathf.Max(1, Mathf.RoundToInt(paymentRate));

                UpdatePayment(payment);
                RestaurantManager.Instance.AdjustMoney(-payment);

                PlayMoneyAnimation();

                if (paidAmount >= unlockPrice)
                    RestaurantManager.Instance.BuyUnlockable();

                yield return new WaitForSeconds(payingInterval);
            }
        }

        void PlayMoneyAnimation()
        {
            var moneyObj = PoolManager.Instance.SpawnObject("Money");
            moneyObj.transform.position = player.transform.position + Vector3.up * 2;
            moneyObj.transform.DOJump(transform.position, 3f, 1, 0.5f)
                .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));

            AudioManager.Instance.PlaySFX(AudioID.Money);
        }
    }
}
