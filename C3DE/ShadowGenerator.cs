using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A generator of shadow for a specified light.
    /// </summary>
    public class ShadowGenerator
    {
        internal RenderTarget2D _shadowRT;
        private GraphicsDevice _device;
        private Effect _shadowEffect;
        private float _shadowMapSize;
        private float _shadowBias;
        private float _shadowStrength;
        private BoundingSphere _boundingSphere;
        private bool _enabled;

        public RenderTarget2D ShadowRT
        {
            get { return _shadowRT; }
        }

        public float ShadowMapSize
        {
            get { return _shadowMapSize; }
        }

        public float ShadowBias
        {
            get { return _shadowBias; }
            set { _shadowBias = value; }
        }

        public float ShadowStrength
        {
            get { return 1 - _shadowStrength; }
            set { _shadowStrength = Math.Min(1.0f, Math.Max(0.0f, value)); }
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
            _shadowBias = 0.0015f;
            _shadowStrength = 0.5f;
            SetShadowMapSize(1024);
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
        /// Render shadows for the specified camera into a renderTarget.
        /// </summary>
        /// <param name="camera"></param>
        public void renderShadows(List<RenderableComponent> renderList, LightPrefab light)
        {
            _boundingSphere = new BoundingSphere();

            if (renderList.Count > 0)
            {
                for (int i = 0; i < renderList.Count; i++)
                {
                    if (renderList[i].CastShadow)
                        _boundingSphere = BoundingSphere.CreateMerged(_boundingSphere, renderList[i].BoundingSphere);
                }

                light.Update(ref _boundingSphere);
            }

            _device.SetRenderTarget(_shadowRT);
            _device.DepthStencilState = DepthStencilState.Default;
            _device.Clear(Color.White);

            _shadowEffect.Parameters["View"].SetValue(light.viewMatrix);
            _shadowEffect.Parameters["Projection"].SetValue(light.projectionMatrix);

            for (int i = 0; i < renderList.Count; i++)
            {
                if (!renderList[i].CastShadow)
                    continue;

                _shadowEffect.Parameters["World"].SetValue(renderList[i].SceneObject.Transform.world);
                _shadowEffect.CurrentTechnique.Passes[0].Apply();
                renderList[i].Draw(_device);
            }

            _device.SetRenderTarget(null);
        }
    }
}
