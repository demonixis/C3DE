using Microsoft.Xna.Framework.Content;

namespace C3DE.Rendering
{
    public interface IRenderer
    {
        bool NeedsBufferUpdate { get; }

        void Initialize(ContentManager content);

        /// <summary>
        /// Render the scene.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        void Render(Scene scene);
    }
}
