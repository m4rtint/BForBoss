using System;
using Perigon.Utility;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Perigon.Weapons
{
    public interface ICharacterMovement
    {
        Vector3 CharacterVelocity { get; }
        float CharacterMaxSpeed { get; }
        bool IsGrounded { get; }
        bool IsDashing { get; }
        bool IsSliding { get; }
        bool IsWallRunning { get; }
    }
    
    public class WeaponsManager : MonoBehaviour
    {
        private const float ACCUMULATED_RECOIL_PERCENTAGE = 0.99F;
        [Resolve] [SerializeField] private GameObject _weaponHolder = null;
        [SerializeField] private EquipmentBehaviour _equipmentBehaviour = null;
        private ICharacterMovement _characterMovement = null;
        
        [Title("Weapon Bob Properties")] 
        [SerializeField]
        private float _hipFireBobAmount = 0.05f;
        [SerializeField] 
        private float _wallRunningBobMultiplier = 0.5f;
        [InfoBox("Frequency at which weapon will move around screen when moving")]
        [SerializeField]
        private float _weaponBobFrequency = 10f;
        [InfoBox("How fast the weapon bob is applied, bigger value is faster")]
        [SerializeField] 
        private float _weaponBobSharpness = 10f;

        [Title("Recoil Properties")]
        [SerializeField] 
        private float _maxRecoilDistance = 0.5f;
        [InfoBox("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
        [SerializeField]
        private float _recoilSharpness = 50f;
        [InfoBox("How fast the weapon goes back to it's original position after the recoil is finished")]
        [SerializeField]
        private float _recoilRestitutionSharpness = 10f;

        private float _weaponBobFactor = 0;
        private Vector3 _weaponBobLocalPosition;
        private Vector3 _accumulatedRecoil;
        private Vector3 _weaponRecoilLocalPosition;

        public void Initialize(ICharacterMovement characterMovement)
        {
            _characterMovement = characterMovement;
        }

        private void Start()
        {
            _equipmentBehaviour.Weapons.ForEach(weapon => weapon.OnFireWeapon += OnWeaponFired);
        }

        private void OnDestroy()
        {
            _equipmentBehaviour.Weapons.ForEach(weapon => weapon.OnFireWeapon -= OnWeaponFired);
        }

        private void OnWeaponFired(int _)
        {
            _accumulatedRecoil += Vector3.back * _equipmentBehaviour.CurrentWeapon.VisualRecoilForce;
            _accumulatedRecoil = Vector3.ClampMagnitude(_accumulatedRecoil, _maxRecoilDistance);
            FMODUnity.RuntimeManager.PlayOneShot(_equipmentBehaviour.WeaponShotAudio, transform.position);
        }

        private void OnValidate()
        {
            if (_equipmentBehaviour == null)
            {
                PanicHelper.Panic(new Exception("Equipment Behaviour missing from WeaponsManager"));
            }
        }

        private void LateUpdate()
        {
            UpdateBobbingWeaponOnMovement();
            UpdateWeaponRecoil();

            _weaponHolder.transform.localPosition = _weaponBobLocalPosition + _weaponRecoilLocalPosition;
        }

        private void UpdateBobbingWeaponOnMovement()
        {
            var characterVelocity = _characterMovement.CharacterVelocity;
            var characterMovementFactor = 0f;
            if (CanBobWeapon())
            {
                characterMovementFactor =
                    Mathf.Clamp01(characterVelocity.magnitude / _characterMovement.CharacterMaxSpeed);
            }
            
            _weaponBobFactor = Mathf.Lerp(_weaponBobFactor, characterMovementFactor, _weaponBobSharpness * Time.deltaTime);
            var bobAmount = _hipFireBobAmount * (_characterMovement.IsWallRunning ? _wallRunningBobMultiplier : 1);
            
            var hBobValue = Mathf.Sin(Time.time * _weaponBobFrequency) * bobAmount * _weaponBobFactor;
            /// Trignometric Graph Tranformation: y = A * Sin(b * x - c) + d
            var vBobValue = (Mathf.Sin(Time.time * _weaponBobFrequency * 2f) * 0.5f + 0.5f) * _weaponBobFactor * bobAmount;
            var xPosition = hBobValue;
            var yPosition = Mathf.Abs(vBobValue);

            _weaponBobLocalPosition = new Vector3(xPosition, yPosition, 0);
        }
        
        private bool CanBobWeapon()
        {
            return (_characterMovement.IsWallRunning || _characterMovement.IsGrounded) 
                   && !_characterMovement.IsDashing 
                   && !_characterMovement.IsSliding;
        }

        private void UpdateWeaponRecoil()
        {
            if (_weaponRecoilLocalPosition.z >= _accumulatedRecoil.z * ACCUMULATED_RECOIL_PERCENTAGE)
            {
                _weaponRecoilLocalPosition = Vector3.Lerp(_weaponRecoilLocalPosition, _accumulatedRecoil,
                    _recoilSharpness * Time.deltaTime);
            }
            else
            {
                _weaponRecoilLocalPosition = Vector3.Lerp(_weaponRecoilLocalPosition, Vector3.zero,
                    _recoilRestitutionSharpness * Time.deltaTime);
                _accumulatedRecoil = _weaponRecoilLocalPosition;
            }
        }
    }
}
