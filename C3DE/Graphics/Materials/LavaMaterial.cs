using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class LavaMaterial : Material, IEmissiveMaterial
    {
        private Vector3 _emissiveColor = Vector3.One;
        private float _totalTime;

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        public Texture2D NormalTexture { get; set; }
        public Texture2D EmissiveTexture { get; set; }
        public float EmissiveIntensity { get; set; } = 1.0f;

        public LavaMaterial(Scene scene, string name = "Lava Material")
            : base(scene)
        {
            DiffuseColor = Color.White;
            _totalTime = 0.0f;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("Shaders/LavaEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            _totalTime += Time.DeltaTime / 10.0f;

            effect.Parameters["Time"].SetValue(_totalTime);
            effect.Parameters["MainTexture"].SetValue(MainTexture);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public bool EmissivePass(Renderer renderer)
        {
            if (EmissiveTexture == null)
                return false;

            effect.Parameters["EmissiveTexture"].SetValue(EmissiveTexture);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["EmissiveIntensity"].SetValue(EmissiveIntensity);
            effect.CurrentTechnique.Passes["EmissivePass"].Apply();
            return true;
        }
    }
}
