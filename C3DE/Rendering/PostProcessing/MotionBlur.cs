using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace C3DE.Rendering.PostProcessing
{
    public class MotionBlur : SimpleBlur
    {
        public MotionBlur(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            var targetValue = 0.0f;

            if (Input.Mouse.Drag())
                targetValue = (Input.Mouse.Delta.X + Input.Mouse.Delta.Y) * Time.DeltaTime * 0.05f;

            BlurDistance = MathHelper.Lerp(BlurDistance, targetValue, Time.DeltaTime * 5.0f);
            base.Draw(spriteBatch, sceneRT);
        }
    }
}
