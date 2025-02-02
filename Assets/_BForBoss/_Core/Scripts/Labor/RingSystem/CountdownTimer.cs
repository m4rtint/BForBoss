using System;
using UnityEngine;

namespace BForBoss
{
    public class CountdownTimer
    {
        public static CountdownTimer Instance
        {
            get
            {
                _instance ??= new CountdownTimer();
                return _instance;
            }
        }
        
        private static CountdownTimer _instance = null;
        
        private float _timer;
        private bool _isRunning;
        private float _delayedTimer;


        private float Timer
        {
            get => _timer;
            set
            {
                _timer = value;
                OnTimeUpdated?.Invoke(value);
            }
        }

        public event Action<float> OnTimeUpdated;
        public event Action OnTimeReset;

        private Action OnCountdownCompleted;

        public void StartCountdown(float amountOfTime, Action onCountdownCompleted = null, float delayedStartTime = 0)
        {
            _delayedTimer = delayedStartTime;
            _timer = amountOfTime;
            _isRunning = true;
            OnCountdownCompleted = onCountdownCompleted;
        }
        
        public void StopCountdown()
        {
            _isRunning = false;
            OnTimeReset?.Invoke();
        }

        public void Tick()
        {
            if (!_isRunning)
            {
                return;
            }

            if (_delayedTimer > 0)
            {
                _delayedTimer -= Time.deltaTime;
                return;
            }

            if (Timer >= 0.0f)
            {
                Timer -= Time.deltaTime;
            }
            else
            {
                CompleteCountdown();
            }
        }

        public void Reset()
        {
            _timer = 0;
            _isRunning = false;
            _delayedTimer = 0;
            
            OnTimeReset?.Invoke();
        }

        private void CompleteCountdown()
        {
            _isRunning = false;
            OnCountdownCompleted?.Invoke();
            OnTimeReset?.Invoke();
        }
    }
}
