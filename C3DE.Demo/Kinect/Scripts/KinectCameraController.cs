using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Demo.Scripts;
using C3DE.UI;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Kinect.Scripts
{
    class UIKinectHand
    {
        private Rectangle _rect;
        private Texture2D _texture;
        private bool _active;
        private float _activeValue;

        public UIKinectHand(int id, float activeValue)
        {
            _texture = Application.Content.Load<Texture2D>(id == 0 ? "Misc/leftHand" : "Misc/rightHand");
            _rect = new Rectangle(_texture.Width, _texture.Height, _texture.Width, _texture.Height);

            if (id == 1)
                _rect.X = Screen.Width - _texture.Width;

            _active = false;
            _activeValue = activeValue;
        }

        public void UpdatePosition(Vector3 position)
        {
            _rect.X = (int)position.X;
            _rect.Y = (int)position.Y;

            _active = (position.Z * 100.0f <= _activeValue);
        }

        public void Draw(GUI gui)
        {
            gui.DrawTexture(ref _rect, _texture, _active ? Color.CornflowerBlue : Color.White);
        }
    }

    public class KinectCameraController : OrbitController
    {
        private UIKinectHand _uiLeftHand;
        private UIKinectHand _uiRightHand;
        private Vector2 _infoPos;
        private Rectangle _logoRect;
        private Texture2D _logoTexture;
        private Camera _camera;
        private KinectManager _kinect;

        public override void Start()
        {
            base.Start();

            _uiLeftHand = new UIKinectHand(0, 110);
            _uiRightHand = new UIKinectHand(1, 110);

            _infoPos = new Vector2(10.0f);

            _logoTexture = Application.Content.Load<Texture2D>("Textures/Kinect_Logo");
            _logoRect = new Rectangle(Screen.Width - _logoTexture.Width - 10, Screen.Height - _logoTexture.Height - 10, _logoTexture.Width, _logoTexture.Height);

            _camera = sceneObject.Scene.MainCamera;

            _kinect = new KinectManager(Screen.Width, Screen.Height, 100);

            Velocity = 0;
            AngularVelocity = 0;
        }

        protected override void UpdateInputs()
        {

            if (_kinect.User.GetPosition(JointType.HandRight).Z * 100.0f < 110.0f)
            {
                angleVelocity.X -= RotationSpeed * _kinect.User.Delta(JointType.HandRight).X * Time.DeltaTime;
                angleVelocity.Y -= RotationSpeed * _kinect.User.Delta(JointType.HandRight).Y * Time.DeltaTime;
            }


            if (_kinect.User.GetPosition(JointType.HandLeft).Z * 100.0f < 110.0f)
            {
                positionVelicoty.X += StrafeSpeed * _kinect.User.Delta(JointType.HandLeft).X * Time.DeltaTime * 0.5f;
                positionVelicoty.Y += StrafeSpeed * _kinect.User.Delta(JointType.HandLeft).Y * Time.DeltaTime * 0.5f;
            }

            _uiLeftHand.UpdatePosition(_kinect.User.GetPosition(JointType.HandLeft));
            _uiRightHand.UpdatePosition(_kinect.User.GetPosition(JointType.HandRight));
        }

        public override void OnGUI(GUI gui)
        {
            gui.DrawTexture(ref _logoRect, _logoTexture);
            gui.Label(ref _infoPos, "Kinect sensor is " + (_kinect.IsAvailable ? "connected" : "not connected"));

            _uiLeftHand.Draw(gui);
            _uiRightHand.Draw(gui);
        }
    }
}
