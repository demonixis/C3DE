using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OSVR.MonoGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class MouselookInterface : OSVR.ClientKit.IInterface<XnaPose>
    {
        Viewport viewport;
        MouseState lastMouseState;
        Quaternion value = Quaternion.Identity;
        bool firstUpdate = true;
        const float RotationSpeed = .5f;
        float rotationX = 0f;
        float rotationY = 0f;

        public MouselookInterface(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void Update(GameTime gameTime)
        {
            // For some reason, Mouse.SetPosition throws an ObjectDisposedException
            // when the game quits. TODO: determine the cause and possible mitigations.
            // commenting out for now
            if(firstUpdate)
            {
                firstUpdate = false;
                Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);
                lastMouseState = Mouse.GetState();
            }
            else
            {
                var mouseState = Mouse.GetState();
                if (mouseState != lastMouseState)
                {
                    var t = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    var deltaX = mouseState.X - lastMouseState.X;
                    var deltaY = mouseState.Y - lastMouseState.Y;
                    rotationY -= (float)deltaX * RotationSpeed * t;
                    rotationX -= (float)deltaY * RotationSpeed * t;
                    if (rotationX > MathHelper.PiOver2) { rotationX = MathHelper.PiOver2; }
                    if (rotationX < -MathHelper.PiOver2) { rotationX = -MathHelper.PiOver2; }
                    Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);
                    lastMouseState = Mouse.GetState();
                }
            }
            value = Quaternion.CreateFromYawPitchRoll(rotationY, rotationX, 0f);
        }

        public OSVR.ClientKit.InterfaceState<XnaPose> GetState()
        {
            var pose = new XnaPose { Rotation = value, Position = Vector3.Zero };
            return new OSVR.ClientKit.InterfaceState<XnaPose>
            {
                Timestamp = new OSVR.ClientKit.TimeValue(),
                Value = pose,
            };
        }

        event OSVR.ClientKit.StateChangedHandler<XnaPose> OSVR.ClientKit.IInterface<XnaPose>.StateChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }
    }
}
