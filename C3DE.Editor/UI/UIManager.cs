using Gwen;
using Gwen.Control;
using Gwen.Platform;
using Gwen.Renderer.MonoGame;
using Gwen.Renderer.MonoGame.Input;
using Gwen.Skin;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Editor.UI
{
    public sealed class UIManager : DrawableGameComponent
    {
        private MonoGameInput m_Input;
        private MonoGameRenderer m_Renderer;
        private SkinBase m_Skin;
        private Canvas m_Canvas;
        private bool m_ChangeGraphicsSettings;
        private GraphicsDeviceManager m_GraphicsDeviceManager;
        private int m_Time;
        private StatusBar m_StatusBar;
        private TreeControl m_SceneTreeControl;

        public event Action<string> CommandSelected = null;
        public event Action<string> GameObjectSelected = null;

        public UIManager(Game game)
            : base(game)
        {
        }

        public void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            m_GraphicsDeviceManager = graphicsDeviceManager;

            Platform.Init(new Gwen.Platform.MonoGame.MonoGamePlatform());
            Gwen.Loader.LoaderBase.Init(new Gwen.Loader.MonoGame.MonoGameAssetLoader(Game.Content));

            m_Renderer = new MonoGameRenderer(GraphicsDevice, Game.Content, Game.Content.Load<Effect>("Gwen/Shaders/GwenEffect"));
            m_Renderer.Resize(graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);

            m_Skin = new TexturedBase(m_Renderer, "Gwen/Skins/DefaultSkin");
            m_Skin.DefaultFont = new Font(m_Renderer, "Gwen/Fonts/Arial", 11);
            m_Canvas = new Canvas(m_Skin);
            m_Input = new MonoGameInput(Game);
            m_Input.Initialize(m_Canvas);

            m_Canvas.SetSize(graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
            m_Canvas.ShouldDrawBackground = false;
            m_Canvas.BackgroundColor = new Gwen.Color(255, 150, 170, 170);

            graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
            Game.Window.AllowUserResizing = true;
            Game.IsMouseVisible = true;
            Game.Window.ClientSizeChanged += OnClientSizeChanged;

            BuildUI();
        }

        public void OpenMessageBox(string title, string text, int width = 320, int height = 200)
        {
            var window = new MessageBox(m_Canvas, text);
            window.Title = title;

            if (width > 0 && height > 0)
                window.SetSize(width, height);

            window.SetPosition(m_Canvas.ActualWidth / 2 - window.ActualWidth / 2, m_Canvas.ActualHeight / 2 - window.ActualHeight / 2);
        }

        public void SetStatusMessage(string message)
        {
            m_StatusBar.Text = message;
        }

        private void BuildUI()
        {
            #region Main Menu

            var menu = new MenuStrip(m_Canvas);
            menu.Dock = Dock.Top;

            var file = menu.AddItem("File");
            file.AddItem("New", string.Empty, "Ctrl+N").Selected += OnCommandSelected;
            file.AddItem("Save", string.Empty, "Ctrl+S").Selected += OnCommandSelected;
            file.AddItem("Load", string.Empty, "Ctrl+L").Selected += OnCommandSelected;
            file.AddItem("Exit").Selected += OnCommandSelected;

            var edit = menu.AddItem("Edit");
            edit.AddItem("Settings").Selected += OnCommandSelected;

            var gameObject = menu.AddItem("GameObject");
            gameObject.AddItem("Cube").Selected += OnGameObjectSelected;
            gameObject.AddItem("Cylinder").Selected += OnGameObjectSelected;
            gameObject.AddItem("Plane").Selected += OnGameObjectSelected;
            gameObject.AddItem("Pyramid").Selected += OnGameObjectSelected;
            gameObject.AddItem("Quad").Selected += OnGameObjectSelected;
            gameObject.AddItem("Sphere").Selected += OnGameObjectSelected;
            gameObject.AddItem("Torus").Selected += OnGameObjectSelected;
            gameObject.AddItem("Camera").Selected += OnGameObjectSelected;

            var terrain = gameObject.AddItem("Terrain");
            terrain.AddItem("Terrain").Selected += OnGameObjectSelected;
            terrain.AddItem("Lava").Selected += OnGameObjectSelected;
            terrain.AddItem("Water").Selected += OnGameObjectSelected;

            var lights = gameObject.AddItem("Light");
            lights.AddItem("Directional").Selected += OnGameObjectSelected;
            lights.AddItem("Point").Selected += OnGameObjectSelected;
            lights.AddItem("Spot").Selected += OnGameObjectSelected;

            var help = menu.AddItem("Help");
            help.AddItem("About").Selected += OnCommandSelected;

            #endregion

            #region Main Dock

            var mainDock = new DockControl(m_Canvas);
            mainDock.Dock = Dock.Fill;
            mainDock.RightDock.Width = 250;

            m_SceneTreeControl = new TreeControl(m_Canvas);

            var ptree = new PropertyTree(m_Canvas);
            ptree.Add("Position");
            ptree.Add("Rotation");
            ptree.Add("Local Scale");

            mainDock.RightDock.Add("Scene", m_SceneTreeControl);
            mainDock.RightDock.Add("Inspector", ptree);

            m_StatusBar = new StatusBar(m_Canvas);
            m_StatusBar.Dock = Dock.Bottom;
            m_StatusBar.Text = "C3DE Editor Ready";

            #endregion
        }

        public void AddGameObject(GameObject go)
        {
            var node = m_SceneTreeControl.AddNode(go.Name);
            node.UserData = go.Id;
        }

        public void RemoveGameObject(GameObject go)
        {
            var node = m_SceneTreeControl.FindNodeByUserData(go.Id, true);
            if (node != null)
                m_SceneTreeControl.RemoveNode(node);
        }

        public void SelectGameObject(GameObject go, bool selected)
        {
            var node = m_SceneTreeControl.FindNodeByUserData(go.Id, true);
            if (node != null)
                node.IsSelected = selected;
        }

        public void ClearGameObjects()
        {
            m_SceneTreeControl.RemoveAllNodes();
        }

        private void OnCommandSelected(ControlBase sender, System.EventArgs arguments)
        {
            var item = (Gwen.Control.MenuItem)sender;
            CommandSelected?.Invoke(item.Text);
        }

        private void OnGameObjectSelected(ControlBase sender, System.EventArgs arguments)
        {
            var item = (Gwen.Control.MenuItem)sender;
            GameObjectSelected?.Invoke(item.Text);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (m_Canvas != null)
            {
                m_Canvas.Dispose();
                m_Canvas = null;
            }
            if (m_Skin != null)
            {
                m_Skin.Dispose();
                m_Skin = null;
            }
            if (m_Renderer != null)
            {
                m_Renderer.Dispose();
                m_Renderer = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (m_ChangeGraphicsSettings)
            {
                m_GraphicsDeviceManager.ApplyChanges();
                m_ChangeGraphicsSettings = false;
            }

            m_Time += gameTime.ElapsedGameTime.Milliseconds;

            if (m_Time > 1000)
            {
                m_Time = 0;

                if (m_Renderer.TextCacheSize > 1000)
                    m_Renderer.FlushTextCache();
            }

            m_Input.ProcessMouseState();
            m_Input.ProcessKeyboardState();
            m_Input.ProcessTouchState();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            m_Canvas.RenderCanvas();
            base.Draw(gameTime);
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            m_GraphicsDeviceManager.PreferredBackBufferWidth = Game.Window.ClientBounds.Width;
            m_GraphicsDeviceManager.PreferredBackBufferHeight = Game.Window.ClientBounds.Height;

            m_ChangeGraphicsSettings = true;

            m_Renderer.Resize(m_GraphicsDeviceManager.PreferredBackBufferWidth, m_GraphicsDeviceManager.PreferredBackBufferHeight);
            m_Canvas.SetSize(m_GraphicsDeviceManager.PreferredBackBufferWidth, m_GraphicsDeviceManager.PreferredBackBufferHeight);
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.R‌​enderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
