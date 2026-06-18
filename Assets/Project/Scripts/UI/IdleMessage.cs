using System.Collections;
using UnityEngine;
using TMPro;

namespace CrackShot
{
    public class IdleMessage : MonoBehaviour
    {
        [Header("Message Display")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float idleThreshold = 180f;
        [SerializeField] private float lineInterval = 1.2f;
        [SerializeField] private float decodeSpeed = 0.018f;
        [SerializeField] private float fadeOutTime = 1.5f;

        [Header("Beep Sound")]
        [SerializeField] private float beepVolume = 0.8f;
        [SerializeField] private float beepFreqLo = 400f;
        [SerializeField] private float beepFreqHi = 550f;
        [SerializeField] private float beepDuration = 0.035f;
        [SerializeField] private float beepGap = 0.022f;

        private AudioSource _beepSource;

        private static readonly string[] Lines =
        {
            "> IDLE STATE DETECTED.",
            "> SCANNING OPERATOR STATUS...",
            "> INPUT SIGNAL: NULL",
            "> RUNNING PRESENCE PROTOCOL...",
            "> BIOMETRIC SCAN: NO RESPONSE",
            "> OPERATOR ID: UNCONFIRMED",
            "> RETRYING CONNECTION... [3/3]",
            "> STATUS: STANDBY",
            "> AWAITING OPERATOR INPUT.",
            "> SYSTEM ON HOLD // END LOG",
        };

        private float _idleTimer;
        private bool _triggered;

        private void Start()
        {
            _beepSource = gameObject.AddComponent<AudioSource>();
            _beepSource.playOnAwake = false;
            _beepSource.volume = beepVolume;

            if (messageText != null)
            {
                messageText.text = "";
                messageText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_triggered)
            {
                return;
            }

            var state = GameManager.Instance?.CurrentState;
            if (state == GameManager.GameState.Shooting ||
                state == GameManager.GameState.Failed ||
                state == GameManager.GameState.Success ||
                state == GameManager.GameState.StageClear)
            {
                _idleTimer = 0f;
                return;
            }

            if (GameInput.AnyActivity)
            {
                _idleTimer = 0f;
                return;
            }

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= idleThreshold)
            {
                _triggered = true;
                StartCoroutine(ShowSequence());
            }
        }

        private IEnumerator ShowSequence()
        {
            if (messageText == null)
            {
                yield break;
            }

            float originalBgm = AudioManager.Instance?.BgmVolume ?? 1f;
            yield return StartCoroutine(FadeBgm(originalBgm, 0f, 1.0f));

            messageText.gameObject.SetActive(true);

            foreach (var line in Lines)
            {
                StartCoroutine(PlayBeeps(line.Length));
                yield return StartCoroutine(DecodeLine(line));
                yield return new WaitForSeconds(lineInterval);

                yield return CyberFx.Tween(fadeOutTime, t => CyberText.SetAlpha(messageText, 1f - t));
                messageText.text = "";
            }

            messageText.gameObject.SetActive(false);
            yield return StartCoroutine(FadeBgm(0f, originalBgm, 1.0f));
        }

        private IEnumerator PlayBeeps(int charCount)
        {
            if (_beepSource == null)
            {
                yield break;
            }

            float totalDecodeTime = charCount * decodeSpeed * 2f;
            float cycleTime = beepDuration + beepGap;
            int count = Mathf.Max(1, Mathf.RoundToInt(totalDecodeTime / cycleTime));

            float tempoScale = Random.Range(0.9f, 1.1f);

            for (int i = 0; i < count; i++)
            {
                float freq = (i % 2 == 0 ? beepFreqLo : beepFreqHi) * Random.Range(0.97f, 1.03f);
                float dur = beepDuration * tempoScale;
                var clip = GenerateBeep(freq, dur);
                _beepSource.PlayOneShot(clip, beepVolume);
                yield return new WaitForSeconds((dur + beepGap) * tempoScale);
            }
        }

        private static AudioClip GenerateBeep(float frequency, float duration, int sampleRate = 44100)
        {
            int samples = Mathf.Max(1, (int)(sampleRate * duration));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.4f;
            }
            var clip = AudioClip.Create("Beep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private IEnumerator DecodeLine(string target)
        {
            CyberText.SetAlpha(messageText, 1f);
            yield return StartCoroutine(CyberText.Decode(
                messageText, target,
                decodeSpeed * 0.5f, decodeSpeed,
                CyberFx.WhiteHex, CyberFx.PinkHex));

            messageText.color = CyberFx.White;
        }

        private IEnumerator FadeBgm(float from, float to, float duration)
        {
            if (AudioManager.Instance == null)
            {
                yield break;
            }
            yield return CyberFx.Tween(duration, t => AudioManager.Instance.SetBgmVolume(Mathf.Lerp(from, to, t)));
            AudioManager.Instance.SetBgmVolume(to);
        }
    }
}
