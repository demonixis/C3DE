using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Materials
{
    public class StandardMaterial : Material
    {
        private string _fxName;
        protected float shadowMapSize;
        protected bool shadowMapEnabled;

        public StandardMaterial()
        {
            _fxName = "StandardEffect";
            shadowMapSize = 512;
            shadowMapEnabled = true;
        }

        public void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>(_fxName);
        }

        public void SetWorldMatrix(ref Matrix worldMatrix)
        {
            effect.Parameters["World"].SetValue(worldMatrix);
        }

        public void Update(RenderTarget2D shadowRT, Camera camera, Light light)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["shadowTexture"].SetValue(shadowRT);
            effect.Parameters["lightView"].SetValue(light.viewMatrix);
            effect.Parameters["lightProjection"].SetValue(light.projectionMatrix);
            effect.Parameters["lightPosition"].SetValue(light.Transform.Position);
            effect.Parameters["lightRadius"].SetValue(light.radius);
            effect.Parameters["shadowMapSize"].SetValue(shadowMapSize);
        }
    }
}
