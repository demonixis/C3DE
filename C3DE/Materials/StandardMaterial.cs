using C3DE.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class StandardMaterial : Material
    {
        protected float shadowMapSize;
        protected bool shadowMapEnabled;

        public StandardMaterial(Scene scene)
            : base (scene)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/StandardEffect");
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // FIXME Do a loop when ok
            var light0 = scene.Lights[0];

            // Shadows
            effect.Parameters["shadowMapEnabled"].SetValue(light0.shadowGenerator.Enabled);

            if (light0.shadowGenerator.Enabled)
            {
                effect.Parameters["shadowTexture"].SetValue(light0.shadowGenerator.ShadowMap);
                effect.Parameters["shadowMapSize"].SetValue(light0.shadowGenerator.ShadowMapSize);
                effect.Parameters["shadowBias"].SetValue(light0.shadowGenerator.ShadowBias);
                effect.Parameters["shadowStrength"].SetValue(light0.shadowGenerator.ShadowStrength);
            }
            
            // Light
            effect.Parameters["lightView"].SetValue(light0.viewMatrix);
            effect.Parameters["lightProjection"].SetValue(light0.projectionMatrix);
            effect.Parameters["lightPosition"].SetValue(light0.SceneObject.Transform.Position);
            effect.Parameters["lightRadius"].SetValue(light0.Radius);
            effect.Parameters["ambientColor"].SetValue(scene.AmbientColor.ToVector4());
        }

        public override void Pass(Transform transform)
        {
            effect.Parameters["mainTexture"].SetValue(mainTexture);
            effect.Parameters["emissiveColor"].SetValue(emissiveColor.ToVector4());
            effect.Parameters["World"].SetValue(transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
