using C3DE.Components;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE
{
    [DataContract]
    public class Skybox
    {
        private static Effect m_Effect = null;
        private Matrix m_World;
        private Matrix _scaleMatrix;
        private CubeMesh m_Geometry;
        private TextureCube m_MainTexture;
        private RasterizerState m_SkyboxRasterizerState;
        private RasterizerState m_CurrentRasterizerState;
        private Vector4 m_CustomFogData;
        private bool m_OverrideFog;

        private EffectPass m_DefaultPass;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPFogEnabled;
        protected EffectParameter m_EPFogColor;
        protected EffectParameter m_EPFogData;

        public TextureCube Texture
        {
            get { return m_MainTexture; }
            set { m_MainTexture = value; }
        }

        [DataMember]
        public bool FogSupported { get; set; } = false;

        [DataMember]
        public bool Enabled { get; set; }

        public Skybox()
        {
            m_Geometry = new CubeMesh();
            m_World = Matrix.Identity;
            _scaleMatrix = Matrix.CreateScale(1.0f);
            m_SkyboxRasterizerState = new RasterizerState();
            m_SkyboxRasterizerState.CullMode = CullMode.None;
        }

        public void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/Skybox");
            m_DefaultPass = m_Effect.CurrentTechnique.Passes["AmbientPass"];
            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPEyePosition = m_Effect.Parameters["EyePosition"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPFogEnabled = m_Effect.Parameters["FogEnabled"];
            m_EPFogColor = m_Effect.Parameters["FogColor"];
            m_EPFogData = m_Effect.Parameters["FogData"];
        }

        public void OverrideSkyboxFog(FogMode mode, float density, float start, float end)
        {
            m_CustomFogData.X = (float)mode;
            m_CustomFogData.Y = density;
            m_CustomFogData.Z = start;
            m_CustomFogData.W = end;
            m_OverrideFog = mode != FogMode.None;
        }

        public void Generate(GraphicsDevice device, ContentManager content, Texture2D[] textures, float size = 250.0f)
        {
            if (textures.Length != 6)
                throw new Exception("The array of texture names must contains 6 elements.");

            m_Geometry.Size = new Vector3(size);
            m_Geometry.Build();
            m_Geometry.ComputeNormals();

            m_MainTexture = new TextureCube(device, textures[0].Width, false, SurfaceFormat.Color);
            Color[] textureData;

            for (int i = 0; i < 6; i++)
            {
                textureData = new Color[textures[i].Width * textures[i].Height];
                textures[i].GetData<Color>(textureData);
                m_MainTexture.SetData<Color>((CubeMapFace)i, textureData);
            }

            Enabled = true;
        }

        public void Generate(GraphicsDevice device, ContentManager content, string[] textureNames, float size = 250.0f)
        {
            var textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
                textures[i] = content.Load<Texture2D>(textureNames[i]);

            Generate(device, content, textures, size);
        }

        public void Generate()
        {
            var skyTop = GraphicsHelper.CreateTexture(new Color(168, 189, 255), 64, 64);
            var skySide = GraphicsHelper.CreateGradiantTexture(new Color(168, 189, 255), Color.White, 64, 64);
            var skyBottom = GraphicsHelper.CreateTexture(Color.White, 64, 64);

            Generate(Application.GraphicsDevice, Application.Content, new Texture2D[] {
                skySide,
                skySide,
                skyTop,
                skyBottom,
                skySide,
                skySide
            }, 1000.0f);
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            m_CurrentRasterizerState = device.RasterizerState;
            device.RasterizerState = m_SkyboxRasterizerState;

            m_World = _scaleMatrix * Matrix.CreateTranslation(camera.Transform.LocalPosition);

            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPMainTexture.SetValue(m_MainTexture);
            m_EPWorld.SetValue(m_World);
#if !DESKTOP
            m_EPFogEnabled.SetValue(FogSupported);
            m_EPFogColor.SetValue(Scene.current.RenderSettings.fogColor);
            m_EPFogData.SetValue(m_OverrideFog ? m_CustomFogData : Scene.current.RenderSettings.fogData);
#endif
            m_DefaultPass.Apply();

            device.SetVertexBuffer(m_Geometry.VertexBuffer);
            device.Indices = m_Geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_Geometry.Indices.Length / 3);
            device.RasterizerState = m_CurrentRasterizerState;
        }

        public void DrawDeferred(GraphicsDevice device, Camera camera)
        {
            if (m_Effect.Name == "Shaders/Forward/Skybox")
            {
                m_Effect.Dispose();
                m_Effect = Application.Content.Load<Effect>("Shaders/Deferred/Skybox");
            }

            m_World = _scaleMatrix * Matrix.CreateTranslation(camera.Transform.LocalPosition);

            m_Effect.Parameters["World"].SetValue(m_World);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Texture"].SetValue(m_MainTexture);
            m_Effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            m_Effect.CurrentTechnique.Passes[0].Apply();

            device.SetVertexBuffer(m_Geometry.VertexBuffer);
            device.Indices = m_Geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_Geometry.Indices.Length / 3);
        }
    }
}
