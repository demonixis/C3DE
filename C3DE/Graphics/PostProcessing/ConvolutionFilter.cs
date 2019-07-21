using C3DE.VR;
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

        private Effect m_Effect;
        private Vector2 m_ScreenSize;
        private RenderTarget2D m_SceneRenderTarget;

        public float[] Kernel { get; set; } = EdgeDetect0;

        public ConvolutionFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/Convolution");
            m_ScreenSize = new Vector2(Screen.Width, Screen.Height);
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            _graphics.SetRenderTarget(m_SceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["ScreenSize"].SetValue(m_ScreenSize);
            m_Effect.Parameters["Kernel"].SetValue(Kernel);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = m_SceneRenderTarget;
            _graphics.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }
    }
}
