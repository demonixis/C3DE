using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class BasicMaterial : Material
    {
        private Vector3 _shadowData;
        private float _alpha;
        private bool _useAlpha;

        public float Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                _useAlpha = value != 1.0f;
            }
        }

        public BasicMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _alpha = 1.0f;
            _useAlpha = false;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/DiffuseSpecularTextureEffect").Clone();
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // Gets the first light.
            var light0 = scene.Lights[0];

            // Update shadow data.
            _shadowData.X = light0.shadowGenerator.Enabled ? light0.shadowGenerator.ShadowMapSize : 0;
            _shadowData.Y = light0.shadowGenerator.ShadowBias;
            _shadowData.Z = light0.shadowGenerator.ShadowStrength;

            effect.Parameters["ShadowData"].SetValue(_shadowData);

            // Light
            effect.Parameters["LightView0"].SetValue(light0.viewMatrix);
            effect.Parameters["LightProjection0"].SetValue(light0.projectionMatrix);

            // FIXME Do a loop when ok
            if (light0.Type != LightType.Directional)
            {
                Vector3 cDir = light0.SceneObject.Transform.Position;
                cDir.Normalize();
                effect.Parameters["DiffuseLightDirection"].SetValue(cDir);
            }
            else
                effect.Parameters["DiffuseLightDirection"].SetValue(light0.Direction);

            // Material
            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector4());
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["Alpha"].SetValue(_alpha);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["RecieveShadows"].SetValue(renderable.RecieveShadow);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[_useAlpha ? 1 : 0].Apply();
        }
    }
}
