using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Rendering.PostProcessing
{
    public class ConvolutionFilter : PostProcessPass
    {
        public static float[] EdgeDetect0 = new float[9] { 1, 0, -1, 0, 0, 0, -1, 0, 1 };
        public static float[] EdgeDetect1 = new float[9] { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
        public static float[] EdgeDetect2 = new float[9] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
        public static float[] Sharpen = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
        public static float[] Emboss = new float[9] { -2, -1, 0, -1, 1, 1, 0, 1, 2 };
        public static float[] Gaussian = new float[9] { 0, 1, 0, 1, 1, 1, 0, 1, 0 };
	
        private Effect m_Effect;
		private Vector2 m_ScreenSize;
        private RenderTarget2D m_SceneRenderTarget;

        public float[] Kernel { get; set; } = EdgeDetect0;

        public ConvolutionFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/Convolution");
			m_ScreenSize = new Vector2(Screen.Width, Screen.Height);
            m_SceneRenderTarget = GetRenderTarget();

            /*if (Application.Engine.VREnabled)
                _screenSize.X *= 0.5f;*/
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["ScreenSize"].SetValue(m_ScreenSize);
            m_Effect.Parameters["Kernel"].SetValue(Kernel);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
