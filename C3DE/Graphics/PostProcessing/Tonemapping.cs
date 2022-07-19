using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class Tonemapping : PostProcessPass
    {
        public float Exposure { get; set; } = 1.0f;

        public Tonemapping(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/PostProcessing/Tonemapping");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["Exposure"].SetValue(Exposure);
        }
    }
}
