using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredLava : ForwardStandardLava
    {
        public DeferredLava(StandardLavaMaterial material) : base(material) { }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Lava");
        }
    }
}
