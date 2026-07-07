using UnityEngine;

namespace SharedCoreModule
{
    public sealed class SharedAudioManager : MonoBehaviour
    {
        private const string MasterVolumeKey = "shared_audio_master_volume";
        private const string BgmVolumeKey = "shared_audio_bgm_volume";
        private const string SfxVolumeKey = "shared_audio_sfx_volume";

        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;

        public void ConfigureSources(AudioSource bgm, AudioSource sfx)
        {
            bgmSource = bgm;
            sfxSource = sfx;
            ApplyVolumes();
        }

        private void Awake()
        {
            EnsureSources();
            LoadVolumeSettings();
            ApplyVolumes();
        }

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            EnsureSources();

            if (clip == null)
            {
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlaySfx(AudioClip clip)
        {
            EnsureSources();

            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void SetMasterVolume(float value, bool save = true)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveVolume(MasterVolumeKey, masterVolume, save);
        }

        public void SetBgmVolume(float value, bool save = true)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveVolume(BgmVolumeKey, bgmVolume, save);
        }

        public void SetSfxVolume(float value, bool save = true)
        {
            sfxVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveVolume(SfxVolumeKey, sfxVolume, save);
        }

        public void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume);
            bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, bgmVolume);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
        }

        public void ApplyVolumes()
        {
            EnsureSources();
            bgmSource.volume = masterVolume * bgmVolume;
            sfxSource.volume = masterVolume * sfxVolume;
        }

        private void EnsureSources()
        {
            if (bgmSource == null)
            {
                bgmSource = CreateAudioSource("BGM Source", true);
            }

            if (sfxSource == null)
            {
                sfxSource = CreateAudioSource("SFX Source", false);
            }
        }

        private AudioSource CreateAudioSource(string sourceName, bool loop)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

        private static void SaveVolume(string key, float value, bool save)
        {
            PlayerPrefs.SetFloat(key, value);

            if (save)
            {
                PlayerPrefs.Save();
            }
        }
    }
}
