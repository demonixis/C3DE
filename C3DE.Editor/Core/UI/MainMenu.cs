using C3DE.Editor.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Editor
{
    public class MainMenu : DrawableGameComponent
    {
        private SpriteBatch m_SpriteBatch;
        private MenuBar m_MenuBar;
        private MenuItem m_LastSelected;

        public event Action<string> CommandSelected = null;
        public event Action<string> GameObjectSelected = null;

        public MainMenu(Game game) 
            : base(game)
        {
            var items = new[]
            {
                new MenuItem("File", OnMainItemSelected, new [] {
                   new MenuItem("New", OnCommandSelected),
                   new MenuItem("Save", OnCommandSelected),
                   new MenuItem("Load", OnCommandSelected),
                   new MenuItem("Exit", (i) => Application.Quit()),
                }),
                new MenuItem("Edition", OnMainItemSelected, new []
                {
                    new MenuItem("Settings", OnCommandSelected)
                }),
                new MenuItem("GameObject", OnMainItemSelected, new [] {
                    new MenuItem("Cube", OnGameObjectSelected),
                    new MenuItem("Cylinder", OnGameObjectSelected),
                    new MenuItem("Plane", OnGameObjectSelected),
                    new MenuItem("Pyramid", OnGameObjectSelected),
                    new MenuItem("Quad", OnGameObjectSelected),
                    new MenuItem("Sphere", OnGameObjectSelected),
                    new MenuItem("Torus", OnGameObjectSelected),
                    new MenuItem("Terrain", OnGameObjectSelected),
                    new MenuItem("Water", OnGameObjectSelected),
                    new MenuItem("-----"),
                    new MenuItem("Light", null, new[]
                    {
                        new MenuItem("Directional"),
                        new MenuItem("Point"),
                        new MenuItem("Spot")
                    })
                }),
                new MenuItem("Help",OnMainItemSelected, new []
                {
                    new MenuItem("About", OnCommandSelected)
                })
            };

            m_MenuBar = new MenuBar(items, GraphicsHelper.CreateTexture(new Color(30, 30, 30), 1, 1));
            m_MenuBar.Compute(25, 5);

            m_SpriteBatch = new SpriteBatch(game.GraphicsDevice);
        }

        public void OnCommandSelected(MenuItem item) => CommandSelected?.Invoke(item.Header);

        private void OnGameObjectSelected(MenuItem item) => GameObjectSelected?.Invoke(item.Header);

        private void OnMainItemSelected(MenuItem item)
        {
            if (m_LastSelected == item)
            {
                m_LastSelected = null;
                return;
            }

            if (m_LastSelected != null)
                m_LastSelected.Close();

            m_LastSelected = item;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_MenuBar.Update();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            m_SpriteBatch.Begin();
            m_MenuBar.Draw(m_SpriteBatch);
            m_SpriteBatch.End();
        }
    }
}
