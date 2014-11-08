using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace C3DE.Inputs
{
    public class TouchComponent : GameComponent
    {
        private TouchCollection _touchState;
        private TouchCollection _lastTouchState;
        private Vector2[] _position;
        private Vector2[] _lastPosition;
        private bool[] _pressed;
        private bool[] _moved;
        private bool[] _released;
        private float[] _pressure;
        private int _maxFingerPoints;
        private bool _needUpdate;
        private Vector2 _cacheVec2;
        private bool _needReset;

        /// <summary>
        /// Determine whether the touch input is available.
        /// </summary>
        public bool Available
        {
            get { return TouchPanel.IsGestureAvailable; }
        }

        /// <summary>
        /// Gets or sets the dead zone.
        /// </summary>
        public float DeadZone { get; set; }

        public int MaxFingerPoints
        {
            get { return _maxFingerPoints; }
            set
            {
                if (_maxFingerPoints != value)
                {
                    _maxFingerPoints = Math.Min(Math.Max(0, value), TouchPanel.GetCapabilities().MaximumTouchCount);
                    _needUpdate = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum values for the delta.
        /// </summary>
        public float MaxDelta { get; set; }

        /// <summary>
        /// Gets or sets the sensibility of the delta.
        /// </summary>
        public Vector2 Sensitivity { get; set; }

        /// <summary>
        /// Gets the number of finger on the screen.
        /// </summary>
        public int TouchCount
        {
            get { return _touchState.Count; }
        }

        public TouchComponent(Game game)
            : base(game)
        {
            _touchState = TouchPanel.GetState();
            _lastTouchState = _touchState;

            if (TouchPanel.GetCapabilities().IsConnected)
                _maxFingerPoints = TouchPanel.GetCapabilities().MaximumTouchCount;
            else
                _maxFingerPoints = 0;

            Sensitivity = Vector2.One;
            MaxDelta = 100;
            DeadZone = 1;
            _needUpdate = false;
            _needReset = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            _position = new Vector2[MaxFingerPoints];
            _lastPosition = new Vector2[MaxFingerPoints];
            _pressed = new bool[MaxFingerPoints];
            _moved = new bool[MaxFingerPoints];
            _released = new bool[MaxFingerPoints];
            _pressure = new float[MaxFingerPoints];

            for (int i = 0; i < MaxFingerPoints; i++)
            {
                _position[i] = Vector2.Zero;
                _lastPosition[i] = Vector2.Zero;
                _pressed[i] = false;
                _moved[i] = false;
                _released[i] = false;
                _pressure[i] = 0.0f;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_needUpdate)
            {
                Initialize();
                _needUpdate = false;
            }

            _lastTouchState = _touchState;
            _touchState = TouchPanel.GetState();
       
            if (MaxFingerPoints > 0 && _touchState.Count > 0)
            {
                int touchCount = _touchState.Count;

                for (int i = 0; i < MaxFingerPoints; i++)
                {
                    if (i < touchCount)
                        UpdateTouchState(i);
                    else
                        RestoreTouchState(i);
                }

                _needReset = true;
            }
            else if (_needReset)
            {
                for (int i = 0; i < MaxFingerPoints; i++)
                    RestoreTouchState(i);

                _needReset = false;
            }
        }

        private void UpdateTouchState(int index)
        {
            _lastPosition[index].X = _position[index].X;
            _lastPosition[index].Y = _position[index].Y;
            _position[index].X = _touchState[index].Position.X;
            _position[index].Y = _touchState[index].Position.Y;

            _pressed[index] = _touchState[index].State == TouchLocationState.Pressed;
            _moved[index] = _touchState[index].State == TouchLocationState.Moved;
            _released[index] = _touchState[index].State == TouchLocationState.Released || _touchState[index].State == TouchLocationState.Invalid;
            _pressure[index] = _touchState[index].Pressure;
        }

        public void RestoreTouchState(int index = 0)
        {
            _lastPosition[index].X = 0;
            _lastPosition[index].Y = 0;
            _position[index].X = 0;
            _position[index].Y = 0;

            _pressed[index] = false;
            _moved[index] = false;
            _released[index] = false;
            _pressure[index] = 0.0f;
        }

        public bool Pressed(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return false;

            return _pressed[id];
        }

        public bool Released(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return false;

            return _released[id];
        }

        public bool Moved(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return false;

            return _moved[id];
        }

        public float Pressure(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return 0.0f;

            return _pressure[id];
        }

        public Vector2 Delta(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return Vector2.Zero;

			// For preventing bad delta
			if (_lastTouchState.Count - 1 < id || _lastTouchState.Count == 0) 
				_lastPosition [id] = _position [id];

            _cacheVec2 = _position[id] - _lastPosition[id];
            _cacheVec2.X = Math.Abs(_cacheVec2.X);
            _cacheVec2.Y = Math.Abs(_cacheVec2.Y);

            if (_cacheVec2.X > DeadZone && _cacheVec2.X < MaxDelta)
                _cacheVec2.X = (_position[id].X - _lastPosition[id].X) * Sensitivity.X;

            if (_cacheVec2.Y > DeadZone && _cacheVec2.Y < MaxDelta)
                _cacheVec2.Y = (_position[id].Y - _lastPosition[id].Y) * Sensitivity.Y;

            return _cacheVec2;
        }

        public Vector2 GetPosition(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return Vector2.Zero;

            return _position[id];
        }

        public Vector2 GetLastPosition(int id = 0)
        {
            if (id >= MaxFingerPoints)
                return Vector2.Zero;

            return _lastPosition[id];
        }

        public bool JustPressed(int id = 0)
        {
            if (id >= _lastTouchState.Count)
                return false;

            return (_lastTouchState[id].State == TouchLocationState.Pressed || _lastTouchState[id].State == TouchLocationState.Moved) && (_touchState[id].State == TouchLocationState.Released);
        }
    }
}