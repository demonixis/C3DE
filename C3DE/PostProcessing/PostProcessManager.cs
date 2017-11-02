using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.PostProcessing
{
    class PostProcessManager
    {
        private List<PostProcessPass> _postProcesses;
        private RenderTarget2D _source;
        private RenderTarget2D _destination;
        private SpriteBatch _spriteBatch;

        public PostProcessManager()
        {
            _postProcesses = new List<PostProcessPass>(2);
        }

        public void Initialize(GraphicsDevice graphics)
        {
            _spriteBatch = new SpriteBatch(graphics);
            _destination = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height);
        }

        public void Add(PostProcessPass pass)
        {
            if (_postProcesses.IndexOf(pass) == -1)
            {
                pass.Initialize(Application.Content);
                _postProcesses.Add(pass);
            }
        }

        public void Apply(RenderTarget2D sceneRT)
        {
            var previousRTs = Application.GraphicsDevice.GetRenderTargets();

            _source = sceneRT;

            for (int i = 0, l = _postProcesses.Count; i < l; i++)
            {
                if (_postProcesses[i].Enabled)
                {
                    _postProcesses[i].Apply(_spriteBatch, _source, _destination);
                    _source = _destination;
                }
            }

            Application.GraphicsDevice.SetRenderTargets(previousRTs);
        }
    }
}
