using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardToonMaterial : StandardMaterial
    {
        public StandardToonMaterial(Scene scene, string name = "Standard Toon Material")
            : base(scene, name)
        {
        }

        protected override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/StandardToonEffect");
        }
    }
}
