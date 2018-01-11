using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardTerrainMaterial : StandardMaterialBase
    {
        public Texture2D SnowTexture { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D WeightTexture { get; set; }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardStandardTerrain(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
