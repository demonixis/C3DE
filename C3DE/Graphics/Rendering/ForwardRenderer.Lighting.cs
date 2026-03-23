using C3DE.Components.Lighting;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Graphics.Rendering
{
    public partial class ForwardRenderer
    {
        private const float GpuDirectionalLightType = 0.0f;
        private const float GpuPointLightType = 1.0f;
        private const float GpuSpotLightType = 2.0f;

        private void ComputeLightData(Scene scene)
        {
            var lights = scene._lights;
            _culledLights.Clear();

            CollectLightsByPriority(lights, LightPrority.High);
            CollectLightsByPriority(lights, LightPrority.Auto);

            var lightCount = Math.Min(MaxLightCount, _culledLights.Count);
            EnsureLightCapacity(lightCount);
            _lightData.Count = lightCount;

            var shadow = false;
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[_culledLights[i]];
                PackLight(light, i);

                if (shadow || !light.ShadowEnabled)
                    continue;

                _shadowData.ProjectionMatrix = light._projectionMatrix;
                _shadowData.ViewMatrix = light._viewMatrix;
                _shadowData.Data = light._shadowGenerator._shadowData;
                _shadowData.ShadowMap = light._shadowGenerator.ShadowMap;
                shadow = true;
            }

            if (!shadow)
            {
                _shadowData.ProjectionMatrix = Matrix.Identity;
                _shadowData.ViewMatrix = Matrix.Identity;
                _shadowData.Data = Vector3.Zero;
                _shadowData.ShadowMap = null;
            }

            _lightData.Ambient = scene.RenderSettings.ambientColor;
        }

        private void CollectLightsByPriority(System.Collections.Generic.List<Light> lights, LightPrority priority)
        {
            for (var i = 0; i < lights.Count; i++)
            {
                var light = lights[i];
                if (!light.Enabled || !light.GameObject.Enabled)
                    continue;

                if (priority == LightPrority.High)
                {
                    if (light.Type == LightType.Directional || light.Priority == LightPrority.High)
                        _culledLights.Add(i);

                    continue;
                }

                if (light.Type != LightType.Directional && light.Priority != LightPrority.High)
                    _culledLights.Add(i);
            }
        }

        private void EnsureLightCapacity(int requiredLightCount)
        {
            if (requiredLightCount <= 0)
                return;

            var currentCapacity = _lightData.Positions?.Length ?? 0;
            if (currentCapacity >= requiredLightCount)
                return;

            // EffectParameter.SetValue(array) expects an array whose length does not exceed
            // the shader-declared array size. Keep one preallocated upload buffer matching
            // the backend light ceiling instead of geometric growth.
            _lightData.Positions = new Vector3[MaxLightLimit];
            _lightData.Colors = new Vector3[MaxLightLimit];
            _lightData.Data = new Vector4[MaxLightLimit];
            _lightData.SpotData = new Vector4[MaxLightLimit];
        }

        private void PackLight(Light light, int index)
        {
            var encodedType = GpuDirectionalLightType;
            var encodedPosition = light.Transform.Position;
            var encodedDirection = light.Direction;

            if (light.Type == LightType.Point)
            {
                encodedType = GpuPointLightType;
            }
            else if (light.Type == LightType.Spot)
            {
                encodedType = GpuSpotLightType;
            }
            else
            {
                encodedPosition = encodedDirection;
            }

            _lightData.Positions[index] = encodedPosition;
            _lightData.Colors[index] = light._color;
            _lightData.Data[index] = new Vector4(encodedType, light.Intensity, light.Radius, light.FallOf);
            _lightData.SpotData[index] = new Vector4(encodedDirection, light.Angle);
        }
    }
}
