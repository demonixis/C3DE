using C3DE.Components;
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
        private GraphicsDevice _device;
        private RenderTarget2D _sceneRT;
        private RenderTarget2D _shadowRT;
        private List<ModelRenderer> _renderList;
        private float _shadowMapSize;
        private Light _light;
        private Effect _objectFx;
        private Effect _shadowFx;
        private SpriteBatch _spriteBatch;

        public Light Light
        {
            get { return _light; }
        }

        public Renderer(GraphicsDevice device)
        {
            _device = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _light = new Light();

            SetShadowMapSize(512);
        }

        /// <summary>
        /// Load shaders.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            _objectFx = content.Load<Effect>("fx/StandardEffect");
            _shadowFx = content.Load<Effect>("fx/ShadowMapEffect");
        }

        /// <summary>
        /// Change the shadow map size and update the render target.
        /// </summary>
        /// <param name="size">Desired size, it must a power of two</param>
        public void SetShadowMapSize(int size)
        {
            _shadowRT = new RenderTarget2D(_device, size, size, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _shadowMapSize = size;
        }

        /// <summary>
        /// Render shadows for the specified camera.
        /// </summary>
        /// <param name="camera"></param>
        private void renderShadows(Camera camera)
        {
            BoundingSphere sphere = new BoundingSphere();

            if (_renderList.Count > 0)
            {
                for (int i = 0; i < _renderList.Count; i++)
                {
                    if (_renderList[i].CastShadow)
                        sphere = BoundingSphere.CreateMerged(sphere, _renderList[i].GetBoundingSphere());
                }

                _light.Update(ref sphere);
            }

            _device.SetRenderTarget(_shadowRT);
            _device.DepthStencilState = DepthStencilState.Default;
            _device.Clear(Color.White);

            _shadowFx.Parameters["View"].SetValue(_light.viewMatrix);
            _shadowFx.Parameters["Projection"].SetValue(_light.projectionMatrix);

            for (int i = 0; i < _renderList.Count; i++)
            {
                if (!_renderList[i].CastShadow)
                    continue;

                _shadowFx.Parameters["World"].SetValue(_renderList[i].SceneObject.Transform.world);
                _shadowFx.CurrentTechnique.Passes[0].Apply();

                _renderList[i].DrawMesh(_device);
            }

            _device.SetRenderTarget(null);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Camera camera)
        {
            _device.SetRenderTarget(_sceneRT);
            _device.Clear(Color.Black);
            _device.DepthStencilState = DepthStencilState.Default;

            _objectFx.Parameters["View"].SetValue(camera.view);
            _objectFx.Parameters["Projection"].SetValue(camera.projection);
            _objectFx.Parameters["shadowTexture"].SetValue(_shadowRT);
            _objectFx.Parameters["lightView"].SetValue(_light.viewMatrix);
            _objectFx.Parameters["lightProjection"].SetValue(_light.projectionMatrix);
            _objectFx.Parameters["lightPosition"].SetValue(_light.position);
            _objectFx.Parameters["lightRadius"].SetValue(new Vector3(500.0f));
            //objectFx.Parameters["shadowMapSize"].SetValue(shadowMapSize);

            for (int i = 0; i < _renderList.Count; i++)
            {
                _objectFx.Parameters["mainTexture"].SetValue(_renderList[i].Texture);
                _objectFx.Parameters["World"].SetValue(_renderList[i].SceneObject.Transform.world);
                _objectFx.CurrentTechnique.Passes[0].Apply();

                _renderList[i].DrawMesh(_device);
            }

            _device.SetRenderTarget(null);
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            int width = _device.Viewport.Width;
            int height = _device.Viewport.Height;

            float aspect = 1;
            int tileHeight = 100;
            int tileWidth = (int)(tileHeight * aspect);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.Draw(_shadowRT, new Rectangle(0, height - tileHeight, tileWidth, tileHeight), Color.White);
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
            renderShadows(camera);
            renderObjects(camera);
            renderBuffers();
        }
    }
}
