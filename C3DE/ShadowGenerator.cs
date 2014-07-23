using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE
{
    public class ShadowGenerator
    {
        internal RenderTarget2D _shadowRT;
        private GraphicsDevice _device;
        private Effect _shadowEffect;
        private float _shadowMapSize;
        private BoundingSphere _boundingSphere;
        private List<ModelRenderer> _renderList;
        private Light _light;
        private bool _enabled;

        public List<ModelRenderer> RenderList
        {
            get { return _renderList; }
            internal set { _renderList = value; }
        }

        public Light Light
        {
            get { return _light; }
            internal set { _light = value; }
        }

        public RenderTarget2D ShadowRT
        {
            get { return _shadowRT; }
        }

        public float ShadowMapSize
        {
            get { return _shadowMapSize; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public ShadowGenerator(GraphicsDevice device)
        {
            _device = device;
            _enabled = true;
            SetShadowMapSize(512);
        }

        public void LoadContent(ContentManager content)
        {
            _shadowEffect = content.Load<Effect>("fx/ShadowMapEffect");
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
        public void renderShadows(Camera camera)
        {
            _boundingSphere = new BoundingSphere();

            if (_renderList.Count > 0)
            {
                for (int i = 0; i < _renderList.Count; i++)
                {
                    if (_renderList[i].CastShadow)
                        _boundingSphere = BoundingSphere.CreateMerged(_boundingSphere, _renderList[i].GetBoundingSphere());
                }

                _light.Update(ref _boundingSphere);
            }

            _device.SetRenderTarget(_shadowRT);
            _device.DepthStencilState = DepthStencilState.Default;
            _device.Clear(Color.White);

            _shadowEffect.Parameters["View"].SetValue(_light.viewMatrix);
            _shadowEffect.Parameters["Projection"].SetValue(_light.projectionMatrix);

            for (int i = 0; i < _renderList.Count; i++)
            {
                if (!_renderList[i].CastShadow)
                    continue;

                _shadowEffect.Parameters["World"].SetValue(_renderList[i].SceneObject.Transform.world);
                _shadowEffect.CurrentTechnique.Passes[0].Apply();
                _renderList[i].DrawMesh(_device);
            }

            _device.SetRenderTarget(null);
        }
    }
}
