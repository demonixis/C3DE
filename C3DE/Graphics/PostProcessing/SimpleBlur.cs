using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class SimpleBlur : PostProcessPass
    {
        public float BlurDistance { get; set; } = 0;

        public SimpleBlur(GraphicsDevice graphics)
             : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/SimpleBlur");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["BlurDistance"].SetValue(BlurDistance);
        }
    }
}
