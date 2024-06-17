using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CryingSnow.FastFoodRush
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private List<Button> mapButtons;
        [SerializeField] private Sprite lockedSprite;

        private List<Sprite> mapButtonSprites = new List<Sprite>();

        void Awake()
        {
            foreach (var mapButton in mapButtons)
            {
                mapButtonSprites.Add(mapButton.image.sprite);
            }
        }

        void Start()
        {
            for (int i = 0; i < mapButtons.Count; i++)
            {
                int index = i + 1;

                mapButtons[i].onClick.AddListener(() =>
                {
                    gameObject.SetActive(false);
                    RestaurantManager.Instance.LoadRestaurant(index);
                });
            }
        }

        void OnEnable()
        {
            for (int i = 0; i < mapButtons.Count; i++)
            {
                int index = i + 1;
                bool isUnlocked = IsUnlocked(index);

                mapButtons[i].interactable = isUnlocked;
                mapButtons[i].image.sprite = isUnlocked ? mapButtonSprites[index - 1] : lockedSprite;
            }
        }

        private bool IsUnlocked(int index)
        {
            if (index == 1) return true;

            var previousRestaurantData = SaveSystem.LoadData<RestaurantData>($"Restaurant0{index - 1}");

            if (previousRestaurantData == null) return false;

            if (previousRestaurantData.IsUnlocked) return true;

            return false;
        }
    }
}
