using System;
using System.Collections;
using System.Collections.Generic;
using Perigon.Entities;
using UnityEngine;

namespace Perigon.Weapons
{
    public class MeleeWeapon
    {
        private const int BUFFER_SIZE = 10;
            
        private readonly IMeleeProperties _meleeProperties;

        private Collider[] _enemyBuffer = new Collider[BUFFER_SIZE];
        private float _currentCooldown = 0f;
        private Action<bool> _onHitEntity;
        public bool CanMelee => _currentCooldown <= 0f;
        
        public MeleeWeapon(IMeleeProperties meleeProperties, Action<bool> onHitEntity = null)
        {
            _meleeProperties = meleeProperties;
        }

        public void DecrementCooldown(float deltaTime)
        {
            if (_currentCooldown > 0)
                _currentCooldown -= deltaTime;
            else if (_currentCooldown < 0)
                _currentCooldown = 0;
        }

        public void AttackIfPossible(Vector3 playerPosition, Vector3 playerForwardDirection)
        {
            if (!CanMelee)
                return;
            var hits = _meleeProperties.OverlapCapsule(playerPosition, playerForwardDirection, ref _enemyBuffer);

            _currentCooldown += _meleeProperties.AttackCoolDown;
            if (hits <= 0) 
                return;
            
            for(int i = 0; i < hits; i++)
            {
                if(_enemyBuffer[i].TryGetComponent(out LifeCycleBehaviour lifeCycle))
                {
                    lifeCycle.Damage(_meleeProperties.Damage);
                    _onHitEntity?.Invoke(!lifeCycle.IsAlive);
                }
            }
        }
    }
}
