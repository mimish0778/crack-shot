using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrackShot
{
    public abstract class ClickInterceptAnimator : ButtonAnimator
    {
        private Button.ButtonClickedEvent _originalOnClick;

        protected bool IsAnimating { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _originalOnClick = _button.onClick;
            _button.onClick = new Button.ButtonClickedEvent();
            _button.onClick.AddListener(OnClick);
            OnAfterAwake();
        }

        protected virtual void OnAfterAwake() { }

        public override void OnPointerClick(PointerEventData e) { }

        private void OnClick()
        {
            if (IsAnimating)
            {
                return;
            }
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            IsAnimating = true;
            SetButtonInteractable(false);
            yield return StartCoroutine(ClickAnimation());
            IsAnimating = false;
            SetButtonInteractable(true);
            _originalOnClick?.Invoke();
        }

        protected abstract IEnumerator ClickAnimation();

        protected override void OnDisable()
        {
            base.OnDisable();
            IsAnimating = false;
        }
    }
}
