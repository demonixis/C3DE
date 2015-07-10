using C3DE.Components;
using C3DE.Components.Lights;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class LightSwitcher : Behaviour
    {
        private Rectangle _box;
        private Rectangle _btn1;
        private Rectangle _btn2;
        private Rectangle _btn3;
        private Light _light;

        public override void Start()
        {
            _box = new Rectangle(Screen.VirtualWidth - 150, 10, 140, 150);
            _btn1 = new Rectangle(_box.X + 10, _box.Y + 30, _box.Width - 20, 30);
            _btn2 = new Rectangle(_box.X + 10, _btn1.Y + 40, _box.Width - 20, 30);
            _btn3 = new Rectangle(_box.X + 10, _btn2.Y + 40, _box.Width - 20, 30);

            _light = GetComponent<Light>();

            GUI.Skin.Font = Application.Content.Load<SpriteFont>("Font/Default");
        }

        public override void OnGUI(GUI gui)
        {
            gui.Box(_box, "Lights");

            if (gui.Button(_btn1, "Ambiant"))
                _light.TypeLight = LightType.Ambient;

            if (gui.Button(_btn2, "Directional"))
                _light.TypeLight = LightType.Directional;

            if (gui.Button(_btn3, "Point"))
                _light.TypeLight = LightType.Point;
        }
    }
}
