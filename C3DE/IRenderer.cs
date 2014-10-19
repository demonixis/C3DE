using C3DE.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE
{
    public interface IRenderer
    {
        bool NeedsBufferUpdate { get; }

        void LoadContent(ContentManager content);

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        void render(Scene scene, Camera camera);

        void RenderEditor(Scene scene, Camera camera, RenderTarget2D target);
    }
}
