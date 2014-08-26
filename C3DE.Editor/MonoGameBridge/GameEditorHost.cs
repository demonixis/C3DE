using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace C3DE.Editor.MonoGameBridge
{
    public class GameEditorHost : D3D11Host
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Draw()
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

        }
    }
}
