using System;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace EF.UI
{
    public class EFBaseUI : MonoBehaviour
    {
        protected RectTransform RectTrans => _rectTransIsCached ? _cachedRectTrans : CacheRectTrans();

        private bool _rectTransIsCached;
        private RectTransform _cachedRectTrans;

        private TweenerCore<Vector3, Vector3, VectorOptions> _scaleTween;
        private bool _isScaleTweenInitialized;
        private TweenerCore<Vector2, Vector2, VectorOptions> _positionTween;
        private bool _isPositionTweenInitialized;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTween;
        private bool _isRotationTweenInitialized;
        
        protected void AnimateScale(float value, float duration)
        {
            if (_isScaleTweenInitialized && _scaleTween.IsPlaying())
            {
                _scaleTween.Restart();
            }
            else
            {
                _scaleTween = RectTrans.DOScale(value * Vector3.one, duration).From();
                _isScaleTweenInitialized = true;
            }
        }

        protected void AnimatePosition(Vector2 targetPos, float duration, Action onComplete = null)
        {
            if (_isPositionTweenInitialized && _positionTween.IsPlaying())
            { 
                _positionTween.Restart();
            }
            else
            {
                _positionTween = RectTrans.DOAnchorPos(targetPos, duration).From();
                _isPositionTweenInitialized = true;
            }
            
            if (onComplete == null)
                return;
            
            _positionTween.OnComplete(new TweenCallback(onComplete));
        }

        protected void AnimateRotation(float value, float duration)
        {
            if (_isRotationTweenInitialized && _rotationTween.IsPlaying())
            {
                _rotationTween.Restart();
            }
            else
            {
                _rotationTween = RectTrans.DOLocalRotate(Vector3.forward * value, duration).From();
                _isRotationTweenInitialized = true;
            }
            
            _rotationTween.OnComplete(() => RectTrans.DOLocalRotate(Vector3.back * value, duration).From());
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
