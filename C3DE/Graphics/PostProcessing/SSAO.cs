using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class SSAO : PostProcessPass
    {
        private Effect _effect;
        private RenderTarget2D _sceneRenderTarget;
        private RenderTarget2D _ssaoTarget;
        private RenderTarget2D _depthBuffer;
        private QuadRenderer _quadRenderer;

        public RenderTarget2D SSAOTexture => _ssaoTarget;

        public SSAO(GraphicsDevice graphics) : base(graphics)
        {
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            _sceneRenderTarget.Dispose();
            _sceneRenderTarget = GetRenderTarget();
        }

        public override void Initialize(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/PostProcessing/SSAO");
            _sceneRenderTarget = GetRenderTarget();
            _ssaoTarget = GetRenderTarget();
            _quadRenderer = new QuadRenderer(_graphics);

            var renderer = Application.Engine.Renderer;
            _depthBuffer = renderer.GetDepthBuffer();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            _graphics.SetRenderTarget(_ssaoTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            var viewport = Application.GraphicsDevice.Viewport;
            var halfPixel = -new Vector2(0.5f / (float)viewport.Width, 0.5f / (float)viewport.Height);
            var viewportSize = new Vector4(viewport.Width, viewport.Height, 1.0f / viewport.Width, 1.0f / viewport.Height);

            _effect.Parameters["HalfPixel"].SetValue(halfPixel);
            _effect.Parameters["ViewportSize"].SetValue(viewportSize);
            _effect.Parameters["MainTexture"].SetValue(sceneRT);
            _effect.Parameters["SecondaryMap"].SetValue(_depthBuffer);

            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();

            _graphics.SetRenderTarget(_sceneRenderTarget);

            _effect.Parameters["MainTexture"].SetValue(sceneRT);
            _effect.Parameters["SecondaryMap"].SetValue(_ssaoTarget);
            _effect.CurrentTechnique.Passes[1].Apply();
            _quadRenderer.RenderFullscreenQuad();

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _sceneRenderTarget;

            _graphics.SetRenderTarget(sceneRT);
            DrawFullscreenQuad(spriteBatch, _sceneRenderTarget, _sceneRenderTarget.Width, _sceneRenderTarget.Height, null);
        }
    }
}
