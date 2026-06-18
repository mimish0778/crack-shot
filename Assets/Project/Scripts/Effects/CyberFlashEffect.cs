using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CrackShot
{
    public class CyberFlashEffect : Singleton<CyberFlashEffect>
    {
        [SerializeField] private Image flashImage;
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.4f);

        public void PlaySuccess() => StartCoroutine(FlashScreen());

        private IEnumerator FlashScreen()
        {
            if (flashImage == null)
            {
                yield break;
            }
            flashImage.color = flashColor;
            flashImage.gameObject.SetActive(true);
            yield return CyberFx.Tween(flashDuration, t =>
                flashImage.color = flashColor.WithAlpha(Mathf.Lerp(flashColor.a, 0f, t)));
            flashImage.gameObject.SetActive(false);
        }
    }
}
