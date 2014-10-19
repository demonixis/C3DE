using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class TerrainMaterial : Material
    {
        public Texture2D SnowTexture { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D WeightTexture { get; set; }

        public TerrainMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Tiling = Vector2.One;
        }

        public override void LoadContent(ContentManager content)
        {
#if ANDROID
            effect = content.Load<Effect>("FX/Android/TerrainEffect");
#else
            effect = content.Load<Effect>("FX/TerrainEffect");
#endif
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);

            var light0 = scene.lights[0]; 

            // Light
            effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
            effect.Parameters["LightDirection"].SetValue(light0.Direction);
            effect.Parameters["LightIntensity"].SetValue(light0.Intensity);

            // Update shadow data.
            effect.Parameters["ShadowData"].SetValue(light0.shadowGenerator.Data);
            effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);

#if !ANDROID
            // Fog
            effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
            effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
#endif
        }

        public override void Pass(RenderableComponent renderable)
        {
            // Material
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);

            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["SnowTexture"].SetValue(SnowTexture);
            effect.Parameters["SandTexture"].SetValue(SandTexture);
            effect.Parameters["RockTexture"].SetValue(RockTexture);
            effect.Parameters["WeightMap"].SetValue(WeightTexture);
            effect.Parameters["RecieveShadow"].SetValue(renderable.ReceiveShadow);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
