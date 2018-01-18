using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OculusRiftSample
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
        public HmdInfo HmdInfo;
        public HeadTracking HeadTracking;

        public bool Initialized { get; private set; }

        Matrix[] ProjectionMatrix = new Matrix[2];     // one for each eye
        public Point[] RenderTargetRes = new Point[2]; // one for each eye

        GraphicsDevice graphicsDevice;

        // the following functions should be called in order
        public int Init(GraphicsDevice gd)
        {
            graphicsDevice = gd;

            IntPtr dxDevice, dxContext;
            graphicsDevice.GetNativeDxDeviceAndContext(out dxDevice, out dxContext);

            int result;

            if ((result = NativeRift.Init(dxDevice, dxContext)) < 0)
                return result;

            HmdInfo = NativeRift.GetHmdInfo();

            FovPort fovLeft = HmdInfo.DefaultFovLeft;
            FovPort fovRight = HmdInfo.DefaultFovRight;

            Point recommendTexResLeft = Point.Zero;
            Point recommendTexResRight = Point.Zero;

            NativeRift.GetRecommendedRenderTargetRes(fovLeft, fovRight, 1,
                ref recommendTexResLeft, ref recommendTexResRight);

            RenderTargetRes[0] = recommendTexResLeft;
            RenderTargetRes[1] = recommendTexResRight;

            if ((result = NativeRift.CreateDXSwapChains(recommendTexResLeft, recommendTexResRight, fovLeft, fovRight)) < 0)
                return result;

            for (int eye = 0; eye < 2; eye++)
                ProjectionMatrix[eye] = NativeRift.GetProjectionMatrix(eye, 0.1f, 1000, 0);

            Initialized = true;

            return 0;
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye,
            SurfaceFormat surfaceFormat = SurfaceFormat.ColorSRgb,
            DepthFormat depthFormat = DepthFormat.Depth24Stencil8)
        {
            Point res = RenderTargetRes[eye];
            return new RenderTarget2D(graphicsDevice, res.X, res.Y, false, surfaceFormat, depthFormat);
        }

        public HeadTracking TrackHead(int frame = 0)
        {
            return HeadTracking = NativeRift.TrackHead(frame);
        }

        public int SubmitRenderTargets(RenderTarget2D rtLeft, RenderTarget2D rtRight, int frame = 0)
        {
            IntPtr dxTexLeft = rtLeft.GetNativeDxResource();
            IntPtr dxTexRight = rtRight.GetNativeDxResource();

            return NativeRift.SubmitRenderTargets(dxTexLeft, dxTexRight, frame);
        }

        public void Shutdown()
        {
            NativeRift.Shutdown();
        }

        // matrices for rendering
        public Matrix GetEyePose(int eye, Matrix playerPose)
        {
            var mat = Matrix.Transpose(eye == 0 ? HeadTracking.EyePoseLeft : HeadTracking.EyePoseRight);
            return mat * playerPose;
        }

        public Matrix GetEyeViewMatrix(int eye, Matrix playerPose)
        {
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
        public static extern int Init(IntPtr dxDevice, IntPtr dxContext);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HmdInfo GetHmdInfo();

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetRecommendedRenderTargetRes(
            FovPort fovLeft, FovPort fovRight, float pixelsPerDisplayPixel,
            ref Point texResLeft, ref Point texResRight);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateDXSwapChains(
            Point texResLeft, Point texResRight, FovPort fovLeft, FovPort fovRight);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Matrix GetProjectionMatrix(
            int eye, float nearClip, float farClip, uint projectionModeFlags);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HeadTracking TrackHead(int frame);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SubmitRenderTargets(
            IntPtr dxTexResourceLeft, IntPtr dxTexResourceRight, int frame);

        [DllImport("OculusRift.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
    }
}

