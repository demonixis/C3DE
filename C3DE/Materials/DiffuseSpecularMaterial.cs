using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class DiffuseSpecularMaterial : Material
    {
        private Matrix _worldInvertTranspose;
        private Vector3 _camView;
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

        public float DiffuseIntensity { get; set; }
        public float DiffuseOffset { get; set; }
        public float SpecularIntensity { get; set; }
        public float Shininess { get; set; }

        public Texture2D NormalMap { get; set; }

        public DiffuseSpecularMaterial(Scene scene)
            : base (scene)
        {
            diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _specularColor = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            DiffuseIntensity = 1.0f;
            SpecularIntensity = 1.0f;
            Shininess = 200.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/DiffuseSpecularTextureEffect");
        }

        public override void PrePass()
        {
            _camView = Vector3.Transform(scene.MainCamera.Target - scene.MainCamera.SceneObject.Transform.Position, Matrix.CreateRotationY(0.0f));
            _camView.Normalize();

            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // FIXME Fake light for now
            var light0 = scene.Lights[0];

            effect.Parameters["ShadowMapEnabled"].SetValue(light0.shadowGenerator.Enabled);

            if (light0.shadowGenerator.Enabled)
            {
                effect.Parameters["ShadowMap"].SetValue(light0.shadowGenerator.ShadowMap);
                effect.Parameters["ShadowMapSize"].SetValue(light0.shadowGenerator.ShadowMapSize);
                effect.Parameters["ShadowBias"].SetValue(light0.shadowGenerator.ShadowBias);
                effect.Parameters["ShadowStrength"].SetValue(light0.shadowGenerator.ShadowStrength);
            }

            // Light
            effect.Parameters["LightView0"].SetValue(light0.viewMatrix);
            effect.Parameters["LightProjection0"].SetValue(light0.projectionMatrix);

            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(0.1f);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);

            var dir0 = light0 as C3DE.Components.Lights.DirectionalLight;

            if (dir0 != null)    
                effect.Parameters["DiffuseLightDirection"].SetValue(dir0.Direction);
            else
            {
                Vector3 cDir = light0.SceneObject.Transform.Rotation;
                cDir.Normalize();
                effect.Parameters["DiffuseLightDirection"].SetValue(cDir);
            }
            
            effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(-0.2f, 0f, -1.0f)); // FIXME
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["DiffuseIntensity"].SetValue(DiffuseIntensity);
            effect.Parameters["DiffuseOffset"].SetValue(DiffuseOffset);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["SpecularIntensity"].SetValue(SpecularIntensity);
            effect.Parameters["ViewVector"].SetValue(_camView);
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
