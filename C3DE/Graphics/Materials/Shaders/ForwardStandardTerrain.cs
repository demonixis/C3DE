using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        public ForwardStandardTerrain(StandardTerrainMaterial material)
        {

        }

        public override void LoadEffect(ContentManager content)
        {
            throw new NotImplementedException();
        }

        public override void Pass(Renderer renderable)
        {
            throw new NotImplementedException();
        }
    }
}
