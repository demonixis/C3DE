using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE.Components.Lighting
{
    /// <summary>
    /// A generator of shadow for a specified light.
    /// </summary>
    [DataContract]
    public class ShadowGenerator : IDisposable
    {
        private Light _light;
        private RenderTarget2D shadowMap;
        private Effect _shadowEffect;
        private BoundingSphere _boundingSphere;

        [DataMember]
        protected internal Vector3 shadowData;

        public RenderTarget2D ShadowMap
        {
            get { return shadowMap; }
        }

        public int ShadowMapSize
        {
            get { return (int)shadowData.X; }
            set
            {
                if (value > 0)
                    SetShadowMapSize(Application.GraphicsDevice, value);

                shadowData.X = value;
            }
        }

        public float ShadowBias
        {
            get { return shadowData.Y; }
            set { shadowData.Y = value; }
        }

        public float ShadowStrength
        {
            get { return shadowData.Z; }
            set { shadowData.Z = value; }
        }

        // FIXME
        public bool Enabled
        {
            get { return shadowData.X > 0; }
            set { shadowData.X = value ? Math.Max(shadowData.X, 256) : 0; }
        }

        public Vector3 Data
        {
            get { return shadowData; }
        }

        public ShadowGenerator(Light light)
        {
            _light = light;
            shadowData = new Vector3(0, 0.005f, 0.8f);
        }

        public void Initialize()
        {
            _shadowEffect = Application.Content.Load<Effect>("Shaders/ShadowMapEffect");
        }

        /// <summary>
        /// Change the shadow map size and update the render target.
        /// </summary>
        /// <param name="size">Desired size, it must a power of two</param>
        public void SetShadowMapSize(GraphicsDevice device, int size)
        {
#if ANDROID
			shadowMap = new RenderTarget2D (device, size, size);
#else
            shadowMap = new RenderTarget2D(device, size, size, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
#endif
            shadowData.X = size;
        }

        /// <summary>
        /// Render shadows for the specified camera into a renderTarget.
        /// </summary>
        /// <param name="camera"></param>
        public void RenderShadows(GraphicsDevice device, List<Renderer> renderList)
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

                _light.Update(ref _boundingSphere);
            }

            var currentRenderTargets = device.GetRenderTargets();

            device.SetRenderTarget(shadowMap);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.Clear(Color.White);

            _shadowEffect.Parameters["View"].SetValue(_light.viewMatrix);
            _shadowEffect.Parameters["Projection"].SetValue(_light.projectionMatrix);

            for (int i = 0; i < renderList.Count; i++)
            {
                if (renderList[i].CastShadow)
                {
                    _shadowEffect.Parameters["World"].SetValue(renderList[i].m_Transform.m_WorldMatrix);
                    _shadowEffect.CurrentTechnique.Passes[0].Apply();
                    renderList[i].Draw(device);
                }
            }

            device.SetRenderTargets(currentRenderTargets);
        }

        public void Dispose()
        {
            if (shadowMap != null)
                shadowMap.Dispose();
        }
    }
}
