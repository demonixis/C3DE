using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class StandardMaterial : Material
    {
        private string _fxName;
        protected float shadowMapSize;
        protected bool shadowMapEnabled;

        public StandardMaterial(Scene scene)
            : base (scene)
        {
            _fxName = "StandardEffect";
        }

        public void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>(_fxName);
        }

        public void SetWorldMatrix(ref Matrix worldMatrix)
        {
            effect.Parameters["World"].SetValue(worldMatrix);
        }

        public void Update(RenderTarget2D shadowRT, CameraPrefab camera, LightPrefab light)
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
