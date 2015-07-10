using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class StandardMaterial : Material
    {
        private Vector3 _diffuseColor;
        private Vector3 _emissiveColor;
        private Vector3 _specularColor;

        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector3(); }
        }

        public float Shininess { get; set; }

        public StandardMaterial(Scene scene, string name = "Standard Material")
            : base(scene)
        {
            _diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
            _specularColor = new Vector3(0.6f, 0.6f, 0.6f);
            Shininess = 250.0f;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            if (ShaderQuality == ShaderQuality.Low)
                effect = content.Load<Effect>("FX/StandardEffect.Low");
            else
                effect = content.Load<Effect>("FX/StandardEffect");
        }

        public override void PrePass(Camera camera)
        {
            // Matrix and camera.
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);

            if (scene.lights.Count > 0)
            {
                var light0 = scene.Lights[0]; // FIXME

                if (ShaderQuality == ShaderQuality.Normal)
                {
                    // Update shadow data.
                    effect.Parameters["ShadowData"].SetValue(light0.shadowGenerator.Data);
                    effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);

                    // Fog
                    effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                    effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);

                    // Light
                    effect.Parameters["LightView"].SetValue(light0.viewMatrix);
                    effect.Parameters["LightProjection"].SetValue(light0.projectionMatrix);
                }

                // Lighting infos.
                effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
                effect.Parameters["LightDirection"].SetValue(light0.Direction);
                effect.Parameters["LightPosition"].SetValue(light0.Transform.Position);
                effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
                effect.Parameters["LightRange"].SetValue(light0.Range);
                effect.Parameters["LightFallOff"].SetValue((int)light0.FallOf);
                effect.Parameters["LightType"].SetValue((int)light0.TypeLight);
            }
        }

        public override void Pass(Renderer renderable)
        {
            if (ShaderQuality == Materials.ShaderQuality.Normal)
                effect.Parameters["RecieveShadow"].SetValue(renderable.ReceiveShadow);

            // Material properties.
            effect.Parameters["DiffuseColor"].SetValue(_diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["World"].SetValue(renderable.Transform.world);

            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
