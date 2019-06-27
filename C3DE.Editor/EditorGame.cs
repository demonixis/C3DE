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
        private UIManager m_UIManager;
        private GizmoComponent m_Gizmo;
        private EditorScene m_EditorScene;
        private ObjectSelector m_ObjectSelector;
        private CopyPast m_CopyPast;

        public EditorGame()
            : base("C3DE Editor", 1440, 900, false)
        {
            m_ObjectSelector = new ObjectSelector();
            m_CopyPast = new CopyPast();
        }

        #region Life Cycle

        protected override void Initialize()
        {
            base.Initialize();

            m_UIManager = new UIManager(this);
            m_UIManager.Initialize(_graphicsDeviceManager);
            m_UIManager.MenuCommandSelected += OnMenuCommandSelectd;
            m_UIManager.MenuGameObjectSelected += OnMenuGameObjectSelected;
            m_UIManager.MenuComponentSelected += OnMenuComponentSelected;
            m_UIManager.TreeViewGameObjectSelected += SelectGameObject;
            m_UIManager.DrawOrder = 1000;
            Components.Add(m_UIManager);

            m_Gizmo = new GizmoComponent(this);
            m_Gizmo.ActiveMode = GizmoMode.Translate;
            m_Gizmo.TranslateEvent += OnGizmoTranslated;
            m_Gizmo.RotateEvent += OnGizmoRotated;
            m_Gizmo.ScaleEvent += OnGizmoScaled;
            Components.Add(m_Gizmo);

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
            var check = Input.Mouse.JustClicked(MouseButton.Left) && m_Gizmo.ActiveAxis == GizmoAxis.None;

            if (!check)
                return;

            var ray = Camera.Main.GetRay(Input.Mouse.Position);
            RaycastInfo info;

            if (m_EditorScene.Raycast(ray, 100, out info))
            {
                if (info.Collider.GameObject == m_ObjectSelector.GameObject)
                    return;

                if (info.Collider.GameObject.Tag == EditorTag)
                    return;

                if (info.Collider.GameObject != m_ObjectSelector.GameObject)
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
                    m_UIManager.OpenMessageBox("About", "C3DE Editor is a 3D Game Engine powered by MonoGame.");
                    break;
                case "Copy":
                    m_CopyPast.Copy = m_ObjectSelector.GameObject;
                    break;
                case "Cut": break;
                case "Past":
                    if (m_CopyPast.Copy != null)
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
            m_UIManager.AddGameObject(gameObject);
            SelectGameObject(gameObject);
        }

        private void OnMenuGameObjectSelected(string name)
        {
            var gameObject = m_EditorScene.AddGameObject(name);
            AddGameObject(gameObject);
        }

        private void OnMenuComponentSelected(string name)
        {
            m_EditorScene.AddComponent(name);
        }

        #region New

        public void NewScene()
        {
            if (m_EditorScene != null)
            {
                m_Gizmo.Selection.Clear();
                m_UIManager.ClearGameObjects();
                Application.SceneManager.Remove(m_EditorScene);
            }

            m_EditorScene = new EditorScene();
            m_EditorScene.SceneInitialized += (gameObjects) =>
            {
                for (var i = 0; i < gameObjects.Length; i++)
                    AddGameObject(gameObjects[i]);
            };

            Application.SceneManager.Add(m_EditorScene);
            Application.SceneManager.LoadLevel(0);
        }

        #endregion

        #region Save & Load

        public void SaveScene()
        {
            m_UIManager.OpenSave(SaveScene);
        }

        public void LoadScene()
        {
            m_UIManager.OpenLoadDialog(LoadScene);
        }

        public void SaveScene(string path)
        {
            try
            {
                var serScene = new SerializedScene()
                {
                    Materials = m_EditorScene.Materials.ToArray(),
                    GameObjects = m_EditorScene.GetGameObjects(),
                    RenderSettings = m_EditorScene.RenderSettings
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
                        m_EditorScene.Add(so);
                    }

                    m_EditorScene.RenderSettings.Set(serializedScene.RenderSettings);
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

            if (m_Gizmo.ActiveMode == GizmoMode.UniformScale)
                target.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                target.LocalScale += delta;

            target.LocalScale = Vector3.Clamp(target.LocalScale, Vector3.Zero, target.LocalScale);
        }

        private void OnGizmoRotated(Transform target, TransformationEventArgs e)
        {
            m_Gizmo.RotationHelper(target, e);
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

            if (m_ObjectSelector.GameObject == gameObject)
                return;

            UnselectGameObject();

            m_ObjectSelector.Set(gameObject);
            m_ObjectSelector.Select(true);
            m_CopyPast.Selected = gameObject;
            m_Gizmo.Selection.Add(gameObject.Transform);

            if (notify)
                m_UIManager.SelectGameObject(gameObject, true);
        }

        private void UnselectGameObject(bool notify = true)
        {
            var gameObject = m_ObjectSelector.GameObject;

            if (gameObject == null)
                return;

            m_Gizmo.Clear();
            m_ObjectSelector.Select(false);
            m_CopyPast.Reset();

            if (notify)
                m_UIManager.SelectGameObject(gameObject, false);
        }

        #endregion

        public void RemoveSelection()
        {
            var gameObject = m_ObjectSelector.GameObject;

            if (gameObject == null)
                return;

            UnselectGameObject();

            if (gameObject != null)
            {
                m_EditorScene.RemoveGameObject(gameObject);
                m_UIManager.RemoveGameObject(gameObject);
            }
        }

        public void Duplicate(GameObject gameObject)
        {
            var clone = (GameObject)gameObject.Clone();
            m_EditorScene.AddGameObject(clone);
            SelectGameObject(clone);
        }

        public void DuplicateSelection()
        {
            var gameObject = m_ObjectSelector.GameObject;
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
