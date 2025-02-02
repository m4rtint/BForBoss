using System;
using UnityEngine.InputSystem;

namespace Perigon.Utility
{
    public class PGInputSystem
    {
        private readonly InputActionAsset _actionAsset;

        private InputActionMap _playerControlsActionMap;
        private InputActionMap _UIControlsActionMap;
        
        private InputAction _reloadInputAction;
        private InputAction _fireInputAction;
        private InputAction _meleeWeaponInputAction;
        private InputAction _dashInputAction;
        private InputAction _slowTimeAction;
        private InputAction _interactAction;
        private InputAction _weaponScrollSwapInputAction;
        private InputAction _weaponDirectSwapInputAction;
        private InputAction _pauseInputAction;
        private InputAction _unpauseInputAction;
        
        public event Action<bool> OnFireAction;
        public event Action OnReloadAction;
        public event Action OnMeleeAction;
        public event Action<bool> OnDashAction;
        public event Action<bool> OnSlowTimeAction;
        public event Action<bool> OnInteractAction;
        public event Action<bool> OnScrollWeaponChangeAction;
        public event Action<int> OnDirectWeaponChangeAction;
        public event Action OnPausePressed;

        public PGInputSystem(InputActionAsset asset)
        {
            _actionAsset = asset;
            SetupActionMapInput();
            SetupFindActions();
            SetupPlayerActions();
            SetupUIActions();
        }

        public void SetToUIControls()
        {
            _UIControlsActionMap?.Enable();
            _playerControlsActionMap?.Disable();
            LockMouseUtility.Instance.UnlockMouse();
        }
        
        public void SetToPlayerControls()
        {
            _UIControlsActionMap?.Disable();
            _playerControlsActionMap?.Enable();
            LockMouseUtility.Instance.LockMouse();
        }

        public void ForceUnbind()
        {
            _fireInputAction.started -= OnFireInputAction;
            _fireInputAction.canceled -= OnFireInputAction;

            _reloadInputAction.performed -= OnReloadInputAction;
            
            _meleeWeaponInputAction.performed -= OnMeleeInputAction;

            _dashInputAction.started -= OnDashInputAction;
            _dashInputAction.canceled -= OnDashInputAction;

            _slowTimeAction.started -= OnSlowTimeInputAction;
            _slowTimeAction.canceled -= OnSlowTimeInputAction;
            
            _interactAction.started -= OnInteractInputAction;
            _interactAction.canceled -= OnInteractInputAction;

            _weaponScrollSwapInputAction.performed -= OnWeaponScrolledInputAction;
            _weaponDirectSwapInputAction.performed -= OnDirectWeaponChangeInputAction;

            _pauseInputAction.performed -= OnPauseInputAction;
        }
        
        private void SetupActionMapInput()
        {
            _playerControlsActionMap = _actionAsset.FindActionMap("Player Controls");
            _UIControlsActionMap = _actionAsset.FindActionMap("UI");
        }
        
        private void SetupFindActions()
        {
            _fireInputAction = _playerControlsActionMap?.FindAction("Fire");
            _reloadInputAction = _playerControlsActionMap?.FindAction("Reload");
            _meleeWeaponInputAction = _playerControlsActionMap?.FindAction("Melee");
            _dashInputAction = _playerControlsActionMap?.FindAction("Dash");
            _slowTimeAction = _playerControlsActionMap?.FindAction("SlowTime");
            _interactAction = _playerControlsActionMap?.FindAction("Interact");
            _weaponScrollSwapInputAction = _playerControlsActionMap?.FindAction("WeaponScrollSwap");
            _weaponDirectSwapInputAction = _playerControlsActionMap?.FindAction("WeaponDirectSwap");
            _pauseInputAction = _playerControlsActionMap?.FindAction("Pause");
        }

        private void SetupPlayerActions()
        {
            if (_fireInputAction != null)
            {
                _fireInputAction.started += OnFireInputAction;
                _fireInputAction.canceled += OnFireInputAction;
            }

            if (_reloadInputAction != null)
            {
                _reloadInputAction.performed += OnReloadInputAction;
            }

            if (_meleeWeaponInputAction != null)
            {
                _meleeWeaponInputAction.performed += OnMeleeInputAction;
            }

            if (_dashInputAction != null)
            {
                _dashInputAction.started += OnDashInputAction;
                _dashInputAction.canceled += OnDashInputAction;
            }

            if (_slowTimeAction != null)
            {
                _slowTimeAction.started += OnSlowTimeInputAction;
                _slowTimeAction.canceled += OnSlowTimeInputAction;
            }

            if (_interactAction != null)
            {
                _interactAction.started += OnInteractInputAction;
                _interactAction.canceled += OnInteractInputAction;
            }

            if (_weaponScrollSwapInputAction != null)
            {
                _weaponScrollSwapInputAction.performed += OnWeaponScrolledInputAction;
                _weaponDirectSwapInputAction.performed += OnDirectWeaponChangeInputAction;
            }

            if (_pauseInputAction != null)
            {
                _pauseInputAction.performed += OnPauseInputAction;
            }
        }

        private void SetupUIActions()
        {
            _unpauseInputAction = _UIControlsActionMap?.FindAction("Unpause");

            if (_unpauseInputAction != null)
            {
                _unpauseInputAction.performed += OnPauseInputAction;
            }
        }

        ~PGInputSystem()
        {
            ForceUnbind();
        }

        private void OnFireInputAction(InputAction.CallbackContext context)
        {
            var isFiring = context.ReadValue<float>();
            OnFireAction?.Invoke(isFiring >= 1);
        }
        
        private void OnReloadInputAction(InputAction.CallbackContext context)
        {
            OnReloadAction?.Invoke();
        }

        private void OnMeleeInputAction(InputAction.CallbackContext context)
        {
            OnMeleeAction?.Invoke();
        }

        private void OnDashInputAction(InputAction.CallbackContext context)
        {
            var isDashing = context.ReadValue<float>();
            OnDashAction?.Invoke(isDashing >= 1);
        }

        private void OnSlowTimeInputAction(InputAction.CallbackContext context)
        {
            var isSlowingTime = context.ReadValue<float>();
            OnSlowTimeAction?.Invoke(isSlowingTime >= 1);
        }
        
        private void OnInteractInputAction(InputAction.CallbackContext context)
        {
            var isInteracting = context.ReadValue<float>();
            OnInteractAction?.Invoke(isInteracting >= 1);
        }
        
        private void OnWeaponScrolledInputAction(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<float>();
            if (direction != 0)
            {   
                OnScrollWeaponChangeAction?.Invoke(direction > 0);
            }
        }
        
        private void OnDirectWeaponChangeInputAction(InputAction.CallbackContext context)
        {
            var key = (int)context.ReadValue<Single>();
            OnDirectWeaponChangeAction?.Invoke(key);
        }

        private void OnPauseInputAction(InputAction.CallbackContext context)
        {
            OnPausePressed?.Invoke();
        }
    }
}
