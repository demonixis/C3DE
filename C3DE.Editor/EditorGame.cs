using C3DE.Components;
using C3DE.Editor.GameComponents;
using C3DE.Editor.UI;
using C3DE.Inputs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using XNAGizmo;

namespace C3DE.Editor
{
    public class EditorGame : Engine
    {
        public const string EditorTag = "Editor_Object";
        private UIManager _UIManager;
        private GizmoComponent _gizmo;
        private EditorScene _editorScene;
        private ObjectSelector _objectSelector;
        private CopyPast _copyPast;

        public EditorGame()
            : base("C3DE Editor", 1440, 900, false)
        {
            _objectSelector = new ObjectSelector();
            _copyPast = new CopyPast();
        }

        #region Life Cycle

        protected override void Initialize()
        {
            base.Initialize();

            _UIManager = new UIManager(this);
            _UIManager.Initialize(_graphicsDeviceManager);
            _UIManager.MenuCommandSelected += OnMenuCommandSelectd;
            _UIManager.MenuGameObjectSelected += OnMenuGameObjectSelected;
            _UIManager.MenuComponentSelected += OnMenuComponentSelected;
            _UIManager.TreeViewGameObjectSelected += SelectGameObject;
            _UIManager.DrawOrder = 1000;
            Components.Add(_UIManager);

            _gizmo = new GizmoComponent(this);
            _gizmo.ActiveMode = GizmoMode.Translate;
            _gizmo.TranslateEvent += OnGizmoTranslated;
            _gizmo.RotateEvent += OnGizmoRotated;
            _gizmo.ScaleEvent += OnGizmoScaled;
            Components.Add(_gizmo);

            GUI.Skin = new GUISkin("Font/Menu");
            GUI.Skin.LoadContent(Content);

            Serializer.AddTypes(typeof(EditorGame));

            NewScene();
        }

        protected override void Update(GameTime gameTime)
        {
            CheckScene();
            base.Update(gameTime);
        }

        private void CheckScene()
        {
            var check = Input.Mouse.JustClicked(MouseButton.Left) && _gizmo.ActiveAxis == GizmoAxis.None;

            if (!check)
                return;

            var ray = Camera.Main.GetRay(Input.Mouse.Position);
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
                case "New": NewScene(); break;
                case "Load": LoadScene(); break;
                case "Save": SaveScene(); break;
                case "Exit": Exit(); break;
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

        #region New

        public void NewScene()
        {
            if (_editorScene != null)
            {
                _gizmo.Selection.Clear();
                _UIManager.ClearGameObjects();
                Application.SceneManager.Remove(_editorScene);
            }

            _editorScene = new EditorScene();
            _editorScene.SceneInitialized += (gameObjects) =>
            {
                for (var i = 0; i < gameObjects.Length; i++)
                    AddGameObject(gameObjects[i]);
            };

            Application.SceneManager.Add(_editorScene);
            Application.SceneManager.LoadLevel(0);
        }

        #endregion

        #region Save & Load

        public void SaveScene()
        {
            _UIManager.OpenSave(SaveScene);
        }

        public void LoadScene()
        {
            _UIManager.OpenLoadDialog(LoadScene);
        }

        public void SaveScene(string path)
        {
            try
            {
                var serScene = new SerializedScene()
                {
                    Materials = _editorScene.Materials.ToArray(),
                    GameObjects = _editorScene.GetGameObjects(),
                    RenderSettings = _editorScene.RenderSettings
                };

                Serializer.Serialize(path, serScene);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        public void LoadScene(string path)
        {
            try
            {
                var data = Serializer.Deserialize(path, typeof(SerializedScene));
                var serializedScene = data as SerializedScene;
                if (serializedScene != null)
                {
                    NewScene();

                    foreach (var so in serializedScene.GameObjects)
                    {
                        so.PostDeserialize();
                        _editorScene.Add(so);
                    }

                    _editorScene.RenderSettings.Set(serializedScene.RenderSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
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
            }
        }

        public void Duplicate(GameObject gameObject)
        {
            var clone = (GameObject)gameObject.Clone();
            _editorScene.AddGameObject(clone);
            SelectGameObject(clone);
        }

        public void DuplicateSelection()
        {
            var gameObject = _objectSelector.GameObject;
            Duplicate(gameObject);
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
