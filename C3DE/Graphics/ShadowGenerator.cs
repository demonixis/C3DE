using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE.Graphics
{
    /// <summary>
    /// A generator of shadow for a specified light.
    /// </summary>
    [DataContract]
    public class ShadowGenerator : IDisposable
    {
        private RenderTarget2D _shadowMap;
        private Effect _shadowEffect;
        private BoundingSphere _boundingSphere;

        [DataMember]
        protected internal Vector3 _shadowData;

        public RenderTarget2D ShadowMap => _shadowMap;

        public int ShadowMapSize
        {
            get => (int)_shadowData.X;
            set
            {
                if (value > 0)
                    SetShadowMapSize(Application.GraphicsDevice, value);

                _shadowData.X = value;
            }
        }

        public float ShadowBias
        {
            get => _shadowData.Y;
            set => _shadowData.Y = value;
        }

        public float ShadowStrength
        {
            get => _shadowData.Z;
            set => _shadowData.Z = value;
        }

        // FIXME
        public bool Enabled
        {
            get => _shadowData.X > 0;
            set => _shadowData.X = value ? Math.Max(_shadowData.X, 256) : 0;
        }

        public Vector3 Data => _shadowData;

        public ShadowGenerator()
        {
            _shadowData = new Vector3(0, 0.005f, 0.8f);
        }

        public void Initialize()
        {
            _shadowEffect = Application.Content.Load<Effect>("Shaders/ShadowMap");
        }

        /// <summary>
        /// Change the shadow map size and update the render target.
        /// </summary>
        /// <param name="size">Desired size, it must a power of two</param>
        public void SetShadowMapSize(GraphicsDevice device, int size)
        {
#if ANDROID
            _shadowMap = new RenderTarget2D (device, size, size);
#else
            _shadowMap = new RenderTarget2D(device, size, size, false, SurfaceFormat.Single, DepthFormat.Depth24, 2, RenderTargetUsage.DiscardContents);
#endif
            _shadowData.X = size;
        }

        /// <summary>
        /// Render shadows for the specified camera into a renderTarget.
        /// </summary>
        /// <param name="camera"></param>
        public void RenderShadows(GraphicsDevice device, List<Renderer> renderList, Light light)
        {
            _boundingSphere.Center = Vector3.Zero;
            _boundingSphere.Radius = 0.0f;

            if (renderList.Count > 0)
            {
                for (int i = 0; i < renderList.Count; i++)
                {
                    if (renderList[i].Enabled && renderList[i].CastShadow)
                        _boundingSphere = BoundingSphere.CreateMerged(_boundingSphere, renderList[i].boundingSphere);
                }

                light.Update(ref _boundingSphere);
            }

            var currentRenderTargets = device.GetRenderTargets();

            device.SetRenderTarget(_shadowMap);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.Clear(Color.White);

            _shadowEffect.Parameters["View"].SetValue(light._viewMatrix);
            _shadowEffect.Parameters["Projection"].SetValue(light._projectionMatrix);

            for (int i = 0; i < renderList.Count; i++)
            {
                if (renderList[i].CastShadow)
                {
                    _shadowEffect.Parameters["World"].SetValue(renderList[i]._transform._worldMatrix);
                    _shadowEffect.CurrentTechnique.Passes[0].Apply();
                    renderList[i].Draw(device);
                }
            }

            device.SetRenderTargets(currentRenderTargets);
        }

        public void Dispose()
        {
            if (_shadowMap != null)
                _shadowMap.Dispose();
        }
    }
}
