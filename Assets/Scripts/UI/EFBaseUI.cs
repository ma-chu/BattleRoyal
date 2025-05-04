using System;
using UnityEngine;
using DG.Tweening;

namespace EF.UI
{
    public class EFBaseUI : MonoBehaviour
    {
        protected RectTransform RectTrans => _rectTransIsCached ? _cachedRectTrans : CacheRectTrans();

        private bool _rectTransIsCached;
        private RectTransform _cachedRectTrans;
        
        protected void AnimateScale(float value, float duration)
        {
            var tw= RectTrans.DOScale(value * Vector3.one, duration).From();
        }

        protected void AnimatePosition(Vector2 targetPos, float duration, Action onComplete = null)
        {
            var tw = RectTrans.DOAnchorPos(targetPos, duration).From();
            tw.OnComplete(new TweenCallback(onComplete));
        }

        protected void AnimateRotation(float value, float duration)
        {
            var tw = RectTrans.DOLocalRotate(Vector3.forward * value, duration).From();
            tw.OnComplete(() => RectTrans.DOLocalRotate(Vector3.back * value, duration).From());
        }

        private RectTransform CacheRectTrans()
        {
            if (_rectTransIsCached)
                return _cachedRectTrans;

            _cachedRectTrans = transform as RectTransform;
            _rectTransIsCached = true;
            return _cachedRectTrans;
        }
    }
}
