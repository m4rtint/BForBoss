﻿using System;
using System.Collections;
using System.Collections.Generic;
using BForBoss.Labor;
using Perigon.Utility;

namespace BForBoss.RingSystem
{
    public class GroupedRingSystem : ILabor
    {
        public event Action<bool> OnLaborCompleted;
        
        private readonly RingBehaviour[] _allRings;
        private readonly float _timeToCompleteSystem;
        private readonly float _penaltyDelayedStartTime;
        private RingBehaviour _currentRing;
        private Queue<RingBehaviour> _ringQueue;
        private bool _didFailRingSystem;
        
        public GroupedRingSystem(
            RingBehaviour[] rings,
            float timeToCompleteSystem,
            float penaltyDelayedStartTime)
        {
            _allRings = rings;
            _timeToCompleteSystem = timeToCompleteSystem;
            if (timeToCompleteSystem <= 0)
            {
                PanicHelper.Panic(new Exception("Time To Complete System is <= 0, which should not be possible. Make sure time to complete system is > 0"));
            }
            _penaltyDelayedStartTime = penaltyDelayedStartTime;
            SetupRings();
        }

        private void SetupRings()
        {
            for (var i = 0; i < _allRings.Length; i++)
            {
                _allRings[i].OnRingTriggered = RingTriggered;
                _allRings[i].SetLabel((i + 1).ToString());
                _allRings[i].Initialize(_penaltyDelayedStartTime);
            }
        }

        public void Reset()
        {
            foreach (var ring in _allRings)
            {
                ring.Deactivate();
            }

            _currentRing = null;
            _ringQueue = new Queue<RingBehaviour>(_allRings);
        }

        public void Activate()
        {
            var delayedTime = _didFailRingSystem ? _penaltyDelayedStartTime : 0;
            TrySetupNextRing();
            CountdownTimer.Instance.StartCountdown(_timeToCompleteSystem, onCountdownCompleted: CountdownFinish, delayedStartTime: delayedTime);
        }

        private void RingTriggered(RingBehaviour ring)
        {
            ring.Deactivate();
            TrySetupNextRing();
        }

        private void CountdownFinish()
        {
            Perigon.Utility.Logger.LogString("Ran out of time! Resetting the labor", key: "Labor");
            Reset();
            _didFailRingSystem = true;
            CountdownTimer.Instance.StopCountdown();
            OnLaborCompleted?.Invoke(false);
        }

        private void TrySetupNextRing()
        {
            if (_ringQueue.Count == 0)
            {
                InvokeLaborCompleted();
                return;
            }
            _currentRing = _ringQueue.Dequeue();
            _currentRing.IsDelayedStart = _didFailRingSystem;
            _didFailRingSystem = false;
            _currentRing.Activate();
        }

        private void InvokeLaborCompleted()
        {
            if(_timeToCompleteSystem > 0f)
                CountdownTimer.Instance.StopCountdown();
            OnLaborCompleted?.Invoke(true);
        }
    }
}