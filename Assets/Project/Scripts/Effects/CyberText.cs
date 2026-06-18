using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace CrackShot
{
    public static class CyberText
    {
        public static readonly char[] NoiseChars =
            "#$%&?".ToCharArray();

        public const string DefaultDimHex = "334455";

        public static void SetAlpha(TMP_Text text, float alpha)
        {
            if (text == null)
            {
                return;
            }
            text.color = text.color.WithAlpha(alpha);
        }

        public static string BuildDecodeRichText(
            char[] display, string target, int index,
            string fixedHex, string noiseHex, string dimHex = DefaultDimHex)
        {
            var sb = new StringBuilder(target.Length * 32);
            for (int i = 0; i < display.Length; i++)
            {
                if (i < index)
                {
                    sb.Append($"<color=#{fixedHex}>{display[i]}</color>");
                }
                else if (i == index)
                {
                    sb.Append($"<color=#{noiseHex}>{display[i]}</color>");
                }
                else
                {
                    sb.Append($"<color=#{dimHex}>{(target[i] == ' ' ? ' ' : '_')}</color>");
                }
            }
            return sb.ToString();
        }

        public static IEnumerator Decode(
            TMP_Text text, string target,
            float noiseInterval, float confirmInterval,
            string fixedHex, string noiseHex, string dimHex = DefaultDimHex,
            int noisePerChar = 2)
        {
            if (text == null || string.IsNullOrEmpty(target))
            {
                yield break;
            }

            char[] display = new char[target.Length];
            for (int i = 0; i < display.Length; i++)
            {
                display[i] = ' ';
            }

            for (int i = 0; i < target.Length; i++)
            {
                for (int n = 0; n < noisePerChar; n++)
                {
                    display[i] = target[i] == ' '
                        ? ' '
                        : NoiseChars[Random.Range(0, NoiseChars.Length)];
                    text.text = BuildDecodeRichText(display, target, i, fixedHex, noiseHex, dimHex);
                    yield return new WaitForSeconds(noiseInterval);
                }
                display[i] = target[i];
                text.text = BuildDecodeRichText(display, target, i + 1, fixedHex, noiseHex, dimHex);
                yield return new WaitForSeconds(confirmInterval);
            }
            text.text = target;
        }

        public static IEnumerator Scramble(
            TMP_Text text, string target, Color scrambleColor, Color resolvedColor,
            int iterations = 4, float interval = 0.04f)
        {
            if (text == null || string.IsNullOrEmpty(target))
            {
                yield break;
            }

            char[] buf = new char[target.Length];
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < buf.Length; j++)
                {
                    buf[j] = target[j] == ' ' ? ' ' : NoiseChars[Random.Range(0, NoiseChars.Length)];
                }
                text.text = new string(buf);
                text.color = scrambleColor;
                yield return new WaitForSeconds(interval);
            }

            text.text = target;
            text.color = resolvedColor;
        }
    }
}
