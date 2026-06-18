using System.Collections;
using UnityEngine;

namespace CrackShot
{
    public class RotateButtonAnimator : ClickInterceptAnimator
    {
        [SerializeField] protected float rotateAngle = 90f;
        [SerializeField] protected float rotateDuration = 0.3f;

        protected override IEnumerator ClickAnimation()
        {
            float startAngle = transform.eulerAngles.z, endAngle = startAngle - rotateAngle;
            yield return CyberFx.Tween(rotateDuration, t =>
            {
                float eased = Ease.InOutQuad(t);
                transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startAngle, endAngle, eased));
            });
            transform.eulerAngles = new Vector3(0f, 0f, endAngle);
        }
    }
}
