using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredUnlit : ForwardUnlit
    {
        public DeferredUnlit(UnlitMaterial material) : base(material)
        {
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Unlit");
            SetupParamaters();
        }
    }
}
