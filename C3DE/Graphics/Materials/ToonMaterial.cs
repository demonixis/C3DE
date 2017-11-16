using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class ToonMaterial : Material
    {
        private Vector3 _emissiveColor;

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        public ToonMaterial(Scene scene, string name = "Toon Material")
            : base(scene)
        {
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("Shaders/ToonEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);

            if (scene.lights.Count > 0)
            {
                var light0 = scene.Lights[0]; // FIXME
                effect.Parameters["LightDirection"].SetValue(light0.transform.LocalRotation);
            }
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["MainTexture"].SetValue(MainTexture);
            effect.Parameters["World"].SetValue(renderable.GameObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
