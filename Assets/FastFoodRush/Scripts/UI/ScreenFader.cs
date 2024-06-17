using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Image))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 1f;

        private Image image;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        public void FadeIn(System.Action onComplete = null)
        {
            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = true;

            image.DOFade(1f, fadeTime).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void FadeOut(System.Action onComplete = null)
        {
            image.color = Color.black;
            image.raycastTarget = false;

            image.DOFade(0f, fadeTime).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
    }
}
