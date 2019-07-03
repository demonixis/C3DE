using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class ReflectionProbeDebugger : Behaviour
    {
        private Texture2D _borderTexture;
        private ReflectionProbe _reflectionProbe;

        public override void Start()
        {
            _borderTexture = TextureFactory.CreateColor(Color.Black, 1, 1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (_reflectionProbe == null)
            {
                _reflectionProbe = GetComponent<ReflectionProbe>();
                if (_reflectionProbe == null)
                    return;
            }

            var width = 128;
            var height = 128;

            GUI.Skin.TextColor = Color.White;

            for (var i = 0; i < 6; i++)
            {
                //ui.DrawTexture(new Rectangle(0, height * i, width, height), m_BorderTexture);
                ui.DrawTexture(new Rectangle(2, 2 + height * i, width - 4, height - 4), _reflectionProbe.GetRenderTarget((CubeMapFace)i));
                ui.Label(new Vector2(5, height - 28 + (i * height)), ((CubeMapFace)i).ToString());
            }
        }
    }
}
