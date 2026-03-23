using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.OpenXR;
using System;

namespace C3DE.VR
{
    /// <summary>
    /// OpenXR VR service implementation using Silk.NET.OpenXR.
    /// Supports any OpenXR-compatible runtime (SteamVR, Oculus, WMR, Meta, etc.).
    ///
    /// IMPORTANT: Session creation requires a platform-specific graphics binding
    /// (XR_KHR_D3D11_enable on Windows, XR_KHR_opengl_enable on Desktop).
    /// Accessing the native graphics device handle from MonoGame requires
    /// platform-specific interop which must be added per target platform.
    /// </summary>
    public unsafe class OpenXRService : VRService
    {
        // OpenXR version 1.0.0 — XR_MAKE_VERSION(1, 0, 0)
        private const ulong XrVersion10 = (1UL << 48);

        private XR _xr;
        private Instance _instance;
        private Session _session;
        private ulong _systemId;
        private Space _referenceSpace;
        private Swapchain[] _swapchains;
        private uint[] _swapchainWidths;
        private uint[] _swapchainHeights;
        private RenderTarget2D[] _renderTargets;
        private View[] _views;
        private bool _sessionRunning;
        private FrameState _frameState;

        private const int EyeCount = 2;

        public OpenXRService(Game game) : base(game)
        {
            _swapchains = new Swapchain[EyeCount];
            _swapchainWidths = new uint[EyeCount];
            _swapchainHeights = new uint[EyeCount];
            _renderTargets = new RenderTarget2D[EyeCount];
            _views = new View[EyeCount];
            for (int i = 0; i < EyeCount; i++)
                _views[i].Type = StructureType.View;
        }

        public override int TryInitialize()
        {
            try
            {
                _xr = XR.GetApi();

                if (!CreateInstance()) return -1;
                if (!GetSystem()) return -2;
                if (!CreateSession()) return -3;
                if (!CreateReferenceSpace()) return -4;
                if (!CreateSwapchains()) return -5;

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OpenXR] TryInitialize failed: {ex.Message}");
                return -99;
            }
        }

        private bool CreateInstance()
        {
            var appInfo = new ApplicationInfo();
            appInfo.ApiVersion = XrVersion10;

            // Copy application name into fixed-size byte array
            var nameBytes = System.Text.Encoding.UTF8.GetBytes("C3DE\0");
            var engineBytes = System.Text.Encoding.UTF8.GetBytes("C3DE Engine\0");
            fixed (byte* namePtr = nameBytes, enginePtr = engineBytes)
            {
                Buffer.MemoryCopy(namePtr, appInfo.ApplicationName, 128, Math.Min(nameBytes.Length, 128));
                Buffer.MemoryCopy(enginePtr, appInfo.EngineName, 128, Math.Min(engineBytes.Length, 128));
            }

            appInfo.ApplicationVersion = 1;
            appInfo.EngineVersion = 1;

            // Extension names for graphics API support
            string[] extensions;
#if WINDOWS
            extensions = new[] { "XR_KHR_D3D11_enable" };
#else
            extensions = new[] { "XR_KHR_opengl_enable" };
#endif

            var extPtr = SilkMarshal.StringArrayToPtr(extensions);
            try
            {
                var createInfo = new InstanceCreateInfo();
                createInfo.Type = StructureType.InstanceCreateInfo;
                createInfo.ApplicationInfo = appInfo;
                createInfo.EnabledExtensionCount = (uint)extensions.Length;
                createInfo.EnabledExtensionNames = (byte**)extPtr;

                var result = _xr.CreateInstance(ref createInfo, ref _instance);
                return result == Result.Success;
            }
            finally
            {
                SilkMarshal.Free(extPtr);
            }
        }

        private bool GetSystem()
        {
            var getInfo = new SystemGetInfo();
            getInfo.Type = StructureType.SystemGetInfo;
            getInfo.FormFactor = FormFactor.HeadMountedDisplay;
            var result = _xr.GetSystem(_instance, ref getInfo, ref _systemId);
            return result == Result.Success;
        }

        private bool CreateSession()
        {
            // Session creation requires a platform-specific graphics binding struct chained via Next.
            // Without native device handle access from MonoGame, this returns failure.
            // To complete this, retrieve the ID3D11Device (Windows) or HGLRC+HDC (Desktop)
            // from GraphicsDevice via reflection or a custom MonoGame extension, then populate
            // the graphics binding struct and set createInfo.Next = &graphicsBinding.
            var createInfo = new SessionCreateInfo();
            createInfo.Type = StructureType.SessionCreateInfo;
            createInfo.SystemId = _systemId;
            var result = _xr.CreateSession(_instance, ref createInfo, ref _session);
            return result == Result.Success;
        }

        private bool CreateReferenceSpace()
        {
            var spaceInfo = new ReferenceSpaceCreateInfo();
            spaceInfo.Type = StructureType.ReferenceSpaceCreateInfo;
            spaceInfo.ReferenceSpaceType = ReferenceSpaceType.Local;
            spaceInfo.PoseInReferenceSpace = new Posef(new Quaternionf(0, 0, 0, 1), new Vector3f(0, 0, 0));
            var result = _xr.CreateReferenceSpace(_session, ref spaceInfo, ref _referenceSpace);
            return result == Result.Success;
        }

        private bool CreateSwapchains()
        {
            uint viewCount = 0;
            _xr.EnumerateViewConfigurationView(_instance, _systemId, ViewConfigurationType.PrimaryStereo, 0, ref viewCount, null);
            if (viewCount == 0) return false;

            var viewConfigs = new ViewConfigurationView[viewCount];
            for (int i = 0; i < (int)viewCount; i++)
                viewConfigs[i].Type = StructureType.ViewConfigurationView;

            fixed (ViewConfigurationView* ptr = viewConfigs)
                _xr.EnumerateViewConfigurationView(_instance, _systemId, ViewConfigurationType.PrimaryStereo, viewCount, ref viewCount, ptr);

            // Pick a swapchain format (DXGI_FORMAT_R8G8B8A8_UNORM = 28, GL_RGBA8 = 32856)
            uint formatCount = 0;
            _xr.EnumerateSwapchainFormats(_session, 0, ref formatCount, null);
            var formats = new long[Math.Max(1, formatCount)];
            fixed (long* fPtr = formats)
                _xr.EnumerateSwapchainFormats(_session, formatCount, ref formatCount, fPtr);

#if WINDOWS
            long preferredFormat = 28; // DXGI_FORMAT_R8G8B8A8_UNORM
#else
            long preferredFormat = 32856; // GL_RGBA8
#endif

            for (int eye = 0; eye < EyeCount && eye < (int)viewCount; eye++)
            {
                _swapchainWidths[eye] = viewConfigs[eye].RecommendedImageRectWidth;
                _swapchainHeights[eye] = viewConfigs[eye].RecommendedImageRectHeight;

                var swapchainInfo = new SwapchainCreateInfo();
                swapchainInfo.Type = StructureType.SwapchainCreateInfo;
                swapchainInfo.UsageFlags = SwapchainUsageFlags.ColorAttachmentBit | SwapchainUsageFlags.SampledBit;
                swapchainInfo.Format = preferredFormat;
                swapchainInfo.SampleCount = viewConfigs[eye].RecommendedSwapchainSampleCount;
                swapchainInfo.Width = _swapchainWidths[eye];
                swapchainInfo.Height = _swapchainHeights[eye];
                swapchainInfo.FaceCount = 1;
                swapchainInfo.ArraySize = 1;
                swapchainInfo.MipCount = 1;

                var result = _xr.CreateSwapchain(_session, ref swapchainInfo, ref _swapchains[eye]);
                if (result != Result.Success) return false;

                _renderTargets[eye] = new RenderTarget2D(
                    Application.GraphicsDevice,
                    (int)_swapchainWidths[eye],
                    (int)_swapchainHeights[eye],
                    false, SurfaceFormat.Color, DepthFormat.Depth24
                );
            }

            return true;
        }

        public override uint[] GetRenderTargetSize()
        {
            if (_swapchainWidths[0] == 0)
                return new uint[] { 1832, 1920 }; // Meta Quest 2 default

            return new uint[] { _swapchainWidths[0], _swapchainHeights[0] };
        }

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            if (_renderTargets[eye] != null)
                return _renderTargets[eye];

            var size = GetRenderTargetSize();
            _renderTargets[eye] = new RenderTarget2D(
                Application.GraphicsDevice,
                (int)size[0], (int)size[1],
                false, SurfaceFormat.Color, DepthFormat.Depth24
            );
            return _renderTargets[eye];
        }

        public override Matrix GetProjectionMatrix(int eye)
        {
            if (!_sessionRunning || _views[eye].Fov.AngleRight == 0)
                return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1.0f, 0.1f, 1000.0f);

            var fov = _views[eye].Fov;
            return CreateProjectionFromFov(fov.AngleLeft, fov.AngleRight, fov.AngleUp, fov.AngleDown, 0.1f, 1000.0f);
        }

        public override Matrix GetViewMatrix(int eye, Matrix parent)
        {
            if (!_sessionRunning) return parent;

            var pose = _views[eye].Pose;
            var pos = new Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z);
            var rot = new Quaternion(pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W);
            var viewMatrix = Matrix.CreateFromQuaternion(rot) * Matrix.CreateTranslation(pos);
            Matrix.Invert(ref viewMatrix, out viewMatrix);
            return viewMatrix * parent;
        }

        public override float GetRenderTargetAspectRatio(int eye)
        {
            if (_swapchainHeights[eye] == 0) return 1.0f;
            return (float)_swapchainWidths[eye] / _swapchainHeights[eye];
        }

        public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
            if (!_sessionRunning) return -1;

            for (int eye = 0; eye < EyeCount; eye++)
            {
                uint imageIndex = 0;
                var acquireInfo = new SwapchainImageAcquireInfo(StructureType.SwapchainImageAcquireInfo);
                _xr.AcquireSwapchainImage(_swapchains[eye], ref acquireInfo, ref imageIndex);

                var waitInfo = new SwapchainImageWaitInfo(timeout: long.MaxValue);
                _xr.WaitSwapchainImage(_swapchains[eye], ref waitInfo);

                // Platform-specific: copy MonoGame render target to swapchain image texture here.

                var releaseInfo = new SwapchainImageReleaseInfo(StructureType.SwapchainImageReleaseInfo);
                _xr.ReleaseSwapchainImage(_swapchains[eye], ref releaseInfo);
            }

            var projViews = stackalloc CompositionLayerProjectionView[EyeCount];
            for (int eye = 0; eye < EyeCount; eye++)
            {
                projViews[eye].Type = StructureType.CompositionLayerProjectionView;
                projViews[eye].Pose = _views[eye].Pose;
                projViews[eye].Fov = _views[eye].Fov;
                projViews[eye].SubImage = new SwapchainSubImage(
                    swapchain: _swapchains[eye],
                    imageRect: new Rect2Di(new Offset2Di(0, 0), new Extent2Di((int)_swapchainWidths[eye], (int)_swapchainHeights[eye])),
                    imageArrayIndex: 0
                );
            }

            var projLayer = new CompositionLayerProjection();
            projLayer.Type = StructureType.CompositionLayerProjection;
            projLayer.Space = _referenceSpace;
            projLayer.ViewCount = EyeCount;
            projLayer.Views = projViews;

            var layerPtr = (CompositionLayerBaseHeader*)&projLayer;
            var endInfo = new FrameEndInfo();
            endInfo.Type = StructureType.FrameEndInfo;
            endInfo.DisplayTime = _frameState.PredictedDisplayTime;
            endInfo.EnvironmentBlendMode = EnvironmentBlendMode.Opaque;
            endInfo.LayerCount = 1;
            endInfo.Layers = &layerPtr;

            _xr.EndFrame(_session, ref endInfo);
            return 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_xr == null) return;

            var eventData = new EventDataBuffer();
            eventData.Type = StructureType.EventDataBuffer;
            while (_xr.PollEvent(_instance, ref eventData) == Result.Success)
            {
                if (eventData.Type == StructureType.EventDataSessionStateChanged)
                    HandleSessionStateChange(ref eventData);

                eventData.Type = StructureType.EventDataBuffer;
            }

            if (_sessionRunning)
                BeginFrame();
        }

        private void HandleSessionStateChange(ref EventDataBuffer eventData)
        {
            fixed (EventDataBuffer* ptr = &eventData)
            {
                var stateEvent = (EventDataSessionStateChanged*)ptr;
                switch (stateEvent->State)
                {
                    case SessionState.Ready:
                        var beginInfo = new SessionBeginInfo();
                        beginInfo.Type = StructureType.SessionBeginInfo;
                        beginInfo.PrimaryViewConfigurationType = ViewConfigurationType.PrimaryStereo;
                        _xr.BeginSession(_session, ref beginInfo);
                        _sessionRunning = true;
                        break;
                    case SessionState.Stopping:
                        _xr.EndSession(_session);
                        _sessionRunning = false;
                        break;
                }
            }
        }

        private void BeginFrame()
        {
            var frameWaitInfo = new FrameWaitInfo(StructureType.FrameWaitInfo);
            _frameState = new FrameState(StructureType.FrameState);
            _xr.WaitFrame(_session, ref frameWaitInfo, ref _frameState);

            var beginInfo = new FrameBeginInfo(StructureType.FrameBeginInfo);
            _xr.BeginFrame(_session, ref beginInfo);

            if (_frameState.ShouldRender == 1)
                LocateViews();
        }

        private void LocateViews()
        {
            var viewLocateInfo = new ViewLocateInfo();
            viewLocateInfo.Type = StructureType.ViewLocateInfo;
            viewLocateInfo.ViewConfigurationType = ViewConfigurationType.PrimaryStereo;
            viewLocateInfo.DisplayTime = _frameState.PredictedDisplayTime;
            viewLocateInfo.Space = _referenceSpace;

            var viewState = new ViewState();
            viewState.Type = StructureType.ViewState;
            uint viewCount = EyeCount;

            fixed (View* viewsPtr = _views)
                _xr.LocateView(_session, ref viewLocateInfo, ref viewState, viewCount, ref viewCount, viewsPtr);
        }

        private static Matrix CreateProjectionFromFov(float left, float right, float up, float down, float nearZ, float farZ)
        {
            float tanLeft = MathF.Tan(left);
            float tanRight = MathF.Tan(right);
            float tanUp = MathF.Tan(up);
            float tanDown = MathF.Tan(down);

            float w = tanRight - tanLeft;
            float h = tanUp - tanDown;

            return new Matrix(
                2.0f / w, 0, 0, 0,
                0, 2.0f / h, 0, 0,
                (tanRight + tanLeft) / w, (tanUp + tanDown) / h, -(farZ + nearZ) / (farZ - nearZ), -1,
                0, 0, -(2.0f * farZ * nearZ) / (farZ - nearZ), 0
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _xr != null)
            {
                for (int i = 0; i < EyeCount; i++)
                {
                    if (_swapchains[i].Handle != 0)
                        _xr.DestroySwapchain(_swapchains[i]);
                    _renderTargets[i]?.Dispose();
                }

                if (_referenceSpace.Handle != 0)
                    _xr.DestroySpace(_referenceSpace);

                if (_session.Handle != 0)
                    _xr.DestroySession(_session);

                if (_instance.Handle != 0)
                    _xr.DestroyInstance(_instance);

                _xr.Dispose();
                _xr = null;
            }

            base.Dispose(disposing);
        }
    }
}
