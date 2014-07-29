using C3DE.Components;
using C3DE.Components.Cameras;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.PostProcess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// The renderer will render a scene to the screen according to a camera view.
    /// </summary>
    public class Renderer
    {
        private GraphicsDevice graphicsDevice;
        private RenderTarget2D _sceneRT;
        private List<RenderableComponent> _renderList;
        private Effect _objectFx;
        private SpriteBatch _spriteBatch;
        private Color _ambientColor;
        private PostProcessManager _postProcessManager;

        public Color AmbientColor
        {
            get { return _ambientColor; }
            set { _ambientColor = value; }
        }

        public Renderer(GraphicsDevice device)
        {
            graphicsDevice = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _ambientColor = Color.White;
            _postProcessManager = new PostProcessManager();
        }

        /// <summary>
        /// Load shaders.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            _objectFx = content.Load<Effect>("fx/StandardEffect");
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

            _objectFx.Parameters["View"].SetValue(camera.view);
            _objectFx.Parameters["Projection"].SetValue(camera.projection);

            // FIXME Do a loop when ok
            var light0 = scene.Lights[0];

            // Shadows
            _objectFx.Parameters["shadowMapEnabled"].SetValue(light0.shadowGenerator.Enabled);

            if (light0.shadowGenerator.Enabled)
            {
                _objectFx.Parameters["shadowTexture"].SetValue(light0.shadowGenerator.ShadowMap);
                _objectFx.Parameters["shadowMapSize"].SetValue(light0.shadowGenerator.ShadowMapSize);
                _objectFx.Parameters["shadowBias"].SetValue(light0.shadowGenerator.ShadowBias);
                _objectFx.Parameters["shadowStrength"].SetValue(light0.shadowGenerator.ShadowStrength);
            }

            // Light
            _objectFx.Parameters["lightView"].SetValue(light0.viewMatrix);
            _objectFx.Parameters["lightProjection"].SetValue(light0.projectionMatrix);
            _objectFx.Parameters["lightPosition"].SetValue(light0.SceneObject.Transform.Position);
            _objectFx.Parameters["lightRadius"].SetValue(light0.Radius);
            _objectFx.Parameters["ambientColor"].SetValue(_ambientColor.ToVector4());

            for (int i = 0; i < _renderList.Count; i++)
            {
                if (_renderList[i].MaterialCount > 0)
                {
                    _objectFx.Parameters["World"].SetValue(_renderList[i].SceneObject.Transform.world);
                    _objectFx.Parameters["mainTexture"].SetValue(scene.Materials[_renderList[i].MaterialIndices[0]].MainTexture);
                    _objectFx.Parameters["emissiveColor"].SetValue(scene.Materials[_renderList[i].MaterialIndices[0]].EmissiveColor.ToVector4());

                    _objectFx.CurrentTechnique.Passes[0].Apply();
                    _renderList[i].Draw(graphicsDevice);
                }
            }

            graphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            // FIXME Use a fullscreen quad instead of a spritebatch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null);
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
            _renderList = scene.RenderList;

            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(graphicsDevice, scene.RenderList);

            renderObjects(scene, camera);
            renderBuffers();
        }
    }
}
