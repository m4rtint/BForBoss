using System;
using Perigon.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Perigon.Entities
{
    public class EnemyHealthBarViewBehaviour : HealthViewBehaviour
    {
        [Resolve][SerializeField]private Image _healthBarImage;
        
        public override void Reset()
        {
            _healthBarImage.fillAmount = 1;
        }
        
        protected override void OnHealthChanged()
        {
            if (_healthBarImage != null)
            {
                _healthBarImage.fillAmount = GetHealthPercentage();
            }
        }
        
        private void Awake()
        {
            this.PanicIfNullObject(_healthBarImage, nameof(_healthBarImage));
        }
    }
}
