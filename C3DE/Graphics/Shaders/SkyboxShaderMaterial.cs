using Microsoft.Xna.Framework;

namespace C3DE.Graphics.Shaders
{
    public abstract class SkyboxShaderMaterial : ShaderMaterial
    {
        public abstract void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix);
    }
}
