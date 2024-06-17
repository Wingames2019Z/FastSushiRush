using UnityEngine;
using UnityEngine.UI;

namespace CryingSnow.FastFoodRush
{
    public class WorkingSpot : Interactable
    {
        [SerializeField] private Image indicatorImage;

        public bool HasWorker => player != null;

        protected override void OnPlayerEnter()
        {
            indicatorImage.color = Color.green;
        }

        protected override void OnPlayerExit()
        {
            indicatorImage.color = Color.red;
        }
    }
}
