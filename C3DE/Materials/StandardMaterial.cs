using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class StandardMaterial : Material
    {
        private Vector4 _diffuseColor;
        private Vector4 _emissiveColor;
        private Vector4 _specularColor;

        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector4(); }
        }

        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector4(); }
        }

        public float Shininess { get; set; }

        public StandardMaterial(Scene scene)
            : base(scene)
        {
            _diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _specularColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
            Shininess = 250.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/StandardEffect");
        }

        public override void PrePass()
        {
            // FIXME - Set these parameters at first time and only on change and only for this effet
            // I need to cache effects and not clone it.

            // FIXME - Please update just once per effect

            // Matrix and camera.
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);
            //effect.Parameters["ViewPosition"].SetValue(scene.MainCamera.camView);

            var light0 = scene.Lights[0]; // FIXME

            // Update shadow data.
            effect.Parameters["ShadowData"].SetValue(light0.shadowGenerator.Data);
            effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);

            // Light
            effect.Parameters["LightView"].SetValue(light0.viewMatrix);
            effect.Parameters["LightProjection"].SetValue(light0.projectionMatrix);
            effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
            effect.Parameters["LightDirection"].SetValue(light0.Direction);
            effect.Parameters["LightPosition"].SetValue(light0.SceneObject.Transform.Position);
            effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
            effect.Parameters["LightRange"].SetValue(light0.Range);
            effect.Parameters["LightFallOff"].SetValue((int)light0.FallOf);
            effect.Parameters["LightType"].SetValue((int)light0.Type);

            // Fog
            effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
            effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
        }

        public override void Pass(RenderableComponent renderable)
        {
            // Material properties.
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(_diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);

            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["RecieveShadow"].SetValue(renderable.RecieveShadow);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
