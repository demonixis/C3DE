using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardMaterial : Material, IMultipassLightingMaterial, IEmissiveMaterial
    {
        private Vector3 _diffuseColor;
        private Vector3 _emissiveColor;
        private Vector3 _specularColor;

        public Texture2D EmissiveTexture { get; set; }

        public TextureCube ReflectionTexture { get; set; }

        [DataMember]
        public float ReflectionIntensity { get; set; } = 0.35f;

        public Texture2D SpecularTexture { get; set; }

        [DataMember]
        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector3(); }
        }

        [DataMember]
        public float Shininess { get; set; }

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        [DataMember]
        public bool EmissiveEnabled { get; set; } = false;

        [DataMember]
        public float EmissiveIntensity { get; set; } = 1.0f;

        public StandardMaterial(Scene scene, string name = "Standard Material")
            : base(scene)
        {
            _diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(1.0f, 1.0f, 1.0f);
            _specularColor = new Vector3(0.6f, 0.6f, 0.6f);
            Shininess = 250.0f;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            if (ShaderQuality == ShaderQuality.Low)
                effect = content.Load<Effect>("Shaders/StandardEffect.Low");
            else
                effect = content.Load<Effect>("Shaders/StandardEffect");
        }

        public override void PrePass(Camera camera)
        {
            // Matrix and camera.
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["DiffuseColor"].SetValue(_diffuseColor);
            effect.Parameters["MainTexture"].SetValue(MainTexture);

            if (ShaderQuality == ShaderQuality.Normal)
            {
                effect.Parameters["ReflectionTexture"].SetValue(ReflectionTexture);
                effect.Parameters["ReflectionEnabled"].SetValue(ReflectionTexture != null ? 1 : 0);
                effect.Parameters["ReflectionIntensity"].SetValue(ReflectionIntensity);
            }

            effect.CurrentTechnique.Passes["AmbientPass"].Apply();
        }

        public void LightPass(Renderer renderer, Light light)
        {
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["LightColor"].SetValue(light.color);
            effect.Parameters["LightDirection"].SetValue(light.transform.Rotation);
            effect.Parameters["LightPosition"].SetValue(light.transform.Position);
            effect.Parameters["LightIntensity"].SetValue(light.Intensity);
            effect.Parameters["LightRange"].SetValue(light.Range);
            effect.Parameters["LightFallOff"].SetValue((int)light.FallOf);
            effect.Parameters["LightType"].SetValue((int)light.TypeLight);

            if (ShaderQuality == ShaderQuality.Normal)
            {
                effect.Parameters["ShadowData"].SetValue(light.shadowGenerator.shadowData);
                effect.Parameters["ShadowMap"].SetValue(light.shadowGenerator.ShadowMap);
                effect.Parameters["RecieveShadow"].SetValue(light.ShadowGenerator.Enabled);
                effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
                effect.Parameters["LightView"].SetValue(light.viewMatrix);
                effect.Parameters["LightProjection"].SetValue(light.projectionMatrix);
                effect.Parameters["SpecularTextureEnabled"].SetValue(SpecularTexture != null ? 1 : 0);
                effect.Parameters["SpecularTexture"].SetValue(SpecularTexture);
            }

            effect.CurrentTechnique.Passes["LightPass"].Apply();
        }

        public bool EmissivePass(Renderer renderer)
        {
            effect.Parameters["EmissiveTextureEnabled"].SetValue(EmissiveTexture != null ? 1 : 0);
            effect.Parameters["EmissiveTexture"].SetValue(EmissiveTexture);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["EmissiveIntensity"].SetValue(EmissiveIntensity);
            effect.CurrentTechnique.Passes["EmissivePass"].Apply();
            return true;
        }
    }
}
