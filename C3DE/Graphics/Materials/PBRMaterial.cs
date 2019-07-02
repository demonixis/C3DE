using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static C3DE.Graphics.TextureFactory;

namespace C3DE.Graphics.Materials
{
    public class PBRMaterial : Material
    {
        internal Texture2D _rmsaoMap;

        public Texture2D NormalMap { get; set; }
        public Texture2D RoughnessMetalicSpecularAOMap => _rmsaoMap;

        /// <summary>
        /// Irradiance Map. Will be generated from the Skybox in the near future.
        /// </summary>
        public TextureCube IrradianceMap { get; set; }

        // TODO;
        public Texture2D EmissiveMap { get; set; }

        public PBRMaterial() : base()
        {
            IrradianceMap = Scene.current.RenderSettings.Skybox.Texture;
        }

        public PBRMaterial(string name) : base(name) { }

        public void CreateRMSFromValues(float roughness = 0.5f, float metallic = 0.5f, float specular = 0.5f, float ao = 1.0f)
        {
            CreateRMSFromTextures(
                CreateColor(GetColor(roughness), 1, 1),
                CreateColor(GetColor(metallic), 1, 1),
                CreateColor(GetColor(specular), 1, 1),
                CreateColor(GetColor(ao), 1, 1));
        }

        public void CreateRMSFromTextures(Texture2D roughness, Texture2D metallic, Texture2D specular, Texture2D ao)
        {
            if (roughness == null || metallic == null || specular == null || ao == null)
                throw new System.Exception("One of the texture is null.");

            var width = roughness.Width;
            var height = roughness.Height;
            var rColors = GetColors(roughness);
            var gColors = GetColors(TryResize(metallic, width, height));
            var bColors = GetColors(TryResize(specular, width, height));
            var aColors = GetColors(TryResize(ao, width, height));
            var colors = new Color[rColors.Length];

            for (var i = 0; i < rColors.Length; i++)
            {
                colors[i].R = rColors[i].R;
                colors[i].G = gColors[i].R;
                colors[i].B = bColors[i].R;
                colors[i].A = aColors[i].R;
            }

            if (_rmsaoMap != null)
                _rmsaoMap.Dispose();

            // TODO: Don't make a new instance if the size is the same as before.
            _rmsaoMap = new Texture2D(Application.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            _rmsaoMap.SetData<Color>(colors);
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            m_ShaderMaterial = new ForwardPBR(this);
            m_ShaderMaterial.LoadEffect(Application.Content);
        }

        private static Color GetColor(float value) => new Color(value, value, value, 1);
    }
}
