using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Materials
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
                effect = content.Load<Effect>("FX/TerrainEffect.Low");
            else
                effect = content.Load<Effect>("FX/TerrainEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);

            if (scene.lights.Count > 0)
            {
                var light0 = scene.lights[0];

                // Light
                effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
                effect.Parameters["LightDirection"].SetValue(light0.Direction);
                effect.Parameters["LightIntensity"].SetValue(light0.Intensity);

                // Update shadow data.
                effect.Parameters["ShadowData"].SetValue(light0.shadowGenerator.shadowData);
                effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);
            }

            if (ShaderQuality == ShaderQuality.Normal)
            {
                // Fog
                effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
            }

            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["SnowTexture"].SetValue(SnowTexture);
            effect.Parameters["SandTexture"].SetValue(SandTexture);
            effect.Parameters["RockTexture"].SetValue(RockTexture);
            effect.Parameters["WeightMap"].SetValue(WeightTexture);
            effect.Parameters["RecieveShadow"].SetValue(renderable.ReceiveShadow);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
