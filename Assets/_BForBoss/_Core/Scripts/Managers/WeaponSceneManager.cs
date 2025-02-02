using System;
using Perigon.Utility;
using Perigon.Weapons;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BForBoss
{
    public class WeaponSceneManager : MonoBehaviour
    {
        [Title("Weapon/Equipment Component")] [SerializeField]
        private WeaponAnimationController _weaponAnimationController = null;

        [SerializeField] private EquipmentBehaviour _equipmentBehaviour = null;
        [SerializeField] private CrossHairBehaviour _crossHairBehaviour;

        public void Initialize(PlayerBehaviour playerBehaviour, PGInputSystem inputSystem, EnergySystemBehaviour energySystemBehaviour)
        {
            _weaponAnimationController.Initialize(playerBehaviour.PlayerMovement);
            _equipmentBehaviour.Initialize(
                playerBehaviour.PlayerMovement,
                inputSystem,
                _weaponAnimationController, 
                _crossHairBehaviour, 
                shootingCases: energySystemBehaviour);
            StateManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(State state)
        {
            if (state == State.PreGame)
            {
                HandleOnPreGameState();
            }

            var shouldShowWeaponHUD = state != State.EndGame;
            _weaponAnimationController.gameObject.SetActive(shouldShowWeaponHUD);
            _crossHairBehaviour.gameObject.SetActive(shouldShowWeaponHUD);
        }

        private void HandleOnPreGameState()
        {
            _equipmentBehaviour.Reset();
        }

        private void OnValidate()
        {
            this.PanicIfNullObject(_weaponAnimationController, nameof(_weaponAnimationController));
            this.PanicIfNullObject(_equipmentBehaviour, nameof(_equipmentBehaviour));
        }

        private void OnDestroy()
        {
            StateManager.Instance.OnStateChanged -= OnStateChanged;
        }
    }
}
