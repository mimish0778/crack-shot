using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrackShot
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private CyberSettingsEffect cyberEffect;

        [Header("Tabs")]
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button cameraTabButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform tabUnderline;
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject cameraPanel;
        [SerializeField] private float underlineSpeed = 10f;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [SerializeField] private TextMeshProUGUI seVolumeText;
        [SerializeField] private Button audioResetButton;

        [Header("Camera")]
        [SerializeField] private Slider rotationSpeedSlider;
        [SerializeField] private Slider zoomSpeedSlider;
        [SerializeField] private Toggle invertXToggle;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private TextMeshProUGUI rotationSpeedText;
        [SerializeField] private TextMeshProUGUI zoomSpeedText;
        [SerializeField] private Button cameraResetButton;

        private bool _isFirstOpen = true;
        private float _underlineTargetX;
        private bool _underlineReady;
        private bool _loadingValues;

        private void Awake()
        {
            SetSliderRange(masterVolumeSlider);
            SetSliderRange(bgmVolumeSlider);
            SetSliderRange(seVolumeSlider);

            closeButton?.onClick.AddListener(Close);
            audioTabButton?.onClick.AddListener(() => ShowTab(true));
            cameraTabButton?.onClick.AddListener(() => ShowTab(false));
            audioResetButton?.onClick.AddListener(ResetAudio);
            cameraResetButton?.onClick.AddListener(ResetCamera);

            BindSlider(masterVolumeSlider, v => SettingsManager.Instance?.SetMasterVolume(v));
            BindSlider(bgmVolumeSlider, v => SettingsManager.Instance?.SetBgmVolume(v));
            BindSlider(seVolumeSlider, v => SettingsManager.Instance?.SetSeVolume(v));
            BindSlider(rotationSpeedSlider, v => SettingsManager.Instance?.SetRotationSpeed(v));
            BindSlider(zoomSpeedSlider, v => SettingsManager.Instance?.SetZoomSpeed(v));
            BindToggle(invertXToggle, v => SettingsManager.Instance?.SetInvertX(v));
            BindToggle(invertYToggle, v => SettingsManager.Instance?.SetInvertY(v));
        }

        private static void SetSliderRange(Slider slider, float min = 0f, float max = SettingsManager.PercentScale)
        {
            if (slider == null)
            {
                return;
            }
            slider.minValue = min;
            slider.maxValue = max;
        }

        private void BindSlider(Slider slider, System.Action<float> setter)
        {
            slider?.onValueChanged.AddListener(v =>
            {
                if (_loadingValues)
                {
                    return;
                }
                setter(v);
                UpdateLabels();
            });
        }

        private void BindToggle(Toggle toggle, System.Action<bool> setter)
        {
            toggle?.onValueChanged.AddListener(v =>
            {
                if (_loadingValues)
                {
                    return;
                }
                setter(v);
            });
        }

        private void Update()
        {
            if (!_underlineReady || tabUnderline == null)
            {
                return;
            }
            var pos = tabUnderline.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, _underlineTargetX, Time.deltaTime * underlineSpeed);
            tabUnderline.anchoredPosition = pos;
        }

        public void Open()
        {
            AudioManager.Instance?.PlaySettingsOpen();
            settingsWindow?.SetActive(true);
            cyberEffect?.PlayOpenAnimation();

            if (_isFirstOpen)
            {
                cyberEffect?.SetTab(true, immediate: true);
                ShowTab(true);
                _isFirstOpen = false;

                if (tabUnderline != null && audioTabButton != null)
                {
                    _underlineTargetX = audioTabButton.GetComponent<RectTransform>().anchoredPosition.x;
                    var pos = tabUnderline.anchoredPosition;
                    pos.x = _underlineTargetX;
                    tabUnderline.anchoredPosition = pos;
                    _underlineReady = true;
                }
            }

            LoadValues();
        }

        private void Close()
        {
            settingsWindow?.SetActive(false);
        }

        private void ShowTab(bool isAudio)
        {
            cyberEffect?.SetTab(isAudio);
            audioPanel?.SetActive(isAudio);
            cameraPanel?.SetActive(!isAudio);

            if (tabUnderline != null)
            {
                var targetBtn = isAudio ? audioTabButton : cameraTabButton;
                if (targetBtn != null)
                {
                    _underlineTargetX = targetBtn.GetComponent<RectTransform>().anchoredPosition.x;
                }
            }
        }

        private void LoadValues()
        {
            if (SettingsManager.Instance == null)
            {
                return;
            }
            var s = SettingsManager.Instance;

            _loadingValues = true;
            if (masterVolumeSlider)
            {
                masterVolumeSlider.value = s.MasterVolume;
            }
            if (bgmVolumeSlider)
            {
                bgmVolumeSlider.value = s.BgmVolume;
            }
            if (seVolumeSlider)
            {
                seVolumeSlider.value = s.SeVolume;
            }
            if (rotationSpeedSlider)
            {
                rotationSpeedSlider.value = s.RotationSpeed;
            }
            if (zoomSpeedSlider)
            {
                zoomSpeedSlider.value = s.ZoomSpeed;
            }
            if (invertXToggle)
            {
                invertXToggle.isOn = s.InvertX;
            }
            if (invertYToggle)
            {
                invertYToggle.isOn = s.InvertY;
            }
            _loadingValues = false;

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (SettingsManager.Instance == null)
            {
                return;
            }
            var s = SettingsManager.Instance;
            if (masterVolumeText)
            {
                masterVolumeText.text = $"{Mathf.RoundToInt(s.MasterVolume)}%";
            }
            if (bgmVolumeText)
            {
                bgmVolumeText.text = $"{Mathf.RoundToInt(s.BgmVolume)}%";
            }
            if (seVolumeText)
            {
                seVolumeText.text = $"{Mathf.RoundToInt(s.SeVolume)}%";
            }
            if (rotationSpeedText)
            {
                rotationSpeedText.text = $"{s.RotationSpeed:F1}";
            }
            if (zoomSpeedText)
            {
                zoomSpeedText.text = $"{s.ZoomSpeed:F1}";
            }
        }

        private void ResetAudio()
        {
            SettingsManager.Instance?.ResetAudio();
            LoadValues();
        }

        private void ResetCamera()
        {
            SettingsManager.Instance?.ResetCamera();
            LoadValues();
        }
    }
}
