using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Materials
{
    public class DiffuseSpecular : Material
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
        public float SpecularIntensity { get; set; }
        public float Shininess { get; set; }

        public Texture2D NormalMap { get; set; }

        public DiffuseSpecular(Scene scene)
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
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // FIXME Fake light for now
            //var light0 = scene.Lights[0];

            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(0.1f);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(-0.2f, -0.5f, -1.0f)); // FIXME
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["DiffuseIntensity"].SetValue(DiffuseIntensity);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["SpecularIntensity"].SetValue(SpecularIntensity);

            _camView = Vector3.Transform(scene.MainCamera.Target - scene.MainCamera.SceneObject.Transform.Position, Matrix.CreateRotationY(0.0f));
            _camView.Normalize();
            effect.Parameters["ViewVector"].SetValue(_camView);
        }

        public override void Pass(RenderableComponent renderable)
        {
            _worldInvertTranspose = Matrix.Transpose(Matrix.Invert(renderable.SceneObject.Transform.world));
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["WorldInverseTranspose"].SetValue(_worldInvertTranspose);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
