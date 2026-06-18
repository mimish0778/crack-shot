using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    [System.Serializable]
    public class SoundCue
    {
        public AudioClip clip;
        [Range(0f, 2f)] public float volume = 1.0f;
        [Range(0.5f, 2f)] public float pitch = 1.0f;
    }

    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [Header("AudioSource")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;

        [Header("BGM")]
        [SerializeField] private AudioClip bgmClip;

        [Header("SE - Game")]
        [SerializeField] private SoundCue shot = new SoundCue();
        [SerializeField] private SoundCue success = new SoundCue();
        [SerializeField] private SoundCue failed = new SoundCue();
        [SerializeField] private SoundCue teleport = new SoundCue();
        [SerializeField] private SoundCue failedTeleportOut = new SoundCue();
        [SerializeField] private SoundCue failedTeleportIn = new SoundCue();
        [SerializeField] private SoundCue ballChange = new SoundCue();

        [Header("SE - Title")]
        [SerializeField] private SoundCue titlePlay = new SoundCue();

        [Header("SE - Select")]
        [SerializeField] private SoundCue selectStage = new SoundCue();
        [SerializeField] private SoundCue selectBack = new SoundCue();

        [Header("SE - Play")]
        [SerializeField] private SoundCue playSelect = new SoundCue();
        [SerializeField] private SoundCue playRetry = new SoundCue();

        [Header("SE - Result")]
        [SerializeField] private SoundCue resultReveal = new SoundCue();
        [SerializeField] private SoundCue resultLabel = new SoundCue();
        [SerializeField] private SoundCue resultSelect = new SoundCue();
        [SerializeField] private SoundCue resultRetry = new SoundCue();
        [SerializeField] private SoundCue resultNextStage = new SoundCue();

        [Header("SE - Settings")]
        [SerializeField] private SoundCue settingsOpen = new SoundCue();

        private const float MasterVolumeBoost = 2.5f;

        private IEnumerable<SoundCue> AllCues()
        {
            yield return shot;
            yield return success;
            yield return failed;
            yield return teleport;
            yield return failedTeleportOut;
            yield return failedTeleportIn;
            yield return ballChange;
            yield return titlePlay;
            yield return selectStage;
            yield return selectBack;
            yield return playSelect;
            yield return playRetry;
            yield return resultReveal;
            yield return resultLabel;
            yield return resultSelect;
            yield return resultRetry;
            yield return resultNextStage;
            yield return settingsOpen;
        }

        [ContextMenu("Reset All Volumes to 1.0")]
        private void ResetAllVolumes()
        {
            foreach (var cue in AllCues())
            {
                cue.volume = 1.0f;
            }
        }

        [ContextMenu("Reset All Pitches to 1.0")]
        private void ResetAllPitches()
        {
            foreach (var cue in AllCues())
            {
                cue.pitch = 1.0f;
            }
        }

        private void Start()
        {
            AudioListener.volume = MasterVolumeBoost;
            PlayBgm();
        }

        public void PlayBgm()
        {
            if (bgmSource == null || bgmClip == null)
            {
                return;
            }
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void SetBgmVolume(float volume)
        {
            if (bgmSource != null)
            {
                bgmSource.volume = volume;
            }
        }

        public void SetSeVolume(float volume)
        {
            if (seSource != null)
            {
                seSource.volume = volume;
            }
        }

        public float BgmVolume => bgmSource?.volume ?? 1f;

        private void PlaySE(SoundCue cue, float volumeMultiplier = 1.0f, float pitchMultiplier = 1.0f)
        {
            if (cue?.clip == null || seSource == null)
            {
                return;
            }
            seSource.pitch = cue.pitch * pitchMultiplier;
            seSource.PlayOneShot(cue.clip, cue.volume * volumeMultiplier);
        }

        public void PlayShot(float volumeMultiplier = 1.0f) => PlaySE(shot, volumeMultiplier);
        public void PlaySuccess() => PlaySE(success);
        public void PlayFailed() => PlaySE(failed);
        public void PlayTeleport(float volumeMultiplier = 1.0f) => PlaySE(teleport, volumeMultiplier);
        public void PlayFailedTeleportOut(float volumeMultiplier = 1.0f) => PlaySE(failedTeleportOut, volumeMultiplier);
        public void PlayFailedTeleportIn(float volumeMultiplier = 1.0f) => PlaySE(failedTeleportIn, volumeMultiplier);
        public void PlayBallChange() => PlaySE(ballChange);

        public void PlayTitlePlay() => PlaySE(titlePlay);

        public void PlaySelectStage() => PlaySE(selectStage);
        public void PlaySelectBack() => PlaySE(selectBack);

        public void PlayPlaySelect() => PlaySE(playSelect);
        public void PlayPlayRetry() => PlaySE(playRetry);

        public void PlayResultReveal(float pitchMultiplier = 1.0f) => PlaySE(resultReveal, 1.0f, pitchMultiplier);
        public void PlayResultLabel() => PlaySE(resultLabel);
        public void PlayResultSelect() => PlaySE(resultSelect);
        public void PlayResultRetry() => PlaySE(resultRetry);
        public void PlayResultNextStage() => PlaySE(resultNextStage);

        public void PlaySettingsOpen() => PlaySE(settingsOpen);
    }
}
