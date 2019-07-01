using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class PBRMaterial : Material
    {
        internal Texture2D _rmsMap;
        protected float _smoothsness;

        public float Smoothsness
        {
            get => _smoothsness;
            set
            {
                _smoothsness = value;

                if (_smoothsness > 1)
                    _smoothsness = 1;
                else if (_smoothsness < 0)
                    _smoothsness = 0;
            }
        }

        public Texture2D NormalMap { get; set; }
        public Texture2D RMSMap => _rmsMap;
        public Texture2D AOMap { get; set; }
        public TextureCube IrradianceMap { get; set; }

        // TODO;
        public Texture2D EmissionMap { get; set; }
        public float EmissionIntensity { get; set; }

        public PBRMaterial() : base()
        {
            IrradianceMap = Scene.current.RenderSettings.Skybox.Texture;
        }

        public PBRMaterial(string name) : base(name) { }

        public void CreateRMSFromValues(float roughness, float metallic, float specular)
        {
            if (_rmsMap != null)
                _rmsMap.Dispose();

            _rmsMap = new Texture2D(Application.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _rmsMap.SetData<Color>(new Color[]
            {
                new Color(roughness, metallic, specular, (byte)(_smoothsness * 255.0f))
            });
        }

        public void CreateRMSFromTextures(Texture2D roughness, Texture2D metallic, Texture2D specular)
        {
            if (specular == null)
                specular = GraphicsHelper.CreateTexture(Color.Black, 1, 1);

            var width = roughness.Width;
            var height = roughness.Height;
            var rColors = GraphicsHelper.ExtractColors(roughness);
            var gColors = GraphicsHelper.ExtractColors(GraphicsHelper.TryResize(metallic, width, height));
            var bColors = GraphicsHelper.ExtractColors(GraphicsHelper.TryResize(specular, width, height));
            var colors = new Color[rColors.Length];

            for (var i = 0; i < rColors.Length; i++)
            {
                colors[i].R = rColors[i].R;
                colors[i].G = gColors[i].R;
                colors[i].B = bColors[i].R;
                colors[i].A = (byte)(_smoothsness * 255.0f);
            }

            if (_rmsMap != null)
                _rmsMap.Dispose();

            // TODO: Don't make a new instance if the size is the same as before.
            _rmsMap = new Texture2D(Application.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            _rmsMap.SetData<Color>(colors);
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            m_ShaderMaterial = new ForwardPBR(this);
            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
