using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A generator of shadow for a specified light.
    /// </summary>
    public class ShadowGenerator : IDisposable
    {
        private Light _light;
        private RenderTarget2D shadowMap;
        private Effect _shadowEffect;
        private BoundingSphere _boundingSphere;
        private Vector3 _shadowData;

        public RenderTarget2D ShadowMap
        {
            get { return shadowMap; }
        }

        public float ShadowMapSize
        {
            get { return _shadowData.X; }
            set { _shadowData.X = value; }
        }

        public float ShadowBias
        {
            get { return _shadowData.Y; }
            set { _shadowData.Y = value; }
        }

        public float ShadowStrength
        {
            get { return 1 - _shadowData.Z; }
            set { _shadowData.Z = Math.Min(1.0f, Math.Max(0.0f, value)); }
        }

        // FIXME
        public bool Enabled
        {
            get { return _shadowData.X > 0; }
            set { _shadowData.X = value ? Math.Max(_shadowData.X, 256) : 0; }
        }

        public Vector3 Data
        {
            get { return _shadowData; }
        }

        public ShadowGenerator(Light light)
        {
            _light = light;
            _shadowData = new Vector3(0, 0.005f, 0.8f);
        }

        public void Initialize()
        {
            _shadowEffect = Application.Content.Load<Effect>("FX/ShadowMapEffect");
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
            _shadowData.X = size;
        }

        /// <summary>
        /// Render shadows for the specified camera into a renderTarget.
        /// </summary>
        /// <param name="camera"></param>
        public void RenderShadows(GraphicsDevice device, List<Renderer> renderList)
        {
            _boundingSphere = new BoundingSphere();

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
            device.DepthStencilState = DepthStencilState.Default;
            device.Clear(Color.White);

            _shadowEffect.Parameters["View"].SetValue(_light.viewMatrix);
            _shadowEffect.Parameters["Projection"].SetValue(_light.projectionMatrix);

            for (int i = 0; i < renderList.Count; i++)
            {
                if (renderList[i].CastShadow)
                {
                    _shadowEffect.Parameters["World"].SetValue(renderList[i].SceneObject.Transform.world);
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

        public string Serialize()
        {
            return SerializerHelper.ToString(_shadowData);
        }

        public void Deserialize(string strData)
        {
            _shadowData = SerializerHelper.ToVector3(strData);
        }
    }
}
