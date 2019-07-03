using C3DE.Editor.UI.Items;
using Gwen;
using Gwen.CommonDialog;
using Gwen.Control;
using Gwen.Platform;
using Gwen.Renderer.MonoGame;
using Gwen.Renderer.MonoGame.Input;
using Gwen.Skin;
using Gwen.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Editor.UI
{
    public sealed class UIManager : DrawableGameComponent
    {
        private MonoGameInput _input;
        private MonoGameRenderer _renderer;
        private SkinBase _skin;
        private Canvas _canvas;
        private bool _changeGraphicsSettings;
        private GraphicsDeviceManager _graphicsDeviceManager;
        private int _time;
        private StatusBar _statusBar;
        private TreeControl _sceneTreeControl;
        private TransformControl _transformControl;

        public event Action<string> MenuCommandSelected = null;
        public event Action<string> MenuGameObjectSelected = null;
        public event Action<string> MenuComponentSelected = null;
        public event Action<string, bool> TreeViewGameObjectSelected = null;

        public UIManager(Game game)
            : base(game)
        {
        }

        public void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphicsDeviceManager = graphicsDeviceManager;

            Platform.Init(new Gwen.Platform.MonoGame.MonoGamePlatform());
            Gwen.Loader.LoaderBase.Init(new Gwen.Loader.MonoGame.MonoGameAssetLoader(Game.Content));

            _renderer = new MonoGameRenderer(GraphicsDevice, Game.Content, Game.Content.Load<Effect>("Gwen/Shaders/GwenEffect"));
            _renderer.Resize(graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);

            _skin = new TexturedBase(_renderer, "Gwen/Skins/DefaultSkin");
            _skin.DefaultFont = new Font(_renderer, "Gwen/Fonts/Arial", 11);
            _canvas = new Canvas(_skin);
            _input = new MonoGameInput(Game);
            _input.Initialize(_canvas);

            _canvas.SetSize(graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
            _canvas.ShouldDrawBackground = false;
            _canvas.BackgroundColor = new Gwen.Color(255, 150, 170, 170);

            graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
            Game.Window.AllowUserResizing = true;
            Game.IsMouseVisible = true;
            Game.Window.ClientSizeChanged += OnClientSizeChanged;

            BuildUI();
        }

        public void OpenSave(Action<string> callback)
        {
            var dialog = Component.Create<SaveFileDialog>(_canvas);
            dialog.EnableNewFolder = true;
            dialog.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Title = "Save scene";
            dialog.Callback = callback;
        }

        public void OpenLoadDialog(Action<string> callback)
        {
            OpenFileDialog dialog = Component.Create<OpenFileDialog>(_canvas);
            dialog.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filters = "C3DE Scene Files (*.c3de)|*.c3de|All Files (*.*)|*.*";
            dialog.Title = "Load a scene";
            dialog.Callback = callback;
        }

        public void OpenMessageBox(string title, string text, int width = 320, int height = 200)
        {
            var window = new MessageBox(_canvas, text);
            window.Title = title;

            if (width > 0 && height > 0)
                window.SetSize(width, height);

            window.SetPosition(_canvas.ActualWidth / 2 - window.ActualWidth / 2, _canvas.ActualHeight / 2 - window.ActualHeight / 2);
        }

        public void SetStatusMessage(string message)
        {
            _statusBar.Text = message;
        }

        private void BuildUI()
        {
            #region Main Menu

            var menu = new MenuStrip(_canvas);
            menu.Dock = Dock.Top;

            var file = menu.AddItem("File");
            file.AddItem("New", string.Empty, "Ctrl+N").Selected += OnCommandSelected;
            file.AddItem("Save", string.Empty, "Ctrl+S").Selected += OnCommandSelected;
            file.AddItem("Save As", string.Empty, "Ctrl+Maj+S").Selected += OnCommandSelected;
            file.AddItem("Load", string.Empty, "Ctrl+L").Selected += OnCommandSelected;
            file.AddItem("Exit").Selected += OnCommandSelected;

            var edit = menu.AddItem("Edit");
            edit.AddItem("Copy", string.Empty, "Ctrl+C").Selected += OnCommandSelected;
            edit.AddItem("Cut", string.Empty, "Ctrl+X").Selected += OnCommandSelected;
            edit.AddItem("Past", string.Empty, "Ctrl+V").Selected += OnCommandSelected;
            edit.AddItem("Duplicate", string.Empty, "Ctrl+D").Selected += OnCommandSelected;
            edit.AddItem("Delete", string.Empty, "Suppr").Selected += OnCommandSelected;
            edit.AddItem("Select All", string.Empty, "Ctrl+A").Selected += OnCommandSelected;
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

            var component = menu.AddItem("Components");
            var pp = component.AddItem("Post Processing");
            pp.AddItem("AverageColor").Selected += OnComponentSelected;
            pp.AddItem("C64").Selected += OnComponentSelected;
            pp.AddItem("CGA").Selected += OnComponentSelected;
            pp.AddItem("Convolution").Selected += OnComponentSelected;
            pp.AddItem("Bloom").Selected += OnComponentSelected;
            pp.AddItem("Film").Selected += OnComponentSelected;
            pp.AddItem("FXAA").Selected += OnComponentSelected;
            pp.AddItem("GlobalFog").Selected += OnComponentSelected;
            pp.AddItem("MotionBlur").Selected += OnComponentSelected;
            pp.AddItem("Screen Space Ambient Obscurance").Selected += OnComponentSelected;
            pp.AddItem("Simple Blur").Selected += OnComponentSelected;
            pp.AddItem("Vignette").Selected += OnComponentSelected;

            var help = menu.AddItem("Help");
            help.AddItem("About").Selected += OnCommandSelected;

            #endregion

            #region Main Dock

            var mainDock = new DockControl(_canvas);
            mainDock.Dock = Dock.Fill;
            mainDock.RightDock.Width = 300;

            _sceneTreeControl = new TreeControl(_canvas);
            _sceneTreeControl.Selected += OnSceneTreeNodeSelected;

            _transformControl = new TransformControl(_canvas);

            mainDock.RightDock.Add("Scene", _sceneTreeControl);
            mainDock.RightDock.Add("Inspector", _transformControl);

            _statusBar = new StatusBar(_canvas);
            _statusBar.Dock = Dock.Bottom;
            _statusBar.Text = "C3DE Editor Ready";

            #endregion
        }

        private void OnSceneTreeNodeSelected(ControlBase sender, EventArgs arguments)
        {
            TreeViewGameObjectSelected?.Invoke(_sceneTreeControl.SelectedNode.UserData.ToString(), true);
        }

        public void AddGameObject(GameObject go)
        {
            var node = _sceneTreeControl.AddNode(go.Name);
            node.UserData = go.Id;
            node.IsSelected = true;
            _transformControl.SetGameObject(go);
        }

        public void RemoveGameObject(GameObject go)
        {
            var node = _sceneTreeControl.FindNodeByUserData(go.Id, true);
            if (node != null)
            {
                if (node.IsSelected)
                    _transformControl.SetGameObject(null);

                _sceneTreeControl.RemoveNode(node);
            }
        }

        public void SelectGameObject(GameObject go, bool selected)
        {
            var node = _sceneTreeControl.FindNodeByUserData(go.Id, true);
            if (node != null)
            {
                node.IsSelected = selected;
                _transformControl.SetGameObject(selected ? go : null);
            }
        }

        public void ClearGameObjects()
        {
            _sceneTreeControl.RemoveAllNodes();
            _transformControl.SetGameObject(null);
        }

        private void OnCommandSelected(ControlBase sender, System.EventArgs arguments)
        {
            var item = (MenuItem)sender;
            MenuCommandSelected?.Invoke(item.Text);
        }

        private void OnGameObjectSelected(ControlBase sender, System.EventArgs arguments)
        {
            var item = (MenuItem)sender;
            MenuGameObjectSelected?.Invoke(item.Text);
        }

        private void OnComponentSelected(ControlBase sender, EventArgs arguments)
        {
            var item = (MenuItem)sender;
            MenuComponentSelected?.Invoke(item.Text);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (_canvas != null)
            {
                _canvas.Dispose();
                _canvas = null;
            }
            if (_skin != null)
            {
                _skin.Dispose();
                _skin = null;
            }
            if (_renderer != null)
            {
                _renderer.Dispose();
                _renderer = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_changeGraphicsSettings)
            {
                _graphicsDeviceManager.ApplyChanges();
                _changeGraphicsSettings = false;
            }

            _time += gameTime.ElapsedGameTime.Milliseconds;

            if (_time > 1000)
            {
                _time = 0;

                if (_renderer.TextCacheSize > 1000)
                    _renderer.FlushTextCache();
            }

            _input.ProcessMouseState();
            _input.ProcessKeyboardState();
            _input.ProcessTouchState();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _canvas.RenderCanvas();
            base.Draw(gameTime);
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            _graphicsDeviceManager.PreferredBackBufferWidth = Game.Window.ClientBounds.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = Game.Window.ClientBounds.Height;

            _changeGraphicsSettings = true;

            _renderer.Resize(_graphicsDeviceManager.PreferredBackBufferWidth, _graphicsDeviceManager.PreferredBackBufferHeight);
            _canvas.SetSize(_graphicsDeviceManager.PreferredBackBufferWidth, _graphicsDeviceManager.PreferredBackBufferHeight);
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.R‌​enderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
