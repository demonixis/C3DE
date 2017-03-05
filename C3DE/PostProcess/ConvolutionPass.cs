using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcess
{
    public class ConvolutionPass : PostProcessPass
    {
        public static float[] EdgeDetect0 = new float[9] { 1, 0, -1, 0, 0, 0, -1, 0, 1 };
        public static float[] EdgeDetect1 = new float[9] { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
        public static float[] EdgeDetect2 = new float[9] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
        public static float[] Sharpen = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
        public static float[] Emboss = new float[9] { -2, -1, 0, -1, 1, 1, 0, 1, 2 };
        public static float[] Gaussian = new float[9] { 0, 1, 0, 1, 1, 1, 0, 1, 0 };
	
        private Effect _covolutionEffect;
		private Vector2 _screenSize;

		public float[] Kernel { get; set; }

        public ConvolutionPass() 
			: base() 
		{ 
			Kernel = EdgeDetect0;
		}

        public override void Initialize(ContentManager content)
        {
            _covolutionEffect = content.Load<Effect>("FX/PostProcess/Convolution");
			_screenSize = new Vector2(Screen.Width, Screen.Height);

            /*if (Application.Engine.VREnabled)
                _screenSize.X *= 0.5f;*/
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _covolutionEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _covolutionEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _covolutionEffect.Parameters["ScreenSize"].SetValue(_screenSize);
            _covolutionEffect.Parameters["Kernel"].SetValue(Kernel);
            _covolutionEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
