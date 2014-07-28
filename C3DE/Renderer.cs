using C3DE.Components;
using C3DE.Components.Cameras;
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
        private LightPrefab _light;
        private Effect _objectFx;
        private ShadowGenerator _shadowGenerator;
        private SpriteBatch _spriteBatch;
        private Color _ambientColor;
        private PostProcessManager _postProcessManager;

        public Color AmbientColor
        {
            get { return _ambientColor; }
            set { _ambientColor = value; }
        }

        public LightPrefab Light // FIXME Create a component for that
        {
            get { return _light; }
        }

        public Renderer(GraphicsDevice device)
        {
            graphicsDevice = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _light = new LightPrefab();
            _ambientColor = Color.White;
            _shadowGenerator = new ShadowGenerator(device);
            _postProcessManager = new PostProcessManager();
        }

        /// <summary>
        /// Load shaders.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            _objectFx = content.Load<Effect>("fx/StandardEffect");
            _shadowGenerator.LoadContent(content);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Scene scene, Camera camera)
        {
            graphicsDevice.SetRenderTarget(_sceneRT);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            _objectFx.Parameters["View"].SetValue(camera.view);
            _objectFx.Parameters["Projection"].SetValue(camera.projection);

            // Shadows
            _objectFx.Parameters["shadowMapEnabled"].SetValue(_shadowGenerator.Enabled);

            if (_shadowGenerator.Enabled)
            {
                _objectFx.Parameters["shadowTexture"].SetValue(_shadowGenerator.ShadowRT);
                _objectFx.Parameters["shadowMapSize"].SetValue(_shadowGenerator.ShadowMapSize);
                _objectFx.Parameters["shadowBias"].SetValue(_shadowGenerator.ShadowBias);
                _objectFx.Parameters["shadowStrength"].SetValue(_shadowGenerator.ShadowStrength);
            }                

            // Light
            _objectFx.Parameters["lightView"].SetValue(_light.viewMatrix);
            _objectFx.Parameters["lightProjection"].SetValue(_light.projectionMatrix);
            _objectFx.Parameters["lightPosition"].SetValue(_light.Transform.Position);
            _objectFx.Parameters["lightRadius"].SetValue(Light.radius);
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

            if (_shadowGenerator.Enabled)
                _shadowGenerator.renderShadows(_renderList, _light);

            renderObjects(scene, camera);
            renderBuffers();
        }
    }
}
