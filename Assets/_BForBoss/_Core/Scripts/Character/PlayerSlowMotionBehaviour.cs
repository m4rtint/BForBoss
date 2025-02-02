using DG.Tweening;
using Perigon.Utility;
using UnityEngine;

namespace BForBoss
{
    public class PlayerSlowMotionBehaviour : MonoBehaviour
    {
        private const float DEFAULT_TIME_SCALE = 1.0f;
        
        [SerializeField, Range(0f,1f)]
        private float _targetTimeScale = 0.25f;

        [SerializeField] private float _tweenDuration = 1.0f;
        
        private bool _isSlowMotionActive = false;
        private float _fixedDeltaTime;
        private Sequence _timeScaleTween;
        private IEnergySystem _energySystem;

        private float CurrentTimeScale => Time.timeScale;

        public void Initialize(IEnergySystem energySystem)
        {
            _energySystem = energySystem;
        }
        
        public void OnSlowMotion(bool isSlowingTime)
        {
            if (!_isSlowMotionActive && isSlowingTime && _energySystem.CanExpend(EnergyExpenseType.SlowMo))
            {
                StartSlowMotion();
            }
            else if (_isSlowMotionActive && !isSlowingTime)
            {
                StopSlowMotion();
            }
        }

        public void PauseTween()
        {
            if (_timeScaleTween.IsActive())
            {
                _timeScaleTween.Pause();
            }
        }

        public void ResumeTween()
        {
            if (_timeScaleTween.IsActive())
            {
                _timeScaleTween.Play();
            }
        }

        private void Start()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;
        }
        
        private void StartSlowMotion()
        {
            _isSlowMotionActive = true;
            SetupSlowMotionTweens(_targetTimeScale, VisualEffectsManager.Instance.Distort(HUDVisualEffect.SlowMotion));
        }

        private void StopSlowMotion()
        {
            _isSlowMotionActive = false;
            SetupSlowMotionTweens(DEFAULT_TIME_SCALE, VisualEffectsManager.Instance.Revert(HUDVisualEffect.SlowMotion));
        }

        private void SetupSlowMotionTweens(float targetVal, Tween vfxTween)
        {
            if (_timeScaleTween.IsActive())
            {
                _timeScaleTween.Kill();
            }
            _timeScaleTween = DOTween.Sequence();
            _timeScaleTween.Append(DOTween.To(() => CurrentTimeScale, 
                SetTimeScale, 
                targetVal,
                _tweenDuration));
            if (vfxTween != null)
            {
                _timeScaleTween.Join(vfxTween);
            }
            _timeScaleTween.timeScale = 1f;
            _timeScaleTween.Play();
        }

        private void SetTimeScale(float targetTimeScale)
        {
            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = _fixedDeltaTime * targetTimeScale;
        }

        private void Update()
        {
            if (_isSlowMotionActive)
            {
                if (_energySystem.CanExpend(EnergyExpenseType.SlowMo))
                {
                    _energySystem.Expend(EnergyExpenseType.SlowMo);
                }
                else
                {
                    StopSlowMotion();
                }
            }
        }
    }
}
