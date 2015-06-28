using C3DE.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Rendering
{
    public interface IRenderer
    {
        bool NeedsBufferUpdate { get; }

        void Initialize(ContentManager content);

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        void render(Scene scene);

        void RenderEditor(Scene scene, Camera camera, RenderTarget2D target);
    }
}
