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

        public Vector2 Tiling { get; set; }

        public TerrainMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Tiling = Vector2.One;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/TerrainEffect").Clone();
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            var light0 = scene.lights[0];

            // Light
            effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
            effect.Parameters["LightDirection"].SetValue(light0.Direction);
            effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
            
            // Material
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["SnowTexture"].SetValue(SnowTexture);
            effect.Parameters["SandTexture"].SetValue(SandTexture);
            effect.Parameters["RockTexture"].SetValue(RockTexture);
            effect.Parameters["WeightTexture"].SetValue(WeightTexture);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
