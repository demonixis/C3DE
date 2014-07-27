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
    }
}
