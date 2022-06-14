using System;
using System.Collections.Generic;
using System.Reflection;
using Perigon.Character;
using Perigon.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BForBoss
{
    public class DebugWindow : MonoBehaviour
    {
        private const Key KEYCODE_CHARACTER = Key.Backquote;

        private const float CANVAS_WIDTH_MULTIPLIER = 0.15f;
        private const float CANVAS_HEIGHT_MULTIPLIER = 0.5f;
        
        //[SerializeField] private List<DebugOptions> _debugOptions;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private FreeRoamCamera _freeRoamCamera = null;

        //State Handling
        private StateManager _stateManager = StateManager.Instance;
        private State _currentState;
        private FreezeActionsUtility _freezeActionsUtility;
        
        private bool _isPanelShowing = false;
        
        private List<Type> _debugOptions = new List<Type>();
        private DebugView _currentDebugView;

        private Rect _windowRect;
        private static DebugWindow _instance = null;

        public static DebugWindow Instance => _instance;
        
        private void Awake()
        {
#if (!UNITY_EDITOR && !DEVELOPMENT_BUILD)
            Destroy(gameObject);
#endif
            
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
            
            DontDestroyOnLoad(this);
            
            foreach (Type viewType in Assembly.GetAssembly(typeof(DebugView)).GetTypes())
            {
                if (viewType.IsClass && !viewType.IsAbstract && viewType.IsSubclassOf(typeof(DebugView)))
                {
                    _debugOptions.Add(viewType);
                }
            }
            
            IInputSettings input = FindObjectOfType<FirstPersonPlayer>();
            _freezeActionsUtility = new FreezeActionsUtility(input);
        }

        private void Update()
        {
            if (Keyboard.current[KEYCODE_CHARACTER].wasPressedThisFrame)
            {
                if (_isPanelShowing)
                {
                    ClosePanel();
                }
                else
                {
                    OpenPanel();
                }
            }
        }

        private void OnGUI()
        {
            if (!_isPanelShowing)
            {
                return;
            }

            if (_currentDebugView != null && _currentDebugView.IsInitialized)
            {
                _currentDebugView.DrawGUI();
            }
            else
            {
                CreateDebugViewList();
            }
        }

        private void CreateDebugViewList()
        {
            using (new GUILayout.AreaScope(_windowRect, "Debug Options", GUI.skin.window))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Space(0.15f * _windowRect.height);

                        foreach (Type debugType in _debugOptions)
                        {
                            if (GUILayout.Button(debugType.Name))
                            {
                                ConstructorInfo constructorInfo = debugType.GetConstructors()[0];
                                _currentDebugView = constructorInfo.Invoke(GetDebugViewConstructorParameters(debugType)) as DebugView;
                            }
                        }
                    }
                    
                    GUILayout.FlexibleSpace();
                }
                
                GUILayout.FlexibleSpace();
            }
        }

        private void ResetView()
        {
            if (_currentDebugView != null)
            {
                _currentDebugView.ResetData();
                _currentDebugView = null;
            }
        }

        private void OpenPanel()
        {
            _currentState = _stateManager.GetState();
            _stateManager.SetState(State.Pause);
            _freezeActionsUtility.LockInput();
            GetCanvasRect();
            transform.localScale = Vector3.one;
            _isPanelShowing = !_isPanelShowing;
        }

        private void ClosePanel()
        {
            _freezeActionsUtility.UnlockInput();
            ResetView();
            transform.localScale = Vector3.zero;
            _stateManager.SetState(_currentState);
            _isPanelShowing = !_isPanelShowing;
        }

        
        private void OnFreeCameraDebugViewOpened()
        {
            _freeRoamCamera.gameObject.SetActive(true);
            _freeRoamCamera.Initialize(Camera.main.transform, OnFreeCameraDebugViewExited);
        }

        private void OnFreeCameraDebugViewExited()
        {
            _freeRoamCamera.gameObject.SetActive(false);
        }

        private object[] GetDebugViewConstructorParameters(Type debugType)
        {
            List<object> parameters = new List<object> {_windowRect};

            if (debugType == typeof(SceneSwitcher))
            {
                parameters.Add((Action) ClosePanel);
            }
            else if (debugType == typeof(FreeCamera))
            {
                Rect freeCameraRect = new Rect(_windowRect)
                {
                    width = _windowRect.width + 20f
                };
                parameters = new List<object>{freeCameraRect, (Action) OnFreeCameraDebugViewOpened, (Action) OnFreeCameraDebugViewExited};
            }

            return parameters.ToArray();
        }

        private void GetCanvasRect()
        {
            if (_rectTransform != null)
            {
                Vector2 anchorMin = _rectTransform.anchorMin;
                
                _windowRect = new Rect(anchorMin.x * Screen.width,
                    anchorMin.y * Screen.height, Screen.width * CANVAS_WIDTH_MULTIPLIER,
                    Screen.height * CANVAS_HEIGHT_MULTIPLIER);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            GetCanvasRect();

            if (_currentDebugView != null)
            {
                _currentDebugView.MasterRect = _windowRect;
            }
        }
    }
}
