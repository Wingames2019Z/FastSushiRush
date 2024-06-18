using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.FastFoodRush
{
    public class UpgradeHandler : MonoBehaviour
    {
        [SerializeField] private Upgrade upgradeType;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private Image[] indicators;
        
        private Color activeColor = new Color(1f, 0.5f, 0.25f, 1f);

        void Start()
        {
            upgradeButton.onClick.AddListener(() =>
                RestaurantManager.Instance.PurchaseUpgrade(upgradeType)
            );

            RestaurantManager.Instance.OnUpgrade += UpdateHandler;
        }

        void OnEnable()
        {
            UpdateHandler();
        }

        void UpdateHandler()
        {
            int level = RestaurantManager.Instance.GetUpgradeLevel(upgradeType);

            for (int i = 0; i < indicators.Length; i++)
            {
                indicators[i].color = i < level ? activeColor : Color.gray;
            }

            if (level < 5)
            {
                int price = RestaurantManager.Instance.GetUpgradePrice(upgradeType);
                priceLabel.text = RestaurantManager.Instance.GetFormattedMoney(price);

                bool hasEnoughMoney = RestaurantManager.Instance.GetMoney() >= price;
                upgradeButton.interactable = hasEnoughMoney;
            }
            else
            {
                priceLabel.text = "MAX";
                upgradeButton.interactable = false;
            }
        }
    }
}
