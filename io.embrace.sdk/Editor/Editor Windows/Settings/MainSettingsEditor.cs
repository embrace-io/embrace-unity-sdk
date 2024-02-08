using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// The MainSettingsEditor handles displaying the various parts of the Embrace Settings editor window.
    /// </summary>
    public class MainSettingsEditor : EmbraceEditorWindow
    {
        private static MainSettingsEditor _window;
        private const float MenuWidth = 135;
        private Vector2 _menuScrollPosition;
        private BaseSettingsManager[] _managers;
        private List<string> _managerNames;

        private int _menuSelection;

        public int MenuSelection
        {
            get => _menuSelection;
            set => _menuSelection = value;
        }

        #region style

        private GUIStyle _menuButton;

        private GUIStyle MenuButton
        {
            get
            {
                if (_menuButton == null)
                {
                    _menuButton = new GUIStyle(EditorStyles.label);
                    _menuButton.fontSize = 13;
                    _menuButton.alignment = TextAnchor.MiddleLeft;
                    _menuButton.padding = new RectOffset(10, 5, 5, 5);
                }

                return _menuButton;
            }
        }


        private GUIStyle _selectedMenuButton;

        private GUIStyle SelectedMenuButton
        {
            get
            {
                if (_selectedMenuButton == null)
                {
                    _selectedMenuButton = new GUIStyle(MenuButton);
                }

                if (_selectedMenuButton.active.background == null)
                {
                    var background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    background.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.243f, 0.373f, 0.588f) : new Color(0.247f, 0.494f, 0.871f));
                    background.Apply();
                    _selectedMenuButton.active.background = _selectedMenuButton.normal.background = background;
                }

                return _selectedMenuButton;
            }
        }

        private GUIStyle _managerTitle;

        private GUIStyle ManagerTitle
        {
            get
            {
                if (_managerTitle == null)
                {
                    _managerTitle = new GUIStyle(styleConfigs.defaultTextStyle.guiStyle);
                    _managerTitle.fontSize = 16;
                    _managerTitle.alignment = TextAnchor.MiddleLeft;
                }

                return _managerTitle;
            }
        }

        private GUIStyle _menuBackground;

        private GUIStyle MenuBackground
        {
            get
            {
                if (_menuBackground == null)
                {
                    _menuBackground = new GUIStyle(EditorStyles.label);
                    // The left, top, and bottom background border should extend to prevent it from being seen.
                    var overflow = _menuBackground.overflow;
                    overflow.left = overflow.top = overflow.bottom = 3;
                    _menuBackground.overflow = overflow;
                    var border = _menuBackground.border;
                    border.left = border.right = 10;
                    _menuBackground.border = border;
                    var background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    background.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.258f, 0.258f, 0.258f) : new Color(0.819f, 0.819f, 0.819f));
                    background.Apply();
                    _menuBackground.normal.background = background;
                }

                if (_menuBackground.normal.background == null)
                {
                    var background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    background.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.258f, 0.258f, 0.258f) : new Color(0.819f, 0.819f, 0.819f));
                    background.Apply();
                    _menuBackground.normal.background = background;
                }

                return _menuBackground;
            }
        }

        #endregion

        [MenuItem("Tools/Embrace/Settings")]
        public static void Init()
        {
            if (!ShouldShowEditorWindows())
            {
                return;
            }

            Setup();
            // Get existing open window or if none, make a new one:
            _window = GetWindow<MainSettingsEditor>(EmbraceEditorConstants.WindowTitleSettings);
            _window.minSize = new Vector2(600f, 500f);

            _window.Show();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            BuildManagerItems();
        }

        protected override void OnFocus()
        {
            base.OnFocus();

            // _managers list can become null if this editor window is open during a recompile
            if (_managers != null)
            {
                _managers[_menuSelection].OnFocus();
            }
        }

        protected override void OnLostFocus()
        {
            _managers[_menuSelection].OnLostFocus();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public override void OnGUI()
        {
            base.OnGUI();

            // Draw the menu..
            OnMenuGUI();

            EditorGUI.BeginChangeCheck();

            // Draw the manager.
            OnManagerGUI();
        }

        /// <summary>
        /// Draws the menu UI.
        /// </summary>
        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void OnMenuGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, MenuWidth, position.height), MenuBackground);
            _menuScrollPosition = GUILayout.BeginScrollView(_menuScrollPosition);
            GUILayout.BeginVertical();
            for (int i = 0; i < _managers.Length; ++i)
            {
                if (GUILayout.Button(_managerNames[i], (i == _menuSelection ? SelectedMenuButton : MenuButton), GUILayout.Height(32)))
                {
                    _menuSelection = i;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draws the manager UI.
        /// </summary>
        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void OnManagerGUI()
        {

            if (styleConfigs == null) EmbraceEditorWindow.Setup();
            GUILayout.BeginArea(new Rect(MenuWidth + 2, 0, position.width - MenuWidth, position.height), styleConfigs.clearBoxStyle.guiStyle);
            GUILayout.Label(_managerNames[_menuSelection], ManagerTitle);

            // Changing some settings while recompiling can lead to editor instability, so we disable interaction
            // with the manager GUI while the editor is compiling.
            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("These settings cannot be changed while scripts are compiling.", MessageType.Info);
            }
            bool guiEnabled = GUI.enabled;
            GUI.enabled &= !EditorApplication.isCompiling;

            GUILayout.Space(5);
            _managers[_menuSelection].OnGUI();
            GUILayout.EndArea();

            GUI.enabled = guiEnabled;
        }

        /// <summary>
        /// Builds the array which contains all of the IManager objects.
        /// </summary>
        private void BuildManagerItems()
        {
            var managerIndexes = new List<int>();
            BaseSettingsManager[] managerAssets = AssetDatabaseUtil.GetInstances<BaseSettingsManager>();

            // A valid manager class.
            foreach (BaseSettingsManager manager in managerAssets)
            {
                var index = managerIndexes.Count;
                if (manager.GetType().GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0)
                {
                    var item = manager.GetType().GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    index = item.Index;
                }

                managerIndexes.Add(index);
            }

            // All of the manager types have been found. Sort by the index.
            var managerTypes = managerAssets;
            Array.Sort(managerIndexes.ToArray(), managerTypes);

            _managers = managerTypes;
            _managerNames = new List<string>(managerAssets.Length);

            // The manager types have been found and sorted. Add them to the list.
            for (int i = 0; i < managerTypes.Length; ++i)
            {
                _managers[i].Initialize(this);

                var managerName = managerTypes[i].GetType().Name;
                if (managerTypes[i].GetType().GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0)
                {
                    var item = managerTypes[i].GetType().GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    managerName = item.Name;
                }

                _managerNames.Add(managerName);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _managers.Length; ++i)
            {
                _managers[i].OnDestroy();
            }
        }
    }
}