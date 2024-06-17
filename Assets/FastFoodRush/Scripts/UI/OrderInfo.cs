using UnityEngine;
using TMPro;

namespace CryingSnow.FastFoodRush
{
    public class OrderInfo : MonoBehaviour
    {
        [SerializeField] private GameObject iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Vector3 displayOffset = new Vector3(0f, 2.5f, 0f);

        Transform displayer;
        Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
            HideInfo();
        }

        void LateUpdate()
        {
            if (displayer == null) return;

            var displayPosition = displayer.position + displayOffset;
            transform.position = mainCamera.WorldToScreenPoint(displayPosition);
        }

        public void ShowInfo(Transform displayer, int amount)
        {
            gameObject.SetActive(true);
            this.displayer = displayer;

            bool active = amount > 0;
            iconImage.SetActive(active);
            amountText.text = active ? amount.ToString() : "NO SEAT!";
        }

        public void HideInfo()
        {
            gameObject.SetActive(false);
            displayer = null;
        }
    }
}
