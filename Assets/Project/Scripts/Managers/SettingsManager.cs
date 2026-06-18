using UnityEngine;
using UnityEngine.Serialization;

namespace CrackShot
{
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        public const float DefaultRotationSpeedValue = 40f;
        public const float DefaultZoomSpeedValue = 20f;

        public const float PercentScale = 100f;

        [Header("Defaults (Restored on Reset)")]
        [SerializeField] private float defaultMasterVolume = 50f;
        [FormerlySerializedAs("defaultBGMVolume")]
        [SerializeField] private float defaultBgmVolume = 50f;
        [FormerlySerializedAs("defaultSEVolume")]
        [SerializeField] private float defaultSeVolume = 50f;
        [SerializeField] private float defaultRotationSpeed = DefaultRotationSpeedValue;
        [SerializeField] private float defaultZoomSpeed = DefaultZoomSpeedValue;
        [SerializeField] private bool defaultInvertX = false;
        [SerializeField] private bool defaultInvertY = false;
        public float MasterVolume { get; private set; }
        public float BgmVolume { get; private set; }
        public float SeVolume { get; private set; }
        public float RotationSpeed { get; private set; }
        public float ZoomSpeed { get; private set; }
        public bool InvertX { get; private set; }
        public bool InvertY { get; private set; }

        protected override void OnAwake() => ApplyDefaults();

        private void Start() => Apply();

        public void SetMasterVolume(float v) { MasterVolume = v; Apply(); }
        public void SetBgmVolume(float v) { BgmVolume = v; Apply(); }
        public void SetSeVolume(float v) { SeVolume = v; Apply(); }
        public void SetRotationSpeed(float v) { RotationSpeed = v; }
        public void SetZoomSpeed(float v) { ZoomSpeed = v; }
        public void SetInvertX(bool v) { InvertX = v; Apply(); }
        public void SetInvertY(bool v) { InvertY = v; Apply(); }

        public void ResetAudio() { ApplyAudioDefaults(); Apply(); }
        public void ResetCamera() { ApplyCameraDefaults(); Apply(); }

        private void ApplyAudioDefaults()
        {
            MasterVolume = defaultMasterVolume;
            BgmVolume = defaultBgmVolume;
            SeVolume = defaultSeVolume;
        }

        private void ApplyCameraDefaults()
        {
            RotationSpeed = defaultRotationSpeed;
            ZoomSpeed = defaultZoomSpeed;
            InvertX = defaultInvertX;
            InvertY = defaultInvertY;
        }

        private void Apply()
        {
            if (AudioManager.Instance != null)
            {
                float master = MasterVolume / PercentScale;
                AudioManager.Instance.SetBgmVolume(BgmVolume / PercentScale * master);
                AudioManager.Instance.SetSeVolume(SeVolume / PercentScale * master);
            }
        }

        private void ApplyDefaults()
        {
            ApplyAudioDefaults();
            ApplyCameraDefaults();
            Apply();
        }
    }
}
