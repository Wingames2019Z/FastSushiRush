using UnityEngine;
using TMPro;

namespace CryingSnow.FastFoodRush
{
    public class ProgressDisplay : MonoBehaviour
    {
        [SerializeField] private Gradient progressGradient;

        private SlicedFilledImage progressFill;
        private TMP_Text progressText;

        void Awake()
        {
            progressFill = GetComponentInChildren<SlicedFilledImage>();
            progressText = GetComponentInChildren<TMP_Text>();
        }

        void OnEnable()
        {
            RestaurantManager.Instance.OnUnlock += UpdateProgress;
        }

        void OnDisable()
        {
            RestaurantManager.Instance.OnUnlock -= UpdateProgress;
        }

        void UpdateProgress(float progress)
        {
            progressFill.color = progressGradient.Evaluate(progress);
            progressFill.fillAmount = progress;
            progressText.text = $"PROGRESS {progress * 100:0.##}%";
        }
    }
}
