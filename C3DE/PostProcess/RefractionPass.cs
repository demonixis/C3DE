using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcess
{
    public class RefractionPass : PostProcessPass
    {
        private Effect _refractionEffect;
        private float _colorLevel;
        private float _depth;
        private Vector2 _tiling;
        private Texture2D _refractionTexture;

        public float ColorLevel
        {
            get { return _colorLevel; }
            set { _colorLevel = value; }
        }

        public float Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public Texture2D RefractionTexture
        {
            get { return _refractionTexture; }
            set
            {
                _refractionTexture = value;
                Enabled = _refractionTexture != null;
            }
        }

        public Vector2 TextureTiling
        {
            get { return _tiling; }
            set { _tiling = value; }
        }

        public RefractionPass() : this(null) { }

        public RefractionPass(Texture2D refractionTexture)
        {
            RefractionTexture = refractionTexture;
            _colorLevel = 0.5f;
            _depth = 0.5f;
            _tiling = Vector2.One;
        }

        public override void Initialize(ContentManager content)
        {
            _refractionEffect = content.Load<Effect>("FX/PostProcess/Refraction");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _refractionEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _refractionEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _refractionEffect.Parameters["RefractionTexture"].SetValue(_refractionTexture);
            _refractionEffect.Parameters["ColorLevel"].SetValue(_colorLevel);
            _refractionEffect.Parameters["Depth"].SetValue(_depth);
            _refractionEffect.Parameters["TextureTiling"].SetValue(_tiling);
            _refractionEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
