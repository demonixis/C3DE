using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class DeferredDebuger : Behaviour
    {
        private Texture2D _borderTexture;
        private DeferredRenderer _renderer;

        public override void Start()
        {
            _borderTexture = TextureFactory.CreateColor(Color.Black, 1, 1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (_renderer == null)
            {
                _renderer = Application.Engine.Renderer as DeferredRenderer;
                if (_renderer == null)
                    return;
            }

            var width = 260;
            var height = 162;

            GUI.Skin.TextColor = Color.White;

            ui.DrawTexture(new Rectangle(0, 0, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2, width - 4, height - 4), _renderer.ColorBuffer);

            ui.DrawTexture(new Rectangle(0, height, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2 + height, width - 4, height - 4), _renderer.NormalMap);

            ui.DrawTexture(new Rectangle(0, height * 2, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2 + height * 2, width - 4, height - 4), _renderer.DepthBuffer);

            ui.DrawTexture(new Rectangle(0, height * 3, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2 + height * 3, width - 4, height - 4), _renderer.LightMap);

            ui.Label(new Vector2(5, height - 25), "Color Buffer");
            ui.Label(new Vector2(5, 2 * height - 25), "Normal Buffer");
            ui.Label(new Vector2(5, 3 * height - 25), "Depth Buffer");
            ui.Label(new Vector2(5, 4 * height - 25), "Lights Buffer");
        }
    }
}
