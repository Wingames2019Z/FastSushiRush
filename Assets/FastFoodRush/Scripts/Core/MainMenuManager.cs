using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private AudioClip backgroundMusic;

        private int targetScene;

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            
            int lastSceneIndex = SaveSystem.LoadData<int>("LastSceneIndex");
            targetScene = lastSceneIndex == 0 ? 1 : lastSceneIndex;

            startButton.onClick.AddListener(StartGame);

            startButton.transform.DOScale(Vector3.one * 1.1f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo);

            AudioManager.Instance.PlayBGM(backgroundMusic);
        }

        void StartGame()
        {
            DOTween.Kill(startButton.transform);
            screenFader.FadeIn(() => SceneManager.LoadScene(targetScene));
            AudioManager.Instance.PlaySFX(AudioID.Magical);
        }
    }
}
