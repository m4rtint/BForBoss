using System;
using Perigon.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace BForBoss
{
    public class EnergySystemViewBehaviour : MonoBehaviour
    {
        private const float LERP_SPEED = 10F;
        [SerializeField] private Image _fillImage; 
        private IEnergyDataSubject _energyDataSubject;
        private float _targetFillAmount = 0;
        
        public void Initialize(IEnergyDataSubject energyDataSubject)
        {
            _energyDataSubject = energyDataSubject;
            _energyDataSubject.OnStateChanged += EnergyDataSubjectOnOnStateChanged;
        }

        private void EnergyDataSubjectOnOnStateChanged(EnergyData data)
        {
            _targetFillAmount = data.Value / Math.Max(data.MaxEnergyValue, 1f);
        }

        private void Update()
        {
            _fillImage.fillAmount = Mathf.Lerp(_fillImage.fillAmount, _targetFillAmount, Time.unscaledDeltaTime * LERP_SPEED);
        }

        private void Awake()
        {
            this.PanicIfNullObject(_fillImage, nameof(_fillImage));
        }

        private void OnDestroy()
        {
            _energyDataSubject.OnStateChanged -= EnergyDataSubjectOnOnStateChanged;
        }
    }
}
