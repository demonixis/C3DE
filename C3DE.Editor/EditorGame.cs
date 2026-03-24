using C3DE.Components;
using C3DE.Editor.Core;
using C3DE.Editor.GameComponents;
using C3DE.Editor.ProjectSystem;
using C3DE.Editor.UI;
using C3DE.Editor.Serialization;
using C3DE.Inputs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.IO;
using XNAGizmo;

namespace C3DE.Editor
{
    public class EditorGame : Engine
    {
        public static EditorGame Instance { get; private set; }
        public const string EditorTag = "Editor_Object";
        private readonly EditorContext _context;
        private readonly SceneSerializer _sceneSerializer;
        private UIManager _UIManager;
        private GizmoComponent _gizmo;
        private EditorScene _editorScene;
        private ObjectSelector _objectSelector;
        private CopyPast _copyPast;
        private bool _saveShortcutDown;
        private bool _duplicateShortcutDown;
        private bool _deleteShortcutDown;
        private bool _focusShortcutDown;

        public EditorGame()
            : base("C3DE Editor", 1440, 900, false)
        {
            Instance = this;
            _context = new EditorContext();
            _sceneSerializer = new SceneSerializer();
            _objectSelector = new ObjectSelector();
            _copyPast = new CopyPast();
        }

        public GameObject SelectedGameObject => _context.SelectedGameObject;

        #region Life Cycle

        protected override void Initialize()
        {
            base.Initialize();

            _UIManager = new UIManager(this, _context);
            _UIManager.Initialize(_graphicsDeviceManager);
            _UIManager.MenuCommandSelected += OnMenuCommandSelectd;
            _UIManager.MenuGameObjectSelected += OnMenuGameObjectSelected;
            _UIManager.MenuComponentSelected += OnMenuComponentSelected;
            _UIManager.TreeViewGameObjectSelected += SelectGameObject;
            _UIManager.ProjectSceneOpenRequested += LoadScene;
            _UIManager.StartupProjectChanged += SetStartupProject;
            _UIManager.ToolbarActionSelected += OnToolbarActionSelected;
            _UIManager.DrawOrder = 1000;
            Components.Add(_UIManager);

            _gizmo = new GizmoComponent(this);
            _gizmo.ActiveMode = GizmoMode.Translate;
            _gizmo.TranslateEvent += OnGizmoTranslated;
            _gizmo.RotateEvent += OnGizmoRotated;
            _gizmo.ScaleEvent += OnGizmoScaled;
            _gizmo.Visible = false;
            Components.Add(_gizmo);

            GUI.Skin = new GUISkin("Font/Menu");
            GUI.Skin.LoadContent(Content);

            EnsureProject();
            NewScene();
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateEditorInputState();
            CheckScene();
            HandleShortcuts();
            base.Update(gameTime);
        }

        protected override void AfterSceneRender(GameTime gameTime)
        {
            base.AfterSceneRender(gameTime);

            var previewTarget = Renderer?.EditorPreviewRenderTarget;
            if (previewTarget == null || _gizmo == null || _gizmo.Selection.Count == 0)
                return;

            GraphicsDevice.SetRenderTarget(previewTarget);
            _gizmo.Draw(gameTime);
            GraphicsDevice.SetRenderTarget(null);
        }

        private void CheckScene()
        {
            var check = Input.Mouse.JustClicked(MouseButton.Left) && _gizmo.ActiveAxis == GizmoAxis.None;

            if (!check)
                return;

            if (_UIManager.WantsMouseCapture || !_UIManager.IsSceneViewHovered || !_UIManager.SceneViewBounds.Contains(Input.Mouse.X, Input.Mouse.Y))
                return;

            var controller = GetEditorController();
            if (controller != null && controller.IsInteracting)
                return;

            if (!TryGetScenePickPosition(out var pickPosition))
                return;

            var ray = Camera.Main.GetRay(pickPosition);
            RaycastInfo info;

            if (_editorScene.Raycast(ray, 100, out info))
            {
                if (info.Collider.GameObject == _objectSelector.GameObject)
                    return;

                if (info.Collider.GameObject.Tag == EditorTag)
                    return;

                if (info.Collider.GameObject != _objectSelector.GameObject)
                    UnselectGameObject();

                SelectGameObject(info.Collider.GameObject);
            }
        }

        #endregion

        private void OnMenuCommandSelectd(string command)
        {
            switch (command)
            {
                case "New Project": NewProject(); break;
                case "Open Project": OpenProject(); break;
                case "New Scene": NewScene(); break;
                case "Load Scene": LoadScene(); break;
                case "Save Scene": SaveScene(); break;
                case "Save Scene As": SaveSceneAs(); break;
                case "Exit": Exit(); break;
                case "Play": PlayProject(); break;
                case "About":
                    _UIManager.OpenMessageBox("About", "C3DE Editor is a 3D Game Engine powered by MonoGame.");
                    break;
                case "Copy":
                    _copyPast.Copy = _objectSelector.GameObject;
                    break;
                case "Cut": break;
                case "Past":
                    if (_copyPast.Copy != null)
                        DuplicateSelection();
                    break;
                case "Delete":
                    RemoveSelection();
                    break;
                case "Duplicate":
                    DuplicateSelection();
                    break;
                case "Select All": break;
                default: break;
            }
        }

        private void AddGameObject(GameObject gameObject)
        {
            _UIManager.AddGameObject(gameObject);
            SelectGameObject(gameObject);
        }

        private void OnMenuGameObjectSelected(string name)
        {
            var gameObject = _editorScene.AddGameObject(name);
            AddGameObject(gameObject);
        }

        private void OnMenuComponentSelected(string name)
        {
            _editorScene.AddComponent(name);
        }

        private void OnToolbarActionSelected(string action)
        {
            switch (action)
            {
                case "Save":
                    SaveScene();
                    break;
                case "Translate":
                    _gizmo.ActiveMode = GizmoMode.Translate;
                    break;
                case "Rotate":
                    _gizmo.ActiveMode = GizmoMode.Rotate;
                    break;
                case "Scale":
                    _gizmo.ActiveMode = GizmoMode.NonUniformScale;
                    break;
                case "ToggleSpace":
                    _gizmo.ToggleActiveSpace();
                    _UIManager.SetStatusMessage($"Transform space: {_gizmo.ActiveSpace}");
                    break;
                case "ToggleSnap":
                    _gizmo.SnapEnabled = !_gizmo.SnapEnabled;
                    _gizmo.TranslationSnapValue = 1.0f;
                    _gizmo.RotationSnapValue = 15.0f;
                    _gizmo.ScaleSnapValue = 0.1f;
                    _UIManager.SetStatusMessage($"Snap: {(_gizmo.SnapEnabled ? "On" : "Off")}");
                    break;
            }
        }

        #region New

        public void NewScene()
        {
            EnsureProject();

            if (_editorScene != null)
            {
                _gizmo.Selection.Clear();
                _UIManager.ClearGameObjects();
                Application.SceneManager.Remove(_editorScene);
            }

            _editorScene = new EditorScene();

            Application.SceneManager.Add(_editorScene);
            Application.SceneManager.LoadLevel(0);
            _context.SetScene(_editorScene, GetDefaultScenePath(), true);
            _UIManager.SetScene(_editorScene);
            _UIManager.SetProject(_context.CurrentProjectPath, _context.AssetDatabase);
        }

        #endregion

        #region Save & Load

        public void SaveScene()
        {
            if (!string.IsNullOrWhiteSpace(_context.CurrentScenePath))
            {
                SaveScene(_context.CurrentScenePath);
                return;
            }

            SaveSceneAs();
        }

        public void LoadScene()
        {
            _UIManager.OpenLoadDialog(LoadScene);
        }

        public void SaveSceneAs()
        {
            _UIManager.OpenSave(SaveScene);
        }

        public void SaveScene(string path)
        {
            if (_editorScene == null)
                return;

            if (!path.EndsWith(".scene.json", StringComparison.OrdinalIgnoreCase))
                path += ".scene.json";

            _sceneSerializer.Save(_editorScene, path);
            _context.SetScene(_editorScene, path, false);
            _context.AssetDatabase?.Scan();
            _UIManager.SetProject(_context.CurrentProjectPath, _context.AssetDatabase);
            _UIManager.SetStatusMessage($"Scene saved: {Path.GetFileName(path)}");
        }

        public void LoadScene(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            if (_editorScene != null)
            {
                _gizmo.Selection.Clear();
                _UIManager.ClearGameObjects();
                Application.SceneManager.Remove(_editorScene);
            }

            _editorScene = _sceneSerializer.Load(path);
            Application.SceneManager.Add(_editorScene);
            Application.SceneManager.LoadLevel(0);
            _context.SetScene(_editorScene, path, false);
            _UIManager.SetScene(_editorScene);
            _UIManager.SetStatusMessage($"Scene loaded: {Path.GetFileName(path)}");
        }

        #endregion

        #region Gizmo Management

        private void OnGizmoScaled(Transform target, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value;

            if (_gizmo.ActiveMode == GizmoMode.UniformScale)
                target.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                target.LocalScale += delta;

            target.LocalScale = Vector3.Clamp(target.LocalScale, Vector3.Zero, target.LocalScale);
        }

        private void OnGizmoRotated(Transform target, TransformationEventArgs e)
        {
            _gizmo.RotationHelper(target, e);
        }

        private void OnGizmoTranslated(Transform target, TransformationEventArgs e)
        {
            var value = (Vector3)e.Value;

            if (Input.Keys.Pressed(Keys.LeftControl))
            {
                if (Vector3.Distance((Vector3)e.Value, target.Position) > 0.05f)
                {
                    var x = target.Position.X + Math.Sign(value.X);
                    var y = target.Position.Y + Math.Sign(value.Y);
                    var z = target.Position.Z + Math.Sign(value.Z);
                    target.SetLocalPosition(x, y, z);
                }
            }
            else
                target.LocalPosition += value;
        }

        #endregion

        #region Select / Unselect a SceneObject

        public void SelectGameObject(string id, bool notify = true)
        {
            var gameObject = Scene.FindById(id);
            SelectGameObject(gameObject, notify);
        }

        private void SelectGameObject(GameObject gameObject, bool notify = true)
        {
            if (gameObject == null)
                return;

            if (_objectSelector.GameObject == gameObject)
                return;

            UnselectGameObject();

            _objectSelector.Set(gameObject);
            _objectSelector.Select(true);
            _copyPast.Selected = gameObject;
            _gizmo.Selection.Add(gameObject.Transform);
            _context.SetSelection(gameObject);

            if (notify)
                _UIManager.SelectGameObject(gameObject, true);
        }

        private void UnselectGameObject(bool notify = true)
        {
            var gameObject = _objectSelector.GameObject;

            if (gameObject == null)
                return;

            _gizmo.Clear();
            _objectSelector.Select(false);
            _copyPast.Reset();
            _context.SetSelection(null);

            if (notify)
                _UIManager.SelectGameObject(gameObject, false);
        }

        #endregion

        public void RemoveSelection()
        {
            var gameObject = _objectSelector.GameObject;

            if (gameObject == null)
                return;

            UnselectGameObject();

            if (gameObject != null)
            {
                _editorScene.RemoveGameObject(gameObject);
                _UIManager.RemoveGameObject(gameObject);
                _context.MarkSceneDirty();
            }
        }

        public void Duplicate(GameObject gameObject)
        {
            var clone = (GameObject)gameObject.Clone();
            _editorScene.AddGameObject(clone);
            SelectGameObject(clone);
            _context.MarkSceneDirty();
        }

        public void DuplicateSelection()
        {
            var gameObject = _objectSelector.GameObject;
            Duplicate(gameObject);
        }

        private void EnsureProject()
        {
            if (_context.CurrentProject != null)
                return;

            var root = ProjectService.GetDefaultProjectRoot();
            var project = Directory.Exists(root)
                ? ProjectService.OpenProject(root) ?? ProjectService.CreateProject(root)
                : ProjectService.CreateProject(root);

            _context.SetProject(project, root);
        }

        private void NewProject()
        {
            _UIManager.OpenFolder(CreateProject, "Create C3DE Project");
        }

        private void OpenProject()
        {
            _UIManager.OpenFolder(OpenProject, "Open C3DE Project");
        }

        private void CreateProject(string selectedPath)
        {
            var root = ProjectService.NormalizeProjectRoot(selectedPath);
            var project = ProjectService.CreateProject(root);
            _context.SetProject(project, root);
            _UIManager.SetProject(root, _context.AssetDatabase);
            _UIManager.SetStatusMessage($"Project created: {project.Name}");
            NewScene();
        }

        private void OpenProject(string selectedPath)
        {
            var root = ProjectService.NormalizeOpenPath(selectedPath);
            var project = ProjectService.OpenProject(root);
            if (project == null)
            {
                _UIManager.OpenMessageBox("Project", "No project.json found in this folder.");
                return;
            }

            _context.SetProject(project, root);
            _UIManager.SetProject(root, _context.AssetDatabase);
            _UIManager.SetStatusMessage($"Project opened: {project.Name}");
        }

        private void SetStartupProject(string startupProject)
        {
            if (_context.CurrentProject == null || string.IsNullOrWhiteSpace(_context.CurrentProjectPath))
                return;

            _context.CurrentProject.StartupProject = startupProject?.Trim() ?? string.Empty;
            ProjectService.SaveProject(_context.CurrentProject, _context.CurrentProjectPath);
            _context.ClearProjectDirty();

            if (string.IsNullOrWhiteSpace(_context.CurrentProject.StartupProject))
                _UIManager.SetStatusMessage("Startup project cleared.");
            else
                _UIManager.SetStatusMessage($"Startup project set: {_context.CurrentProject.StartupProject}");
        }

        private void PlayProject()
        {
            if (_context.CurrentProject == null || string.IsNullOrWhiteSpace(_context.CurrentProjectPath))
            {
                _UIManager.OpenMessageBox("Play", "No project is currently opened.");
                return;
            }

            var startupProject = _context.CurrentProject.StartupProject;
            if (string.IsNullOrWhiteSpace(startupProject))
            {
                _UIManager.OpenMessageBox("Play", "Set a startup project in the Project panel before using Play.");
                return;
            }

            var resolvedProjectPath = Path.IsPathRooted(startupProject)
                ? startupProject
                : Path.GetFullPath(Path.Combine(_context.CurrentProjectPath, startupProject));

            if (!File.Exists(resolvedProjectPath))
            {
                _UIManager.OpenMessageBox("Play", $"Startup project not found:\n{resolvedProjectPath}");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{resolvedProjectPath}\"",
                WorkingDirectory = Path.GetDirectoryName(resolvedProjectPath) ?? _context.CurrentProjectPath,
                UseShellExecute = false
            };

            Process.Start(startInfo);
            _UIManager.SetStatusMessage($"Play: dotnet run --project {Path.GetFileName(resolvedProjectPath)}");
        }

        private string GetDefaultScenePath()
        {
            var root = _context.CurrentProjectPath ?? ProjectService.GetDefaultProjectRoot();
            return Path.Combine(root, "Scenes", "Main.scene.json");
        }

        private void UpdateEditorInputState()
        {
            var controller = GetEditorController();
            if (controller == null)
                return;

            var viewportActive = _UIManager.IsSceneViewHovered || _UIManager.IsSceneViewFocused;
            controller.ViewportInputEnabled = viewportActive && !_UIManager.IsTextInputActive;
            controller.KeyboardShortcutsEnabled = viewportActive && !_UIManager.IsTextInputActive;

            if (SelectedGameObject != null)
                controller.SetOrbitPivot(SelectedGameObject);
        }

        private void HandleShortcuts()
        {
            if (_UIManager.IsTextInputActive)
                return;

            var ctrl = Input.Keys.Pressed(Keys.LeftControl) || Input.Keys.Pressed(Keys.RightControl);
            var saveDown = ctrl && Input.Keys.Pressed(Keys.S);
            var duplicateDown = ctrl && Input.Keys.Pressed(Keys.D);
            var deleteDown = Input.Keys.Pressed(Keys.Delete);
            var focusDown = Input.Keys.Pressed(Keys.F);

            if (saveDown && !_saveShortcutDown)
                SaveScene();

            if (duplicateDown && !_duplicateShortcutDown && SelectedGameObject != null)
                DuplicateSelection();

            if (deleteDown && !_deleteShortcutDown && SelectedGameObject != null)
                RemoveSelection();

            if (focusDown && !_focusShortcutDown && SelectedGameObject != null && (_UIManager.IsSceneViewHovered || _UIManager.IsSceneViewFocused))
                GetEditorController()?.Focus(SelectedGameObject);

            _saveShortcutDown = saveDown;
            _duplicateShortcutDown = duplicateDown;
            _deleteShortcutDown = deleteDown;
            _focusShortcutDown = focusDown;
        }

        private EditorController GetEditorController()
        {
            var camera = Camera.Main;
            return camera?.GetComponent<EditorController>();
        }

        private bool TryGetScenePickPosition(out Vector2 pickPosition)
        {
            pickPosition = Vector2.Zero;

            var previewTarget = Renderer?.EditorPreviewRenderTarget;
            var bounds = _UIManager.SceneViewBounds;
            if (previewTarget == null || bounds.Width <= 0 || bounds.Height <= 0)
                return false;

            var mousePosition = Input.Mouse.Position;
            if (!bounds.Contains((int)mousePosition.X, (int)mousePosition.Y))
                return false;

            var localX = (mousePosition.X - bounds.X) / bounds.Width;
            var localY = (mousePosition.Y - bounds.Y) / bounds.Height;

            pickPosition = new Vector2(
                MathHelper.Clamp(localX, 0.0f, 1.0f) * previewTarget.Width,
                MathHelper.Clamp(localY, 0.0f, 1.0f) * previewTarget.Height);
            return true;
        }

#if !ANDROID && !NETFX_CORE
        static void Main(string[] args)
        {
            using (var game = new EditorGame())
                game.Run();
        }
#endif
    }
}
