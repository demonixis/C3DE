using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Rendering.Passes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public partial class ForwardRenderer : BaseRenderer
    {
#if WINDOWS
        internal const int MaxLightLimit = 128;
#else
        internal const int MaxLightLimit = 16;
#endif
        private static int _maxLightCount = MaxLightLimit;

        private DepthPass _depthPass;
        private NormalPass _normalPass;
        private DepthNormalPass _depthNormalPass;

        private readonly List<int> _culledLights;
        private LightData _lightData;
        private ShadowData _shadowData;

        public bool DirectRendering { get; set; }
        public bool UseMRT { get; set; }

        public static int MaxLightCount
        {
            get => _maxLightCount;
            set => _maxLightCount = Math.Clamp(value, 0, MaxLightLimit);
        }

        public ForwardRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            _culledLights = new List<int>(MaxLightLimit);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();

            _depthPass = new DepthPass(_graphicsDevice);
            _depthPass.LoadContent(content);

            _normalPass = new NormalPass(_graphicsDevice);
            _normalPass.LoadContent(content);

            _depthNormalPass = new DepthNormalPass(_graphicsDevice);
            _depthNormalPass.LoadContent(content);
        }

        public override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    DisposeObject(_sceneRenderTargets);

                _disposed = true;
            }
        }

        /// <summary>
        /// Renders the scene with the specified camera.
        /// Render order:
        /// 1 - Shadow maps
        /// 2 - Objects
        /// 3 - Post Processing
        /// 4 - UI
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        public override void Render(Scene scene)
        {
            if (scene == null || scene?._cameras.Count == 0)
                return;

            Camera camera = null;

            foreach (var cam in scene._cameras)
            {
                if (cam.RenderTarget == null)
                {
                    camera = cam;
                    break;
                }
            }

            if (camera == null)
            {
                RenderUI(scene._scripts);
                return;
            }

            if (DirectRendering)
            {
                _graphicsDevice.Clear(camera._clearColor);

                var cameraPosition = camera._transform.Position;
                var cameraViewMatrix = camera._viewMatrix;
                var cameraProjectionMatrix = camera._projectionMatrix;

                RenderObjects(scene, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
                RenderUI(scene._scripts);
                return;
            }

            RebuildRenderTargets();
            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                var cameraParent = Matrix.Identity;
                var parent = camera._transform.Parent;
                if (parent != null)
                    cameraParent = parent._worldMatrix;

                for (var eye = 0; eye < 2; eye++)
                {
                    camera._projectionMatrix = _VRService.GetProjectionMatrix(eye);
                    camera._viewMatrix = _VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, _sceneRenderTargets[eye]);
                }

                _VRService.SubmitRenderTargets(_sceneRenderTargets[0], _sceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene._scripts);
                return;
            }

            RenderSceneForCamera(scene, camera, _sceneRenderTargets[0]);
        }
    }
}
