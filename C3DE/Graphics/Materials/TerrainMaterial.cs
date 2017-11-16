using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class TerrainMaterial : Material
    {
        public Texture2D SnowTexture { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D WeightTexture { get; set; }

        public TerrainMaterial(Scene scene, string name = "Terrain Material")
            : base(scene)
        {
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            Tiling = Vector2.One;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            if (ShaderQuality == ShaderQuality.Low)
                m_Effect = content.Load<Effect>("Shaders/TerrainEffect.Low");
            else
                m_Effect = content.Load<Effect>("Shaders/TerrainEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection);
            m_Effect.Parameters["EyePosition"].SetValue(camera.Transform.LocalPosition);

            if (scene.lights.Count > 0)
            {
                var light0 = scene.lights[0];

                // Light
                m_Effect.Parameters["LightColor"].SetValue(light0.color);
                m_Effect.Parameters["LightDirection"].SetValue(light0.transform.LocalRotation);
                m_Effect.Parameters["LightIntensity"].SetValue(light0.Intensity);

                // Update shadow data.
                m_Effect.Parameters["ShadowData"].SetValue(light0.shadowGenerator.shadowData);
                m_Effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);
            }

            if (ShaderQuality == ShaderQuality.Normal)
            {
                // Fog
                m_Effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                m_Effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
            }

            m_Effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            m_Effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["SnowTexture"].SetValue(SnowTexture);
            m_Effect.Parameters["SandTexture"].SetValue(SandTexture);
            m_Effect.Parameters["RockTexture"].SetValue(RockTexture);
            m_Effect.Parameters["WeightMap"].SetValue(WeightTexture);
            m_Effect.Parameters["RecieveShadow"].SetValue(renderable.ReceiveShadow);
            m_Effect.Parameters["World"].SetValue(renderable.Transform.world);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
