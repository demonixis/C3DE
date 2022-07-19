using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class ConvolutionFilter : PostProcessPass
    {
        public static float[] EdgeDetect0 = new float[9] { 1, 0, -1, 0, 0, 0, -1, 0, 1 };
        public static float[] EdgeDetect1 = new float[9] { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
        public static float[] EdgeDetect2 = new float[9] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
        public static float[] Sharpen = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
        public static float[] Emboss = new float[9] { -2, -1, 0, -1, 1, 1, 0, 1, 2 };
        public static float[] Gaussian = new float[9] { 0, 1, 0, 1, 1, 1, 0, 1, 0 };

        private Vector2 _screenSize;

        public float[] Kernel { get; set; } = EdgeDetect0;

        public ConvolutionFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            _effect = content.Load<Effect>("Shaders/PostProcessing/Convolution");
            _screenSize = new Vector2(Screen.Width, Screen.Height);
        }

        public override void SetupEffect()
        {
            _effect.Parameters["ScreenSize"].SetValue(_screenSize);
            _effect.Parameters["Kernel"].SetValue(Kernel);
        }
    }
}
