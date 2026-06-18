using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrackShot
{
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Animation")]
        [SerializeField] protected float hoverScale = 1.04f;
        [SerializeField] protected float pressScale = 0.95f;
        [SerializeField] protected float animSpeed = 10f;
        [SerializeField] protected float pressSpeed = 30f;
        [SerializeField] protected float clickDisableDuration = 0.5f;

        [Header("Hover Color Override")]
        [SerializeField] private bool overrideHoverColor = false;
        [SerializeField] private Color hoverColorOverride;

        protected Vector3 _originalScale;
        protected Vector3 _targetScale;
        protected Button _button;
        private Coroutine _anim;

        private TMP_Text _label;
        private Color _originalTextColor;
        private bool _isHovering;

        protected virtual void Awake()
        {
            _originalScale = _targetScale = transform.localScale;
            _button = GetComponent<Button>();
            _label = GetComponentInChildren<TMP_Text>();
            if (_label)
            {
                _originalTextColor = _label.color;
            }
        }

        protected virtual void Update()
        {
            if (_isHovering && _label && IsInsideSettingsWindow())
            {
                _label.color = GetHoverColor();
            }
        }

        private void Reset() => hoverColorOverride = CyberFx.Pink;

        private bool IsInsideSettingsWindow()
        {
            var effect = CyberSettingsEffect.Instance;
            return effect != null && effect.IsInsideWindow(transform);
        }

        protected void SetButtonInteractable(bool v) { if (_button) { _button.interactable = v; } }

        public virtual void OnPointerEnter(PointerEventData e)
        {
            _isHovering = true;
            SetTarget(_originalScale * hoverScale);
            if (_label)
            {
                _label.color = GetHoverColor();
            }
        }

        protected virtual Color GetHoverColor()
        {
            if (overrideHoverColor)
            {
                return hoverColorOverride;
            }
            var effect = CyberSettingsEffect.Instance;
            if (effect != null && effect.IsInsideWindow(transform))
            {
                return effect.ActiveColor;
            }
            return Random.value > 0.5f ? CyberFx.Pink : CyberFx.Cyan;
        }

        public virtual void OnPointerExit(PointerEventData e)
        {
            _isHovering = false;
            SetTarget(_originalScale);
            if (_label)
            {
                _label.color = _originalTextColor;
            }
        }
        public virtual void OnPointerDown(PointerEventData e) => SetTarget(_originalScale * pressScale, pressSpeed);
        public virtual void OnPointerUp(PointerEventData e) => SetTarget(_originalScale * hoverScale);

        public virtual void OnPointerClick(PointerEventData e)
        {
            if (_button == null || !_button.interactable)
            {
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            StartCoroutine(DisableTemporarily());
        }

        private IEnumerator DisableTemporarily()
        {
            SetButtonInteractable(false);
            yield return new WaitForSeconds(clickDisableDuration);
            SetButtonInteractable(true);
        }

        protected void SetTarget(Vector3 target, float speed = -1f)
        {
            _targetScale = target;
            if (_anim != null)
            {
                StopCoroutine(_anim);
            }
            _anim = StartCoroutine(Animate(speed > 0 ? speed : animSpeed));
        }

        private IEnumerator Animate(float speed)
        {
            while (Vector3.Distance(transform.localScale, _targetScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * speed);
                yield return null;
            }
            transform.localScale = _targetScale;
        }

        protected virtual void OnDisable()
        {
            _isHovering = false;
            transform.localScale = _originalScale;
            if (_label)
            {
                _label.color = _originalTextColor;
            }
        }
    }
}
