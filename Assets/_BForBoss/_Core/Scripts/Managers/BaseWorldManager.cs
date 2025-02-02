using DG.Tweening;
using Perigon.Entities;
using Perigon.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BForBoss
{
    public abstract class BaseWorldManager : MonoBehaviour
    {
        private const string ADDITIVE_USER_INTERFACE_SCENE_NAME = "AdditiveUserInterfaceScene";
        private const string ADDITIVE_DEBUG_SCENE_NAME = "AdditiveDebugScene";
        protected const string ADDITIVE_WEAPON_SCENE_NAME = "AdditiveWeaponManager";
        protected const string ADDITIVE_HUD_SCENE_NAME = "AdditiveHUDScene";

        protected readonly StateManager _stateManager = StateManager.Instance;
        private readonly LifeCycle _playerLifeCycle = new LifeCycle();

        [Title("Base Components", "", TitleAlignments.Centered)]
        [SerializeField] private EnergySystemBehaviour _energySystemBehaviour;
        
        [Title("","Base Dependencies", bold: false, horizontalLine: false)]
        [SerializeField] protected PlayerBehaviour _playerBehaviour = null;
        [SerializeField] private InputActionAsset _actionAsset;

        [Title("","Base Configuration", bold: false, horizontalLine: false)]
        [SerializeField] protected Transform _spawnLocation;
        
        protected WeaponSceneManager _weaponSceneManager;

        private PGInputSystem _inputSystem;
        private EnvironmentManager _environmentManager;

        private UserInterfaceManager _userInterfaceManager;
        private HUDManager _hudManager;

        private UserInterfaceManager UserInterfaceManager
        {
            get
            {
                if (_userInterfaceManager == null)
                {
                    _userInterfaceManager = FindObjectOfType<UserInterfaceManager>();
                }

                return _userInterfaceManager;
            }
        }

        protected WeaponSceneManager WeaponSceneManager
        {
            get
            {
                if (_weaponSceneManager == null)
                {
                    _weaponSceneManager = FindObjectOfType<WeaponSceneManager>();
                }

                return _weaponSceneManager;
            }
        }

        protected HUDManager HUDManager
        {
            get
            {
                if (_hudManager == null)
                {
                    _hudManager = FindObjectOfType<HUDManager>();
                }

                return _hudManager;
            }
        }

        protected virtual Vector3 SpawnLocation => _spawnLocation.position;
        protected virtual Quaternion SpawnLookDirection => _spawnLocation.rotation;

        protected virtual void CleanUp()
        {
            DOTween.KillAll();
            if (_environmentManager != null)
            {
                _environmentManager.CleanUp();
            }
        }

        protected virtual void Reset()
        {
            _energySystemBehaviour.Reset();
            _playerBehaviour.Reset();
            _playerBehaviour.SpawnAt(SpawnLocation, SpawnLookDirection);
            if (_environmentManager != null)
            {
                _environmentManager.Reset();
            }
            VisualEffectsManager.Instance.Reset();
            _stateManager.SetState(State.Play);
            HUDManager?.Reset();
        }

        protected virtual void Awake()
        {
            SceneManager.sceneLoaded += OnAdditiveSceneLoaded;
            _inputSystem = new PGInputSystem(_actionAsset);
            _inputSystem.OnPausePressed += HandlePausePressed;
            _stateManager.OnStateChanged += HandleStateChange;
            _environmentManager = gameObject.AddComponent<EnvironmentManager>();
            SceneManager.LoadScene(ADDITIVE_WEAPON_SCENE_NAME, LoadSceneMode.Additive);
            SceneManager.LoadScene(ADDITIVE_USER_INTERFACE_SCENE_NAME, LoadSceneMode.Additive);
            SceneManager.LoadScene(ADDITIVE_HUD_SCENE_NAME, LoadSceneMode.Additive);
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            SceneManager.LoadScene(ADDITIVE_DEBUG_SCENE_NAME, LoadSceneMode.Additive);
#endif
        }

        protected virtual void Start()
        {
            _playerBehaviour.Initialize(_inputSystem, _playerLifeCycle, _energySystemBehaviour);            
            _environmentManager.Initialize();
        }
        
        protected virtual void OnDestroy()
        {
            _stateManager.OnStateChanged -= HandleStateChange;
            _inputSystem.OnPausePressed -= HandlePausePressed;
            SceneManager.sceneLoaded -= OnAdditiveSceneLoaded;
        }

        protected virtual void OnValidate()
        {
            this.PanicIfNullObject(_playerBehaviour, nameof(_playerBehaviour));
            this.PanicIfNullObject(_energySystemBehaviour, nameof(_energySystemBehaviour));
        }
        
        private void OnAdditiveSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            switch (scene.name)
            {
                case ADDITIVE_USER_INTERFACE_SCENE_NAME:
                    UserInterfaceManager.Initialize();
                    break;
                case ADDITIVE_WEAPON_SCENE_NAME:
                    WeaponSceneManager.Initialize(_playerBehaviour, _inputSystem, _energySystemBehaviour);
                    break; 
                case ADDITIVE_HUD_SCENE_NAME:
                    HUDManager.Initialize(_playerLifeCycle, _energySystemBehaviour);
                    break;
                default:
                    return;
            }
        }
        
        private void HandlePausePressed()
        {
            switch (_stateManager.GetState())
            {
                case State.Play:
                    _stateManager.SetState(State.Pause);
                    break;
                case State.Pause:
                    _stateManager.SetState(State.Play);
                    break;
            }
        }

        private void HandleStateChange(State newState)
        {
            switch (newState)
            {
                case State.PreGame:
                {
                    HandleStatePreGame();
                    break;
                }
                case State.Play:
                {
                    HandleStatePlay();
                    break;
                }
                case State.Tutorial:
                {
                    HandleStateTutorial();
                    break;
                }
                case State.Debug:
                case State.Pause:
                {
                    HandleStatePause();
                    break;
                }
                case State.Death:
                {
                    HandleOnDeath();
                    break;
                }
                case State.EndGame:
                    HandleOnEndGame();
                    break;
            }
        }

        protected virtual void HandleStatePreGame()
        {
            CleanUp();
            Reset();
        }

        protected virtual void HandleStatePlay()
        {
            Time.timeScale = 1.0f;
            _inputSystem.SetToPlayerControls();
        }

        protected virtual void HandleStateTutorial()
        {
            Time.timeScale = 0.0f;
            _inputSystem.SetToUIControls();
        }

        protected virtual void HandleStatePause()
        {
            Time.timeScale = 0.0f;
            _inputSystem.SetToUIControls();
        }

        protected virtual void HandleOnDeath()
        {
            Time.timeScale = 0.0f;
            _inputSystem.SetToUIControls();
        }

        protected virtual void HandleOnEndGame()
        {
            Time.timeScale = 0.0f;
            _inputSystem.SetToUIControls();
        }
    }
}
