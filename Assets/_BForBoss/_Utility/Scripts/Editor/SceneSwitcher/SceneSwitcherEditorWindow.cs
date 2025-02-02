using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Sirenix.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneSwitcherEditorWindow : EditorWindow, IHasCustomMenu
{
    private const string SCENE_FILE_EXTENSION = ".unity";
    private const string FAVORITE_ICON_NAME = "d_Favorite On Icon";
    private const string ASSETS_PATH = "Assets";

    private bool _needsToRefreshElements = false;
    private List<SceneConfigSetup> _sceneConfigSetups = new List<SceneConfigSetup>();
    private List<SceneConfigSetup> _favoriteSceneConfigs = new List<SceneConfigSetup>();
    private SceneConfigSetup _currentSceneConfig;
    private Vector2 _scrollView;
    
    private Texture2D _favouriteSymbol;

    [MenuItem("Tools/SceneSwitcher")]
    private static void Init()
    {
        SceneSwitcherEditorWindow window = (SceneSwitcherEditorWindow) GetWindow(typeof(SceneSwitcherEditorWindow));
        window.titleContent = new GUIContent("Scene Switcher");
        window.minSize = new Vector2(440, 350);

        window.Show();
    }

    public void OnSettingsProviderChanged()
    {
        _needsToRefreshElements = true;
        Repaint();
    }
    
    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Refresh Scenes"), false, ReloadScenes);
        menu.AddItem(new GUIContent("Change Settings"), false,
            () => SettingsService.OpenUserPreferences(SceneSwitcherSettingsProvider.SETTINGS_PATH));
    }

    private void OnProjectChanged()
    {
        _needsToRefreshElements = true;
    }
    
    private void ReloadScenes()
    {
        _sceneConfigSetups = new List<SceneConfigSetup>();
        _favoriteSceneConfigs = new List<SceneConfigSetup>();
        LoadFromPath(SceneSwitcherProjectSettings.GetOrCreateSettings().baseSearchFolder);
    }

    private void LoadFromPath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        string[] files = Directory.GetFiles(path);
        foreach (string fp in files)
        {
            if (fp.Contains(SCENE_FILE_EXTENSION))
            {
                string[] relativeAssetPath =  fp.Split(new string[] {Application.dataPath}, 2, StringSplitOptions.None);
                string assetPath = ASSETS_PATH + Path.DirectorySeparatorChar + relativeAssetPath[1].Substring(1);

                SceneAsset sceneLoaded = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);

                if (sceneLoaded == null)
                {
                    continue;
                }
                
                int lastFolderCharacterIndex = assetPath.LastIndexOf(Path.DirectorySeparatorChar);
                string parentFolderName = assetPath.Remove(lastFolderCharacterIndex);

                SceneConfigSetup scs = new SceneConfigSetup()
                {
                    path = assetPath,
                    parentFolder = parentFolderName
                };
                
                if (string.Equals(SceneManager.GetActiveScene().path.Replace('/', '\\'), assetPath))
                {
                    _currentSceneConfig = scs;
                }

                if (EditorPrefs.GetBool(scs.path, false))
                {
                    _favoriteSceneConfigs.Add(scs);
                }
                
                _sceneConfigSetups.Add(scs);
            }
        }

        string[] dirs = Directory.GetDirectories(path);
        foreach (string dir in dirs)
        {
            LoadFromPath(dir);
        }
    }

    private void OnFocus()
    {
        if (_needsToRefreshElements)
        {
            ReloadScenes();
            _needsToRefreshElements = false;
        }
    }
    
    private void OnGUI()
    {
        if (_needsToRefreshElements)
        {
            ReloadScenes();
            _needsToRefreshElements = false;
        }

        if (_sceneConfigSetups.IsNullOrEmpty())
        {
            return;
        }
        
        string lastFolderName = string.Empty;

        using (new EditorGUILayout.VerticalScope())
        {
            GUILayout.Space(5f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Scene Switcher", new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 30,
                    fixedWidth = 300,
                    fixedHeight = 25
                });
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(15f);
            
            if (!_favoriteSceneConfigs.IsNullOrEmpty())
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    Color currentGUIColor = GUI.color;
                    GUI.color = Color.yellow;
                    GUILayout.Label(_favouriteSymbol, GUILayout.Width(20f), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUI.color = currentGUIColor;
                    GUILayout.Label(new GUIContent("Favourite Scenes"), EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }

                foreach (SceneConfigSetup sceneConfig in _favoriteSceneConfigs)
                {
                    DrawSceneElement(sceneConfig, true);
                }

                GUILayout.Space(15f);
            }

            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollView))
            { 
                _scrollView = scrollViewScope.scrollPosition;
                foreach (SceneConfigSetup scene in _sceneConfigSetups)
                {
                    if (scene.parentFolder != lastFolderName)
                    {
                        EditorGUI.indentLevel++;
                            
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            GUILayout.Label(scene.parentFolder, EditorStyles.boldLabel);
                        }

                        EditorGUI.indentLevel--;
                            
                        lastFolderName = scene.parentFolder;
                    }
                    
                    DrawSceneElement(scene, false);
                }
            }
            
            GUILayout.Space(3f);
        }
    }
    
    private void DrawSceneElement(SceneConfigSetup scene, bool disableFavouriteToggle)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            string scenePath = scene.path;
            SceneAsset sceneLoaded = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            using (new EditorGUI.DisabledGroupScope(disableFavouriteToggle))
            {
                bool isFavoriteScene = EditorPrefs.GetBool(scenePath, false);

                Color currentGUIColor = GUI.color;
                GUI.color = isFavoriteScene ? Color.yellow : currentGUIColor;
                if (GUILayout.Button(_favouriteSymbol, GUIStyle.none, 
                    GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(20f), GUILayout.ExpandHeight(true)))
                {
                    if (!isFavoriteScene)
                    {
                        EditorPrefs.SetBool(scene.path, true);
                        _favoriteSceneConfigs.Add(scene);
                    }
                    else
                    {
                        EditorPrefs.DeleteKey(scene.path);
                        _favoriteSceneConfigs.Remove(scene);
                    }
                }
                GUI.color = currentGUIColor;
            }
            
            using (new EditorGUI.DisabledGroupScope(true))
            {
                Color defaultColor = GUI.color;
                GUI.color = (_currentSceneConfig != null && _currentSceneConfig.Equals(scene)) ? Color.green : defaultColor;
                EditorGUILayout.ObjectField(GUIContent.none, sceneLoaded, typeof(SceneAsset), false);
                GUI.color = defaultColor;
            }

            if (GUILayout.Button("Open Single"))
            { 
                if (SceneManager.GetActiveScene().isDirty) 
                { 
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) 
                    { 
                        OpenSceneSingleMode(scene);
                    }
                }
                else
                {
                    OpenSceneSingleMode(scene);
                }
            }

            if (GUILayout.Button("Open Additively"))
            {
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            }
        }
    }
    
    private void OpenSceneSingleMode(SceneConfigSetup sceneConfig)
    {
        EditorSceneManager.OpenScene(sceneConfig.path, OpenSceneMode.Single);
        _currentSceneConfig = sceneConfig;
    }

    private void LoadSymbols()
    {
        _favouriteSymbol = EditorGUIUtility.IconContent(FAVORITE_ICON_NAME).image as Texture2D;
    }
    
    private void OnEnable()
    {
        LoadSymbols();
        ReloadScenes();
        EditorApplication.projectChanged += OnProjectChanged;
    }
    
    private void OnDestroy()
    {
        EditorApplication.projectChanged -= OnProjectChanged;
    }
}
