﻿using System;
using System.Collections.Generic;
using BForBoss.Labor;
using FMODUnity;
using Perigon.Utility;
using PerigonGames;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = Perigon.Utility.Logger;

namespace BForBoss.RingSystem
{
    public class RingLaborManager : MonoBehaviour
    {
        [SerializeField,
         InfoBox("When player fails, delayed time before starting the round of rings again"),
        MinValue(0f)]
        private float penaltyDelayedStartTime = 3f;

        [SerializeField] private EventReference _laborCompleteAudio;
        [SerializeField] private EventReference _laborFailedAudio;

        private ILabor _ringSystem;
        private IRandomUtility _randomUtility;

        [HideInEditorMode]
        public Action OnLaborCompleted;

        public void Reset()
        {
            _ringSystem?.Reset();
            CountdownTimer.Instance.Reset();
        }

        public void Initialize(IRandomUtility randomUtility = null)
        {
            _randomUtility = randomUtility ?? new RandomUtility();
        }

        public void SetRings(RingGrouping ringSystemsToBuild)
        {
            _ringSystem = BuildLabor(ringSystemsToBuild);
            _ringSystem.OnLaborCompleted += RingSystemOnLaborCompleted;
        }

        private void RingSystemOnLaborCompleted(ILabor sender, OnLaborCompletedArgs onLaborCompleted)
        {
            if (onLaborCompleted.DidSucceed)
            {
                Logger.LogString("Labor Completed", key: "Labor");
                OnLaborCompleted?.Invoke();
            }
            else
            {
                Logger.LogString("Labor Failed, Retrying", key: "Labor");
                _ringSystem.Activate();
            }
            RuntimeManager.PlayOneShot(onLaborCompleted.DidSucceed ? _laborCompleteAudio : _laborFailedAudio);
        }

        public void ActivateSystem()
        {
            Debug.Log("Activate System");
            _ringSystem?.Activate();
        }

        private ILabor BuildLabor(RingGrouping grouping)
        {
            if (_randomUtility.NextTryGetElement(grouping.GroupOfRings, out var groupOfRings))
            {
                foreach (var ringBehaviour in groupOfRings.Rings)
                {
                    ringBehaviour.Deactivate();
                }
                var ringSystem = new GroupedRingSystem(
                    rings: groupOfRings.Rings,
                    color: grouping.RingColor,
                    timeToCompleteSystem: grouping.TimeToComplete,
                    penaltyDelayedStartTime: penaltyDelayedStartTime);
                ringSystem.OnLaborCompleted += (sender, arg) => Logger.LogString($"{(arg.DidSucceed ? "Completed" : "Failed")} {groupOfRings.Rings.Length} ring system", key:"Labor");
                return ringSystem;
            }

            PanicHelper.Panic(new Exception("Unable to get random element from grouping rings"));
            return null;
        }
    }

    [Serializable]
    public class RingGrouping
    {
        public float TimeToComplete = 5f;
        [ListDrawerSettings(ShowPaging = false)]
        public List<GroupedRingsWrapper> GroupOfRings = new List<GroupedRingsWrapper>();
        public Color RingColor;
    }
    
    [Serializable]
    public class GroupedRingsWrapper
    {
        public RingBehaviour[] Rings;
    }
}
