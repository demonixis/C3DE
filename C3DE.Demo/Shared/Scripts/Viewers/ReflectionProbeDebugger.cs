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
    public class ReflectionProbeViewer : Behaviour
    {
        private Texture2D m_BorderTexture;
        private Texture2D _faceTexture;
        private Color[] _colorBuffer;
        private ReflectionProbe _reflectionProbe;

        public override void Start()
        {
            m_BorderTexture = TextureFactory.CreateColor(Color.Black, 1, 1);
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

            var width = 96;
            var height = 96;

            GUI.Skin.TextColor = Color.White;

            var size = _reflectionProbe.Size;

            if (_faceTexture == null || _faceTexture.Width != size)
            {
                _faceTexture?.Dispose();
                _faceTexture = new Texture2D(Application.GraphicsDevice, size, size);
                _colorBuffer = new Color[size * size];
            }

            var map = _reflectionProbe.ReflectionMap;

            for (var i = 0; i < 6; i++)
            {
                map.GetData((CubeMapFace)i, _colorBuffer);
                _faceTexture.SetData(_colorBuffer);

                ui.DrawTexture(new Rectangle(Screen.VirtualWidth - width, height * i, width, height), m_BorderTexture);
                ui.DrawTexture(new Rectangle(Screen.VirtualWidth - width - 2, 2 + height * i, width - 4, height - 4), _faceTexture);
                ui.Label(new Vector2(Screen.VirtualWidth - width + 5, height - 28 + (i * height)), ((CubeMapFace)i).ToString());
            }
        }
    }
}
