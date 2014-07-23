using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Materials
{
    public abstract class Material : Component
    {
        protected Color ambientColor;
        protected Effect effect;
    }
}
