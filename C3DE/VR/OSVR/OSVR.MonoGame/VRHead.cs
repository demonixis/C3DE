/* OSVR-Unity Connection
 * 
 * <http://sensics.com/osvr>
 * Copyright 2014 Sensics, Inc.
 * All rights reserved.
 * 
 * Final version intended to be licensed under Apache v2.0
 */
/// <summary>
/// Author: Bob Berkebile
/// Email: bob@bullyentertainment.com || bobb@pixelplacement.com
/// </summary>

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Diagnostics;

namespace OSVR
{
    namespace MonoGame
    {

        public enum ViewMode { Stereo, Mono };

        public interface IStereoSceneDrawer
        {
            void DrawScene(GameTime gameTime, Viewport viewport, Matrix stereoTransform, Matrix view, Matrix projection);
        }

        public class VRHead
        {
            public ViewMode ViewMode { get; set; }

            private float worldUnitsPerMeter = 1f;
            public float WorldUnitsPerMeter { get { return worldUnitsPerMeter; } set { worldUnitsPerMeter = value; } }

            private float ipd = .061f;
            public float IPDInMeters { get { return ipd; } set { ipd = value; } }
            public float IPDInWorldUnits { get { return IPDInMeters * WorldUnitsPerMeter; } set { ipd = value / WorldUnitsPerMeter; } }


            public float VerticalFieldOfView { get; private set; }

            public VREye LeftEye { get; private set; }
            public VREye RightEye { get; private set; }

            public RenderTarget2D renderTargetLeft;
            public RenderTarget2D renderTargetRight;

            readonly GraphicsDeviceManager graphicsDeviceManager;
            DeviceDescriptor deviceDescriptor;
            readonly OSVR.ClientKit.ClientContext context;

            OSVR.ClientKit.IInterface<XnaPose> pose;
            public OSVR.ClientKit.IInterface<XnaPose> Pose
            {
                get { return pose; }
                set 
                { 
                    pose = value;
                    LeftEye.Pose = value;
                    RightEye.Pose = value;
                }
            }

            public VRHead(GraphicsDeviceManager graphicsDeviceManager,
                OSVR.ClientKit.ClientContext context,
                OSVR.ClientKit.IInterface<XnaPose> pose)
            {
                this.pose = pose;
                this.graphicsDeviceManager = graphicsDeviceManager;
                this.context = context;
                // TODO: Provide a way to pass in an explicit json value?
                GetDeviceDescription();
            }

            public void Update()
            {
                UpdateStereoAmount();
            }

            void UpdateStereoAmount()
            {
                LeftEye.Translation = Vector3.Left * (IPDInWorldUnits * 0.5f);
                RightEye.Translation = Vector3.Right * (IPDInWorldUnits * 0.5f);
            }

            private void GetDeviceDescription()
            {
                var displayJson = context.getStringParameter("/display");
                deviceDescriptor = DeviceDescriptor.Parse(displayJson);
                if(deviceDescriptor != null)
                {
                    //// temporary overrides to simulate OSVR HDK
                    //// without actually reading in the json value
                    //deviceDescriptor.DisplayMode = "horz_side_by_side";
                    //deviceDescriptor.MonocularHorizontal = 90f;
                    //deviceDescriptor.MonocularVertical = 101.25f;
                    //deviceDescriptor.OverlapPercent = 100f;
                    //deviceDescriptor.PitchTilt = 0f;
                    //deviceDescriptor.K1Red = 0f;
                    //deviceDescriptor.K1Green = 0f;
                    //deviceDescriptor.K1Blue = 0f;
                    //deviceDescriptor.RightRoll = 0f;
                    //deviceDescriptor.LeftRoll = 0f;
                    //deviceDescriptor.CenterProjX = 0.5f;
                    //deviceDescriptor.CenterProjY = 0.5f;

                    LeftEye = new VREye(pose, Eye.Left, deviceDescriptor);
                    RightEye = new VREye(pose, Eye.Right, deviceDescriptor);

                    switch(deviceDescriptor.DisplayMode)
                    {
                        case "full_screen":
                            ViewMode = MonoGame.ViewMode.Mono;
                            break;
                        case "horz_side_by_side":
                        case "vert_side_by_side":
                        default:
                            ViewMode = MonoGame.ViewMode.Stereo;
                            break;
                    }
                    //SetResolution(deviceDescriptor.Width, deviceDescriptor.Height); // set resolution before FOV
                    VerticalFieldOfView = MathHelper.Clamp(deviceDescriptor.MonocularVertical, 0, 180);
                    // TODO: should we provide HorizontalFieldOfView?
                    //SetDistortion(deviceDescriptor.K1Red, deviceDescriptor.K1Green, deviceDescriptor.K1Blue,
                    //    deviceDescriptor.CenterProjX, deviceDescriptor.CenterProjY); //set distortion shader
                    
                    // if the view needs to be rotated 180 degrees, create a parent game object that is flipped
                    // 180 degrees on the z axis
                    if (deviceDescriptor.Rotate180 > 0)
                    {
                        LeftEye.RotatePi = true;
                        RightEye.RotatePi = true;
                    }

                    SetEyeRotation(deviceDescriptor.OverlapPercent, deviceDescriptor.MonocularHorizontal);
                    SetEyeRoll(deviceDescriptor.LeftRoll, deviceDescriptor.RightRoll);
                }
            }

            //private void SetDistortion(float k1Red, float k1Green, float k1Blue, float centerProjX, float centerProjY)
            //{
            //    if (_distortionEffect != null)
            //    {
            //        _distortionEffect.k1Red = k1Red;
            //        _distortionEffect.k1Green = k1Green;
            //        _distortionEffect.k1Blue = k1Blue;
            //        _distortionEffect.fullCenter = new Vector2(centerProjX, centerProjY);
            //    }
            //}

            //Set the Screen Resolution

            private RenderTarget2D MakeRenderTarget2D(int width, int height)
            {
                return new RenderTarget2D(
                    graphicsDevice: graphicsDeviceManager.GraphicsDevice,
                    width: width / 2,
                    height: height,
                    mipMap: false,
                    preferredFormat: graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferFormat, 
                    preferredDepthFormat: DepthFormat.Depth24);
            }

            private void SetResolution(int width, int height)
            {
                //set the resolution
                graphicsDeviceManager.PreferredBackBufferWidth = width;
                graphicsDeviceManager.PreferredBackBufferHeight = height;
                renderTargetLeft = MakeRenderTarget2D(width, height);
                renderTargetRight = MakeRenderTarget2D(width, height);
                if (!Debugger.IsAttached)
                {
                    graphicsDeviceManager.IsFullScreen = true;
                }
                graphicsDeviceManager.ApplyChanges();
            }

            // rotate each eye based on overlap percent and horizontal FOV
            // Formula: ((OverlapPercent/100) * hFOV)/2
            private void SetEyeRotation(float overlapPercent, float horizontalFov)
            {
                float overlap = overlapPercent * 0.01f * horizontalFov * 0.5f;

                // with a 90 degree FOV with 100% overlap, the eyes should not be rotated
                // compare rotationY with half of FOV

                float halfFOV = horizontalFov * 0.5f;
                float rotateYAmount = MathHelper.ToRadians(System.Math.Abs(overlap - halfFOV));

                LeftEye.EyeRotationY = -rotateYAmount;
                RightEye.EyeRotationY = rotateYAmount;
            }

            // rotate each eye on the z axis by the specified amount, in degrees
            private void SetEyeRoll(float leftRoll, float rightRoll)
            {
                LeftEye.EyeRoll = MathHelper.ToRadians(leftRoll);
                RightEye.EyeRoll = MathHelper.ToRadians(rightRoll);
            }

            // TODO: instead of an Action, maybe pass an interface?
            // I think that might be easier to use. Hard to document
            // what each action arguments should be.
            public void DrawScene(GameTime gameTime, SpriteBatch spriteBatch, IStereoSceneDrawer sceneDrawer)
            {
                DrawSceneForEye(LeftEye, gameTime, renderTargetLeft, sceneDrawer);
                DrawSceneForEye(RightEye, gameTime, renderTargetRight, sceneDrawer);

                var leftRectangle = new Rectangle(
                    LeftEye.Viewport.X, LeftEye.Viewport.Y,
                    LeftEye.Viewport.Width, LeftEye.Viewport.Height);

                var rightRectangle = new Rectangle(
                    RightEye.Viewport.X, RightEye.Viewport.Y,
                    LeftEye.Viewport.Width, RightEye.Viewport.Height);

                graphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque/*, null, null, null, distortionShader*/);
                spriteBatch.Draw(renderTargetLeft, leftRectangle, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque/*, null, null, null, distortionShader*/);
                spriteBatch.Draw(renderTargetRight, rightRectangle, Color.White);
                spriteBatch.End();
                // TODO: implement monoscopic rendering, which will basically just call DrawScene
                // once with the full ViewPort and a non-stereo view matrix/projection.
            }

            private void DrawSceneForEye(VREye eye, GameTime gameTime, RenderTarget2D renderTarget, IStereoSceneDrawer sceneDrawer)
            {
                graphicsDeviceManager.GraphicsDevice.SetRenderTarget(renderTarget);
                graphicsDeviceManager.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
                graphicsDeviceManager.GraphicsDevice.Clear(Color.Navy);
                sceneDrawer.DrawScene(
                    gameTime: gameTime, 
                    viewport: graphicsDeviceManager.GraphicsDevice.Viewport,
                    // Note: The transform is in view space, but VREye.Translation is in world
                    // space, so we need to negate it here.
                    stereoTransform: Matrix.CreateTranslation(Vector3.Negate(eye.Translation)), 
                    view: eye.Transform, 
                    projection: eye.Projection);
            }
        }
    }
}
