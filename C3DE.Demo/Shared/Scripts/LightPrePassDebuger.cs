using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class LightPrePassDebuger : Behaviour
    {
        private Texture2D _borderTexture;
        private LightPrePassRenderer _renderer;

        public override void Start()
        {
            _borderTexture = TextureFactory.CreateColor(Color.Black, 1, 1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (_renderer == null)
            {
                _renderer = Application.Engine.Renderer as LightPrePassRenderer;
                if (_renderer == null)
                    return;
            }

            var width = 320;
            var height = 200;

            GUI.Skin.TextColor = Color.White;

            ui.DrawTexture(new Rectangle(0, 0, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2, width - 4, height - 4), _renderer.DepthBuffer);

            ui.DrawTexture(new Rectangle(0, height, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2 + height, width - 4, height - 4), _renderer.NormalBuffer);

            ui.DrawTexture(new Rectangle(0, height * 2, width, height), _borderTexture);
            ui.DrawTexture(new Rectangle(2, 2 + height * 2, width - 4, height - 4), _renderer.LightBuffer);

            ui.Label(new Vector2(5, height - 25), "Depth Buffer");
            ui.Label(new Vector2(5, 2 * height - 25), "Normal Buffer");
            ui.Label(new Vector2(5, 3 * height - 25), "Lights Buffer");
        }
    }
}
