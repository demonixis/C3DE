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

        public int Amount { get; set; } = 40;

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
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            _graphics.SetRenderTarget(_sceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;
            _graphics.Textures[1] = _sceneRenderTarget;

            var textureSamplerTexelSize = new Vector4(1.0f / (float)renderTarget.Width, 1.0f / (float)renderTarget.Height, renderTarget.Width, renderTarget.Height);

            _effect.Parameters["Amount"].SetValue(Amount);
            _effect.Parameters["MainTextureTexelSize"].SetValue(textureSamplerTexelSize);

            DrawFullscreenQuad(spriteBatch, renderTarget, _sceneRenderTarget, _effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _sceneRenderTarget;

            var viewport = _graphics.Viewport;
            _graphics.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, _sceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
