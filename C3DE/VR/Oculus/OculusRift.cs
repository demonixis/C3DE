using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.VR
{
    public struct HmdInfo
    {
        public int Type;
        public int VendorId;
        public int ProductId;
        public int FirmwareMajor;
        public int FirmwareMinor;
        public Point DisplayResolution;
        public float DisplayRefreshRate;
        public FovPort DefaultFovLeft;
        public FovPort DefaultFovRight;
        public FovPort MaxFovLeft;
        public FovPort MaxFovRight;
        public uint AvailableHmdCaps;
        public uint DefaultHmdCaps;
        public uint AvailableTrackingCap;
        public uint DefaultTrackingCaps;
    };

    public struct HeadTracking
    {
        public uint StatusFlags;
        public Matrix HeadPose;
        public Matrix EyePoseLeft;
        public Matrix EyePoseRight;
    };

    public struct FovPort
    {
        public float UpTan;    // The tangent of the angle between the viewing vector and the top edge of the field of view.
        public float DownTan;  // The tangent of the angle between the viewing vector and the bottom edge of the field of view.
        public float LeftTan;  // The tangent of the angle between the viewing vector and the left edge of the field of view.
        public float RightTan; // The tangent of the angle between the viewing vector and the right edge of the field of view.
    }

    public class OculusRift
    {
        private Matrix[] ProjectionMatrix = new Matrix[2];     // one for each eye
        private Point[] RenderTargetRes = new Point[2]; // one for each eye
        private HeadTracking HeadTracking;
        private IntPtr[] renderTargetPtrs = new IntPtr[2];
        private int[] renderTargetIds = new int[2];
        private HmdInfo HmdInfo;
        private GraphicsDevice _graphicsDevice;

        // the following functions should be called in order
        public int Initialize(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;

#if !WINDOWS && !DESKTOP
            return 0;
#endif

            var result = 0;

#if WINDOWS
            IntPtr dxDevice, dxContext;
            _graphicsDevice.GetNativeDxDeviceAndContext(out dxDevice, out dxContext);

            result = NativeRift.InitializeDX(dxDevice, dxContext);
            if (result < 0)
                return result;
#elif DESKTOP
            result = NativeRift.InitializeGL();
            if (result < 0)
                return result;
#endif
            HmdInfo = NativeRift.GetHmdInfo();

            var fovLeft = HmdInfo.DefaultFovLeft;
            var fovRight = HmdInfo.DefaultFovRight;
            var recommendTexResLeft = Point.Zero;
            var recommendTexResRight = Point.Zero;

            NativeRift.GetRecommendedRenderTargetRes(fovLeft, fovRight, 1, ref recommendTexResLeft, ref recommendTexResRight);

            RenderTargetRes[0] = recommendTexResLeft;
            RenderTargetRes[1] = recommendTexResRight;
#if WINDOWS
            result = NativeRift.CreateDXSwapChains(recommendTexResLeft, recommendTexResRight, fovLeft, fovRight);
            if (result < 0)
                return result;
#else
            result = NativeRift.CreateGLSwapChains(recommendTexResLeft, recommendTexResRight, fovLeft, fovRight);
            if (result < 0)
                return result;
#endif

            for (var eye = 0; eye < 2; eye++)
                ProjectionMatrix[eye] = NativeRift.GetProjectionMatrix(eye, 0.1f, 1000, 0);

            return 0;
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye, SurfaceFormat surfaceFormat = SurfaceFormat.ColorSRgb, DepthFormat depthFormat = DepthFormat.Depth24Stencil8)
        {
            var renderTarget = new RenderTarget2D(_graphicsDevice, RenderTargetRes[eye].X, RenderTargetRes[eye].Y, false, surfaceFormat, depthFormat);
#if WINDOWS
            var info = typeof(RenderTarget2D).GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var value = (SharpDX.Direct3D11.Resource)info.GetValue(renderTarget);
            renderTargetPtrs[eye] = value.NativePointer;
#elif DESKTOP
            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            renderTargetIds[eye] = (int)info.GetValue(renderTarget);
#endif
            return renderTarget;
        }

        public HeadTracking TrackHead(int frame = 0)
        {
#if !WINDOWS && !DESKTOP
            return HeadTracking;
#endif
            return HeadTracking = NativeRift.TrackHead(frame);
        }

        public int SubmitRenderTargets(RenderTarget2D rtLeft, RenderTarget2D rtRight, int frame = 0)
        {
#if WINDOWS
            return NativeRift.SubmitRenderTargetsDX(renderTargetPtrs[0], renderTargetPtrs[1], frame);
#elif DESKTOP
            return NativeRift.SubmitRenderTargsGL(renderTargetIds[0], renderTargetIds[1], frame);
#else
            return 0;
#endif
        }

        public void Shutdown()
        {
#if !WINDOWS && !DESKTOP
            return;
#endif
            NativeRift.Shutdown();
        }

        // matrices for rendering
        public Matrix GetEyePose(int eye, Matrix playerPose)
        {
#if !WINDOWS && !DESKTOP
            return Components.Camera.main.view;
#endif
            var mat = Matrix.Transpose(eye == 0 ? HeadTracking.EyePoseLeft : HeadTracking.EyePoseRight); 
            return mat * playerPose;
        }

        public Matrix GetEyeViewMatrix(int eye, Matrix playerPose)
        {
#if !WINDOWS && !DESKTOP
            return Components.Camera.main.projection;
#endif
            return Matrix.Invert(GetEyePose(eye, playerPose));
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            return Matrix.Transpose(ProjectionMatrix[eye]);
        }

        // helper functions
        public float GetRenderTargetAspectRatio(int eye)
        {
            return (float)(RenderTargetRes[eye].X) / RenderTargetRes[eye].Y;
        }
    }

    // interface to native C++ dll
    public static class NativeRift
    {
        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeGL();

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeDX(IntPtr dxDevice, IntPtr dxContext);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HmdInfo GetHmdInfo();

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetRecommendedRenderTargetRes(FovPort fovLeft, FovPort fovRight, float pixelsPerDisplayPixel, ref Point texResLeft, ref Point texResRight);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateGLSwapChains(Point texResLeft, Point texResRight, FovPort fovLeft, FovPort fovRight);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateDXSwapChains(Point texResLeft, Point texResRight, FovPort fovLeft, FovPort fovRight);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Matrix GetProjectionMatrix(int eye, float nearClip, float farClip, uint projectionModeFlags);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HeadTracking TrackHead(int frame);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SubmitRenderTargsGL(int rtLeft, int rtRight, int frame);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SubmitRenderTargetsDX(IntPtr dxTexResourceLeft, IntPtr dxTexResourceRight, int frame);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
    }
}

