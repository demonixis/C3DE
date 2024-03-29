﻿using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;

namespace C3DE.Graphics.Shaders
{
    public abstract class ForwardPBRBase : ShaderMaterial
    {
        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["ShadowStrength"].SetValue(shadowData.Data.Z);
            _effect.Parameters["ShadowBias"].SetValue(shadowData.Data.Y);
            _effect.Parameters["ShadowMap"].SetValue(shadowData.ShadowMap);
            _effect.Parameters["LightView"].SetValue(shadowData.ViewMatrix);
            _effect.Parameters["LightProjection"].SetValue(shadowData.ProjectionMatrix);
            _effect.Parameters["LightCount"].SetValue(lightData.Count);

            if (lightData.Count > 0)
            {
                _effect.Parameters["LightPosition"].SetValue(lightData.Positions);
                _effect.Parameters["LightColor"].SetValue(lightData.Colors);
                _effect.Parameters["LightData"].SetValue(lightData.Data);
            }

            _effect.Parameters["IrradianceMap"].SetValue(Scene.current.RenderSettings.skybox.IrradianceTexture);
        }
    }
}
