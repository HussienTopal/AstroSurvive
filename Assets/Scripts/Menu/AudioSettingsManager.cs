using UnityEngine;
using UnityEngine.UI;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Manages Master, Music, and SFX volume with PlayerPrefs persistence.
    /// Matches clean World Space Options UI style.
    /// </summary>
    public class AudioSettingsManager : MonoBehaviour
    {
        [Header("UI Sliders")]
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioClip clickSfxClip;

        private const string PREF_MASTER = "Astro_MasterVol";
        private const string PREF_MUSIC = "Astro_MusicVol";
        private const string PREF_SFX = "Astro_SfxVol";

        private void Start()
        {
            float master = PlayerPrefs.GetFloat(PREF_MASTER, 0.85f);
            float music = PlayerPrefs.GetFloat(PREF_MUSIC, 0.75f);
            float sfx = PlayerPrefs.GetFloat(PREF_SFX, 0.90f);

            if (masterSlider != null)
            {
                masterSlider.value = master;
                masterSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.value = music;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfx;
                sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            }

            ApplyVolumes(master, music, sfx);
        }

        public void SetMasterVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_MASTER, val);
            AudioListener.volume = val;
        }

        public void SetMusicVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_MUSIC, val);
            if (musicSource != null) musicSource.volume = val;
        }

        public void SetSfxVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_SFX, val);
            if (sfxSource != null) sfxSource.volume = val;
        }

        public void PlayClickSound()
        {
            if (sfxSource != null && clickSfxClip != null)
            {
                sfxSource.PlayOneShot(clickSfxClip, sfxSlider != null ? sfxSlider.value : 1.0f);
            }
        }

        private void ApplyVolumes(float master, float music, float sfx)
        {
            AudioListener.volume = master;
            if (musicSource != null) musicSource.volume = music;
            if (sfxSource != null) sfxSource.volume = sfx;
        }
    }
}
