using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Inputs
{
    public class GamepadComponent : GameComponent
    {
#if ANDROID
        private int _gamepadCount = 1;
#else
        private int _gamepadCount = 4;
#endif
        private GamePadState[] _gpState;
        private GamePadState[] _previousGpState;
        private Vector2 _sensibility;
        private Vector2 _deadZone;
        private Vector2 _tmpVector;

        public Vector2 Sensitivity
        {
            get { return _sensibility; }
            set
            {
                if (value.X >= 0.0f && value.Y >= 0.0f)
                    _sensibility = value;
            }
        }

        public Vector2 DeadZone
        {
            get { return _deadZone; }
            set
            {
                if (value.X >= 0.0f && value.Y >= 0.0f)
                    _deadZone = value;
            }
        }

        public GamepadComponent(Game game)
            : base(game)
        {
            _gpState = new GamePadState[4];
            _previousGpState = new GamePadState[4];

            for (int i = 0; i < _gamepadCount; i++)
            {
                _gpState[i] = GamePad.GetState((PlayerIndex)i);
                _previousGpState[i] = _gpState[i];
            }

            _deadZone = new Vector2(0.4f);
            _sensibility = Vector2.One;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _gamepadCount; i++)
            {
                _previousGpState[i] = _gpState[i];
                _gpState[i] = GamePad.GetState((PlayerIndex)i);
            }

            base.Update(gameTime);
        }

        public bool IsConnected(PlayerIndex index = PlayerIndex.One)
        {
            return _gpState[(int)index].IsConnected;
        }

        public bool Pressed(Buttons button, PlayerIndex index = PlayerIndex.One)
        {
            return _gpState[(int)index].IsButtonDown(button);
        }

        public bool Released(Buttons button, PlayerIndex index = PlayerIndex.One)
        {
            return _gpState[(int)index].IsButtonUp(button);
        }

        public bool JustPressed(Buttons button, PlayerIndex index = PlayerIndex.One)
        {
            return _gpState[(int)index].IsButtonUp(button) && _previousGpState[(int)index].IsButtonDown(button);
        }

        public Vector2 GetAxis(Buttons button, PlayerIndex index = PlayerIndex.One)
        {

            if (button == Buttons.LeftTrigger || button == Buttons.RightTrigger)
            {
                _tmpVector.X = _gpState[(int)index].Triggers.Left;
                _tmpVector.Y = _gpState[(int)index].Triggers.Right;
            }

            else if (button == Buttons.LeftStick)
                _tmpVector = ThumbSticks(true, index);
            
            else if (button == Buttons.RightStick)
                _tmpVector = ThumbSticks(false, index);

            return _tmpVector;
        }

        public float Triggers(bool left = true, PlayerIndex index = PlayerIndex.One)
        {
            if (left)
                return _gpState[(int)index].Triggers.Left;
            else
                return _gpState[(int)index].Triggers.Right;
        }

        public Vector2 ThumbSticks(bool left = true, PlayerIndex index = PlayerIndex.One)
        {
            if (left)
                return CheckDeadZone(_gpState[(int)index].ThumbSticks.Left) * _sensibility;
            else
                return CheckDeadZone(_gpState[(int)index].ThumbSticks.Right) * _sensibility;
        }

        #region Digital pad

        public bool Up(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.DPadUp, index);
        }

        public bool Down(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.DPadDown, index);
        }

        public bool Left(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.DPadLeft);
        }

        public bool Right(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.DPadRight);
        }

        #endregion

        #region Buttons

        public bool A(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.A);
        }

        public bool B(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.B);
        }

        public bool X(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.X);
        }

        public bool Y(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.Y);
        }

        public bool Start(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.Start);
        }

        public bool Back(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.Back);
        }

        public bool Guide(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.BigButton);
        }

        #endregion

        #region Triggers

        public bool LeftTrigger(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftTrigger);
        }

        public bool LeftShoulder(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftShoulder);
        }

        public bool RightTrigger(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightTrigger);
        }

        public bool RightShoulder(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightShoulder);
        }

        public float LeftTriggerValue(PlayerIndex index = PlayerIndex.One)
        {
            return Triggers(true, index);
        }

        public float RightTriggerValue(PlayerIndex index = PlayerIndex.One)
        {
            return Triggers(false, index);
        }

        #endregion

        #region Left Thumbstick

        public bool LeftStick(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftStick);
        }

        public bool LeftStickUp(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftThumbstickUp);
        }

        public bool LeftStickDown(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftThumbstickDown);
        }

        public bool LeftStickLeft(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftThumbstickLeft);
        }

        public bool LeftStickRight(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.LeftThumbstickRight);
        }

        public Vector2 LeftStickValue(PlayerIndex index = PlayerIndex.One)
        {
            return ThumbSticks(true, index);
        }

        #endregion

        #region Right Thumbstick

        public bool RightStick(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightStick);
        }

        public bool RightStickUp(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightThumbstickUp);
        }

        public bool RightStickDown(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightThumbstickDown);
        }

        public bool RightStickLeft(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightThumbstickLeft);
        }

        public bool RightStickRight(PlayerIndex index = PlayerIndex.One)
        {
            return Pressed(Buttons.RightThumbstickRight);
        }

        public Vector2 RightStickValue(PlayerIndex index = PlayerIndex.One)
        {
#if LINUX
			var value = ThumbSticks(false, index);
			var temp = value.X;
			value.X = -value.Y;
			value.Y = -temp;
			return value;
#else
            return ThumbSticks(false, index);
#endif
        }

        #endregion

        private Vector2 CheckDeadZone(Vector2 vector)
        {
            _tmpVector.X = (Math.Abs(vector.X) >= _deadZone.X) ? vector.X : 0.0f;
            _tmpVector.Y = (Math.Abs(vector.Y) >= _deadZone.Y) ? vector.Y : 0.0f;
            return _tmpVector;
        }
    }
}