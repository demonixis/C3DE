using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandard : ForwardStandardBase
    {
        private StandardMaterial _material;
        private Vector3 _features;

        public ForwardStandard(StandardMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Standard");
        }


        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {

        }
    }
}
