using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class SuperMaterial : Material
    {
        private Matrix _worldInvertTranspose;
        private Vector3 _shadowData;
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

        public SuperMaterial(Scene scene)
            : base(scene)
        {
            _diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
            _specularColor = new Vector3(0.8f, 0.8f, 0.8f);
            Shininess = 200.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/StandardEffect").Clone();
        }

        public override void PrePass()
        {
            // Matrix and camera.
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);

            // Material properties.
            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector3());
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);

            var light0 = scene.Lights[0]; // FIXME

            // Update shadow data.
            _shadowData.X = light0.shadowGenerator.Enabled ? light0.shadowGenerator.ShadowMapSize : 0;
            _shadowData.Y = light0.shadowGenerator.ShadowBias;
            _shadowData.Z = light0.shadowGenerator.ShadowStrength;

            effect.Parameters["ShadowData"].SetValue(_shadowData);

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
        }

        public override void Pass(RenderableComponent renderable)
        {
            _worldInvertTranspose = Matrix.Transpose(Matrix.Invert(renderable.SceneObject.Transform.world));
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["RecieveShadows"].SetValue(renderable.RecieveShadow);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.Parameters["WorldInverseTranspose"].SetValue(_worldInvertTranspose);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
