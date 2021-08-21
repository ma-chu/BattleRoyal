using System;
using UnityEngine;
using DG.Tweening;

namespace EF.UI
{
    public class EFBaseUI : MonoBehaviour
    {
        public RectTransform RectTrans => _rectTransIsCached ? _cachedRectTrans : CacheRectTrans();
        
        private bool _rectTransIsCached;
        private RectTransform _cachedRectTrans;
        
        public Vector2 LocalPosition
        {
            get
            {
                if (!_rectTransIsCached) CacheRectTrans();
                return _cachedRectTrans.localPosition;
            }
        }
        
        public void AnimateScale(float value, float duration)
        {
            var tw= RectTrans.DOScale(value * Vector3.one, duration).From();
            //Debug.Log("Scale animated!!!" + RectTrans.localScale + " " + tw);
        }

        public void AnimatePosition(Vector2 targetPos, float duration, Action onComplete = null)
        {
            var tw = RectTrans.DOAnchorPos(targetPos, duration).From();
            tw.OnComplete(new TweenCallback(onComplete));
        }

        public void AnimateRotation(float value, float duration)
        {
            var tw = RectTrans.DOLocalRotate(Vector3.forward * value, duration).From();
            tw.OnComplete(() => RectTrans.DOLocalRotate(Vector3.back * value, duration).From());
            //Debug.Log("Rotation animated!!! " + tw);
        }

        protected RectTransform CacheRectTrans()
        {
            if (_rectTransIsCached) return _cachedRectTrans;

            _cachedRectTrans = GetComponent<RectTransform>();

            _rectTransIsCached = true;

            return _cachedRectTrans;
        }
    }
}
