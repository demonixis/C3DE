using C3DE.Components.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class StandardMaterial : Material
    {
        private string _fxName;
        protected float shadowMapSize;
        protected bool shadowMapEnabled;

        public StandardMaterial(Scene scene)
            : base (scene)
        {
            _fxName = "StandardEffect";
        }

        public void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>(_fxName);
        }
    }
}
