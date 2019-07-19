using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders
{
    public abstract class ShaderMaterial 
    {
        internal protected Effect _effect;

        public abstract void LoadEffect(ContentManager content);
        public abstract void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData);
        public abstract void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced);
    }
}
