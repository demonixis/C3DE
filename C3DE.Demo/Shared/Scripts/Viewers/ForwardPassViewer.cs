using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class ForwardPassViewer : Behaviour
    {
        private Texture2D _borderTexture;
        private ForwardRenderer _renderer;

        public override void Start()
        {
            _borderTexture = TextureFactory.CreateColor(Color.Black, 1, 1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (_renderer == null)
            {
                _renderer = Application.Engine.Renderer as ForwardRenderer;
                if (_renderer == null)
                    return;
            }

            var width = 260;
            var height = 162;
            var x = Screen.Width - width - 25;
            var y = 25;

            GUI.Skin.TextColor = Color.White;

            ui.DrawTexture(new Rectangle(x, y, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(x + 2, y + 2, width - 4, height - 4), _renderer.GetNormalBuffer());
            ui.Label(new Vector2(x + 5, y + height - 25), "Normal Buffer");

            ui.DrawTexture(new Rectangle(x, height, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(x + 2, 2 + height, width - 4, height - 4), _renderer.GetDepthBuffer());
            ui.Label(new Vector2(x + 5, 2 * height - 25), "Depth Buffer");

            _renderer.UseMRT = ui.Checkbox(new Rectangle(x, height * 2, 32, 32), "Use MRT", _renderer.UseMRT);
        }
    }
}
