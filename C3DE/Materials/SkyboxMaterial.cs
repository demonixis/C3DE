using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class SkyboxMaterial : Material
    {
        private Texture2D[] _textures;

        public Texture2D[] Textures
        {
            get { return _textures; }
            set { _textures = value; }
        }

        public SkyboxMaterial(Scene scene)
            : base(scene)
        {

        }

        public override void LoadContent(ContentManager content)
        {

        }

        public override void PrePass()
        {

        }

        public override void Pass(Components.Transform transform)
        {

        }
    }
}
