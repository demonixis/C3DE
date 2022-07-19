using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class GrayScaleFilter : PostProcessPass
    {
        public GrayScaleFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/GrayScale");
        }
    }
}
