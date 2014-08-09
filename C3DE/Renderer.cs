using C3DE.Components;
using C3DE.Materials;
using C3DE.PostProcess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE
{
    /// <summary>
    /// The renderer will render a scene to the screen according to a camera view.
    /// </summary>
    public class Renderer
    {
        private GraphicsDevice graphicsDevice;
        private RenderTarget2D _sceneRT;
        private SpriteBatch _spriteBatch;
        private PostProcessManager _postProcessManager;
        private Skybox _skybox;

        public Skybox Skybox
        {
            get { return _skybox; }
        }

        public Renderer(GraphicsDevice device)
        {
            graphicsDevice = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _postProcessManager = new PostProcessManager();
            _skybox = new Skybox();
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Scene scene, Camera camera)
        {
            // FIXME ...
            if (scene.Lights.Count == 0)
                return;

            graphicsDevice.SetRenderTarget(_sceneRT);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (_skybox.Enabled)
                _skybox.Draw(graphicsDevice, camera);

            foreach (Material material in scene.Materials)
                material.PrePass();

            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                if (scene.RenderList[i].MaterialCount > 0)
                {
                    scene.RenderList[i].Material.Pass(scene.RenderList[i]);
                    scene.RenderList[i].Draw(graphicsDevice);
                }
            }

            graphicsDevice.SetRenderTarget(null);
        }


        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public void render(Scene scene, Camera camera)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(graphicsDevice, scene.RenderList);

            renderObjects(scene, camera);
            renderBuffers();
        }
    }
}
