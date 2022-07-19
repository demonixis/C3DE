using C3DE.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering.Passes
{
    public abstract class RenderPass
    {
        protected GraphicsDevice _graphicsDevice;
        protected RenderTarget2D _renderTarget;
        protected Effect _effect;

        public bool Enabled { get; set; }
        public RenderTarget2D RenderTarget => _renderTarget;

        public RenderPass(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Render(Scene scene, Camera camera);
    }
}
