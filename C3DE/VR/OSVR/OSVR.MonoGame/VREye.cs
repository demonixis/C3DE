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

namespace OSVR
{
    namespace MonoGame
    {
        public enum Eye { Left, Right };

        public class VREye
        {
            private OSVR.ClientKit.IInterface<XnaPose> pose;
            public OSVR.ClientKit.IInterface<XnaPose> Pose 
            { 
                get { return pose; } 
                set { pose = value; } 
            }

            private readonly DeviceDescriptor deviceDescriptor;

            public Eye Eye { get; private set; }

            /// <summary>
            /// Eye rotation around the Y axis, in radians
            /// </summary>
            public float EyeRotationY { get; set; }

            /// <summary>
            /// Eye roll around the Z axis, in radians
            /// </summary>
            public float EyeRoll { get; set; }

            public Vector3 Translation { get; set; }
            
            public bool RotatePi { get; set; }

            // TODO: Should we cache this transform and recalculate it
            // on an Update method?
            public Matrix Transform
            {
                get
                {
                    // orientation matrix
                    var poseState = this.pose.GetState();
                    var translation = Matrix.CreateTranslation(Vector3.Negate(poseState.Value.Position));
                    var rotation = Matrix.CreateFromQuaternion(Quaternion.Inverse(poseState.Value.Rotation));
                    var viewMatrix = rotation * translation;

                    // eye device rotation
                    var pitch = EyeRotationY;
                    var roll = EyeRoll;
                    var yaw = RotatePi ? MathHelper.Pi : 0f;
                    var eyeRotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

                    var ret = eyeRotation * viewMatrix;
                    return ret;
                }
            }

            public Matrix Projection
            {
                get
                {
                    float aspectRatio = (float)deviceDescriptor.Width / (float)deviceDescriptor.Height;
                    // aspect ratio per eye depends on how many displays the HMD has
                    // for example, dSight has two 1920x1080 displays, so each eye should have 1.77 aspec
                    // whereas HDK has one 1920x1080 display, each eye should have 0.88 aspec (half of 1.77)
                    float aspectRatioPerEye = deviceDescriptor.NumDisplays == 1 ? aspectRatio * 0.5f : aspectRatio;
                    // set projection matrix for each eye.
                    
                    // Disabling the projection translation until I can verify the correct values to place here.
                    // The Unreal translation values were incorrect.

                    // TODO: calculate the actual lense separation offset based on the device metadata
                    //var projectionOffset = 0.151976421f;
                    //var projectionTranslation = Matrix.CreateTranslation(
                    //    Eye == MonoGame.Eye.Left ? projectionOffset : -projectionOffset,
                    //    0f, 0f);

                    // Camera.projectionMatrix = Matrix4x4.Perspective(_deviceDescriptor.MonocularVertical, aspectRatioPerEye, Camera.nearClipPlane, Camera.farClipPlane);
                    return /*projectionTranslation **/ Matrix.CreatePerspectiveFieldOfView(
                        fieldOfView: MathHelper.ToRadians(deviceDescriptor.MonocularHorizontal),
                        aspectRatio: aspectRatioPerEye,
                        nearPlaneDistance: 0.01f,
                        farPlaneDistance: 5000f);
                }
            }

            // TODO: Should we cache this Viewport and recalculate it
            // on an Update method?
            public Viewport Viewport {
                get
                {
                    switch(Eye)
                    {
                        case MonoGame.Eye.Left:
                            return new Viewport
                            {
                                MinDepth = 0,
                                MaxDepth = 1,
                                X = 0,
                                Y = 0,
                                Width = deviceDescriptor.Width / 2,
                                Height = deviceDescriptor.Height,
                            };
                        case MonoGame.Eye.Right:
                            return new Viewport
                            {
                                MinDepth = 0,
                                MaxDepth = 1,
                                X = deviceDescriptor.Width / 2,
                                Y = 0,
                                Width = deviceDescriptor.Width / 2,
                                Height = deviceDescriptor.Height,
                            };
                    }
                    throw new InvalidOperationException("Unexpected eye type.");
                }
            }

            public VREye(OSVR.ClientKit.IInterface<XnaPose> pose, Eye eye, DeviceDescriptor deviceDescriptor)
            {
                this.pose = pose;
                this.Eye = eye;
                this.deviceDescriptor = deviceDescriptor;
            }
        }
    }
}