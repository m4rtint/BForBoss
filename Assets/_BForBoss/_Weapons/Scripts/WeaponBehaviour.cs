using PerigonGames;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Perigon.Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        private readonly Vector3 CenterOfCameraPosition = new Vector3(0.5f, 0.5f, 0);
        private readonly float FutherestDistanceToRayCast = 10000f;
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] protected Transform _firePoint = null;
        [SerializeField] private CrosshairBehaviour _crosshair = null;
        [InlineEditor]
        [SerializeField] private WeaponScriptableObject _weaponScriptableObject;
        
        protected float _elapsedRateOfFire = 0;
        protected IWeapon _weaponProperty;
        
        private IRandomUtility _randomUtility;
        private InputAction FireInputAction { get; set; }
        private Camera _mainCamera = null;
        
        protected bool CanShoot => _elapsedRateOfFire <= 0;
        
        private Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }

                return _mainCamera;
            }
        }
        
        public void Initialize(IWeapon weaponProperty = null, IRandomUtility randomUtility = null)
        {
            _randomUtility = randomUtility ?? new RandomUtility();
            _weaponProperty = weaponProperty ?? _weaponScriptableObject;
            _crosshair.SetCrosshairImage(_weaponProperty.Crosshair);
        }
        
        protected abstract void OnFire(InputAction.CallbackContext context);
        protected abstract void Update();
        
        protected void Fire()
        {
            _elapsedRateOfFire = _weaponProperty.RateOfFire;
            GenerateBullet(_firePoint.position, GetDirectionOfShot());
        }
        
        //Placeholder
        protected void GenerateBullet(Vector3 position, Vector3 fireDirection)
        {
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            var rb = bullet.AddComponent<Rigidbody>();
            rb.AddForce(fireDirection * 100, ForceMode.Impulse);
            
            bullet.transform.position = position;
        }

        private Vector3 GetDirectionOfShot()
        {
            var camRay = MainCamera.ViewportPointToRay(CenterOfCameraPosition);
            Vector3 targetPoint; 
            if (Physics.Raycast(camRay, out var hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = camRay.GetPoint(FutherestDistanceToRayCast);
            }

            var directionWithoutSpread = targetPoint - _firePoint.position;
            var directionWithSpread = directionWithoutSpread + GenerateSpreadAmount();
            return directionWithSpread.normalized;
        }

        private Vector3 GenerateSpreadAmount()
        {
            var spread = _weaponProperty.BulletSpread;
            var spreadRange = spread * 2;
            var x = -spread + (float)_randomUtility.NextDouble() * spreadRange;
            var y = -spread + (float)_randomUtility.NextDouble() * spreadRange;
            return new Vector3(x, y, 0);
        }

        private void Start()
        {
            Initialize();
            SetupPlayerInput();
        }

        private void SetupPlayerInput()
        {
            if (_actions == null)
            {
                Debug.LogWarning("Input Action Asset is missing from weapons behaviour");
                return;
            }
            
            FireInputAction = _actions.FindAction("Fire");
            if (FireInputAction != null)
            {
                FireInputAction.started += OnFire;
                FireInputAction.canceled += OnFire;
                FireInputAction.Enable();
            }
        }

    }
}
