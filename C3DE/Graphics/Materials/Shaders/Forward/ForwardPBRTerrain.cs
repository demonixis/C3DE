using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBRTerrain : ShaderMaterial
    {
        private PBRTerrainMaterial _material;
        private Vector2 _features;

        public ForwardPBRTerrain(PBRTerrainMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/TerrainPBR");
            SetupParamaters();
        }

        protected virtual void SetupParamaters()
        {
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera._viewMatrix);
            _effect.Parameters["Projection"].SetValue(camera._projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);

            ForwardPBR.UpdatePBRPrePass(_effect, camera);
        }

        public override void Pass(Renderer renderable)
        {
            _features.X = _material._combinedNormals != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["CombinedAlbedos"].SetValue(_material._combinedAlbedos);
            _effect.Parameters["CombinedNormals"].SetValue(_material._combinedNormals);
            _effect.Parameters["CombinedRMAOs"].SetValue(_material._combinedRMAO);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(renderable.ReceiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
