using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    /// <summary>
    /// An FXAA (Fast Approximate AntiAliasing) filter.
    /// Original Author: SeriousMaxx
    /// Link: https://github.com/SeriousMaxx/FXAAMonoGame
    /// </summary>
    public class FXAA : PostProcessPass
    {
        public enum FXAAQuality
        {
            Desktop = 0, Console
        }

        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private float consoleEdgeSharpness = 8.0f;
        private float consoleEdgeThreshold = 0.125f;
        private float consoleEdgeThresholdMin = 0.05f;
        private float fxaaQualitySubpix = 0.75f;
        private float fxaaQualityEdgeThreshold = 0.166f;
        private float fxaaQualityEdgeThresholdMin = 0.0833f;

        public FXAAQuality Quality { get; set; } = FXAAQuality.Console;

        public FXAA(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/FXAA");
            m_SceneRenderTarget = GetRenderTarget();
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            float w = m_SceneRenderTarget.Width;
            float h = m_SceneRenderTarget.Height;

            if (Quality == FXAAQuality.Console)
            {
                m_Effect.CurrentTechnique = m_Effect.Techniques["ppfxaa_Console"];
                m_Effect.Parameters["ConsoleOpt1"].SetValue(new Vector4(-2.0f / w, -2.0f / h, 2.0f / w, 2.0f / h));
                m_Effect.Parameters["ConsoleOpt2"].SetValue(new Vector4(8.0f / w, 8.0f / h, -4.0f / w, -4.0f / h));
                m_Effect.Parameters["ConsoleEdgeSharpness"].SetValue(consoleEdgeSharpness);
                m_Effect.Parameters["ConsoleEdgeThreshold"].SetValue(consoleEdgeThreshold);
                m_Effect.Parameters["ConsoleEdgeThresholdMin"].SetValue(consoleEdgeThresholdMin);
            }
            else
            {
                m_Effect.CurrentTechnique = m_Effect.Techniques["ppfxaa_PC"];
                m_Effect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                m_Effect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                m_Effect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
            }

            m_Effect.Parameters["invViewportWidth"].SetValue(1f / w);
            m_Effect.Parameters["invViewportHeight"].SetValue(1f / h);
            m_Effect.Parameters["texScreen"].SetValue(sceneRT);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }
    }
}
