using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    public partial class ForwardRenderer
    {
        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            var cameraPosition = camera._transform.Position;
            var cameraViewMatrix = camera._viewMatrix;
            var cameraProjectionMatrix = camera._projectionMatrix;

            _depthPass.Enabled = false;
            _normalPass.Enabled = false;
            _depthNormalPass.Enabled = false;
            PreparePostProcess(scene);
            RenderPreSceneBuffers(scene, camera);
            RenderReflectionProbes(scene);

            _graphicsDevice.SetRenderTarget(renderTarget);

            var renderToRT = camera.RenderTarget != null;
            if (renderToRT)
                _graphicsDevice.SetRenderTarget(camera.RenderTarget);

            _graphicsDevice.Clear(camera._clearColor);

            RenderObjects(scene, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
            RenderPostProcess(scene, camera, renderTarget);

            if (renderToRT)
                return;

            if (!m_VREnabled)
            {
                RenderToBackBuffer();
                RenderUI(scene._scripts);
            }
        }

        protected void RenderObjects(Scene scene, ref Vector3 cameraPosition, ref Matrix cameraViewMatrix, ref Matrix cameraProjectionMatrix)
        {
            ResetGraphicsStateForSceneRender();
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            var fogData = scene.RenderSettings.fogData;

            ComputeLightData(scene);

            if (scene.RenderSettings.Skybox.Enabled)
            {
                using (_graphicsDevice.GeometryState())
                    scene.RenderSettings.Skybox.Draw(_graphicsDevice, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
            }

            var renderCount = scene._renderList.Count;

            for (var i = 0; i < renderCount; i++)
            {
                var renderer = scene._renderList[i];

                if (!renderer.Enabled || !renderer._gameObject.Enabled)
                    continue;

                var material = renderer.Material;
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                ShaderMaterial shader = material._shaderMaterial;
                shader.PrePassForward(ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix, ref _lightData, ref _shadowData, ref fogData);
                shader.Pass(ref renderer._transform._worldMatrix, renderer.ReceiveShadow, renderer.InstancedEnabled);
                renderer.Draw(_graphicsDevice);
            }
        }

        private void ResetGraphicsStateForSceneRender()
        {
            for (var i = 0; i < 16; i++)
            {
                _graphicsDevice.Textures[i] = null;
                _graphicsDevice.SamplerStates[i] = SamplerState.LinearWrap;
            }

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.Indices = null;
            _graphicsDevice.SetVertexBuffer(null);
        }

        private void RenderPreSceneBuffers(Scene scene, Camera camera)
        {
            if (_depthNormalPass.Enabled)
            {
                _depthNormalPass.Render(scene, camera);
                return;
            }

            if (_depthPass.Enabled)
                _depthPass.Render(scene, camera);

            if (_normalPass.Enabled)
                _normalPass.Render(scene, camera);
        }

        private void RenderReflectionProbes(Scene scene)
        {
            var reflectionProbes = scene._reflectionProbes;
            if (reflectionProbes.Count == 0)
                return;

            Color[] colorBuffer = null;
            var oldRT = _graphicsDevice.GetRenderTargets();

            foreach (var probe in reflectionProbes)
            {
                if (probe.Mode != ReflectionProbe.RenderingMode.Realtime && !probe.Dirty)
                    continue;

                if (colorBuffer == null || colorBuffer.Length != probe.Resolution * probe.Resolution)
                    colorBuffer = new Color[probe.Resolution * probe.Resolution];

                var probeCam = probe._camera;

                for (var i = 0; i < 6; i++)
                {
                    probeCam._transform.LocalRotation = probe.GetCameraRotation((CubeMapFace)i);
                    probeCam._transform.UpdateWorldMatrix();
                    probeCam.Update();

                    var probeCamPos = probeCam._transform.Position;
                    var probeCamView = probeCam._viewMatrix;
                    var probeCamProj = probeCam._projectionMatrix;

                    _graphicsDevice.SetRenderTarget(probeCam.RenderTarget);
                    _graphicsDevice.Clear(Color.Black);

                    RenderObjects(scene, ref probeCamPos, ref probeCamView, ref probeCamProj);
                    probeCam.RenderTarget.GetData(colorBuffer);

                    probe._reflectionTexture.SetData((CubeMapFace)i, colorBuffer);
                }

                probe.Dirty = false;
            }

            _graphicsDevice.SetRenderTargets(oldRT);
        }
    }
}
