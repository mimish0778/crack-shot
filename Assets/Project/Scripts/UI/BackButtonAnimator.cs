using System.Collections;
using UnityEngine;

namespace CrackShot
{
    public class BackButtonAnimator : ClickInterceptAnimator
    {
        [SerializeField] private float backDistance = 10f;
        [SerializeField] private float backDuration = 0.05f;
        [SerializeField] private float exitDistance = 300f;
        [SerializeField] private float exitDuration = 0.3f;

        private RectTransform _rect;
        private Vector2 _originalPos;

        protected override void OnAfterAwake()
        {
            _rect = GetComponent<RectTransform>();
            _originalPos = _rect.anchoredPosition;
        }

        protected override IEnumerator ClickAnimation()
        {
            yield return CyberFx.Tween(backDuration, t =>
            {
                _rect.anchoredPosition = Vector2.Lerp(_originalPos, _originalPos + Vector2.right * backDistance, t);
            });

            Vector2 backPos = _rect.anchoredPosition;
            yield return CyberFx.Tween(exitDuration, t =>
            {
                float eased = Ease.InQuad(t);
                _rect.anchoredPosition = Vector2.Lerp(backPos, backPos + Vector2.left * exitDistance, eased);
            });

            _rect.anchoredPosition = _originalPos;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_rect != null)
            {
                _rect.anchoredPosition = _originalPos;
            }
        }
    }
}
