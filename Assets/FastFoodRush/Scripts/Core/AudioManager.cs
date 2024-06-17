using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource BGMPlayer;
        [SerializeField] private AudioSource SFXPlayer;

        [SerializeField] private float fadeDuration = 0.75f;

        [SerializeField] private List<AudioData> SFXList;

        private Dictionary<AudioID, AudioData> SFXLookup;
        private AudioClip currentBGM;
        private float originalBGMVolume;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            originalBGMVolume = BGMPlayer.volume;
            SFXLookup = SFXList.ToDictionary(x => x.id);
        }

        public void PlayBGM(AudioClip clip, bool loop = true, bool fade = true)
        {
            if (clip == null || clip == currentBGM) return;

            currentBGM = clip;
            StartCoroutine(PlayBGMAsync(clip, loop, fade));
        }

        public void PlaySFX(AudioClip clip, bool pauseBGM = false)
        {
            if (clip == null) return;

            if (pauseBGM)
            {
                BGMPlayer.Pause();
                StartCoroutine(UnPauseBGM(clip.length));
            }

            SFXPlayer.PlayOneShot(clip);
        }

        public void PlaySFX(AudioID audioID, bool pauseBGM = false)
        {
            if (!SFXLookup.ContainsKey(audioID)) return;

            var audioData = SFXLookup[audioID];
            PlaySFX(audioData.clip, pauseBGM);
        }

        IEnumerator PlayBGMAsync(AudioClip clip, bool loop, bool fade)
        {
            if (fade) yield return BGMPlayer.DOFade(0, fadeDuration).WaitForCompletion();

            BGMPlayer.clip = clip;
            BGMPlayer.loop = loop;
            BGMPlayer.Play();

            if (fade) yield return BGMPlayer.DOFade(originalBGMVolume, fadeDuration).WaitForCompletion();
        }

        IEnumerator UnPauseBGM(float delay)
        {
            yield return new WaitForSeconds(delay);
            BGMPlayer.volume = 0;
            BGMPlayer.UnPause();
            BGMPlayer.DOFade(originalBGMVolume, fadeDuration);
        }
    }

    public enum AudioID { Money, Pop, Trash, Bin, Magical, Kaching }

    [System.Serializable]
    public class AudioData
    {
        public AudioID id;
        public AudioClip clip;
    }
}
