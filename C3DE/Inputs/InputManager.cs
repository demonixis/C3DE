using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Inputs
{
    /// <summary>
    /// An input manager which works with input profiles. It can handle input for virtual axis or buttons
    /// for the keyboard, mouse, gamepad and touch.
    /// </summary>
    public class InputManager
    {
        private Dictionary<string, InputProfile> _inputProfiles;
        private InputProfile _cacheProfile;

        // Caches
        private float _cacheFloat;
        private Vector2 _cacheVector2;

        // In the future, we'll load them using a JSON file, or a XML file (but JSON is cooler)
        public InputManager()
        {
            SetProfiles(null);
        }

        /// <summary>
        /// Sets the collection of input profiles which can be used by the InputManager.
        /// The default behaviour will load default InputProfiles if profiles variable is null.
        /// </summary>
        /// <param name="profiles">An array of InputProfile.</param>
        public void SetProfiles(InputProfile[] profiles)
        {
            if (profiles == null)
            {
                var axis = InputProfile.CreateDefaultAxis();
                var buttons = InputProfile.CreateDefaultButtons();

                profiles = new InputProfile[axis.Length + buttons.Length];

                var aCount = axis.Length;
                var bCount = buttons.Length;

                for (int i = 0, l = aCount + bCount; i < l; i++)
                {
                    if (i < aCount)
                        profiles[i] = axis[i];
                    else
                        profiles[i] = buttons[i - aCount];
                }
            }

            int count = profiles.Length;

            _inputProfiles = new Dictionary<string, InputProfile>(count);

            for (int i = 0; i < count; i++)
            {
                if (_inputProfiles.ContainsKey(profiles[i].Name) || profiles[i].Name == string.Empty)
                    throw new Exception("An input profile must be unique and not empty.");

                _inputProfiles.Add(profiles[i].Name, profiles[i]);
            }
        }

        /// <summary>
        /// Gets the value for the given axis.
        /// </summary>
        /// <param name="axis">The identifier of the axis.</param>
        /// <returns>Returns the value of the axis [-1.0, 1.0].</returns>
        public float GetAxis(string axis)
        {
            _cacheFloat = 0;

            if (!_inputProfiles.ContainsKey(axis))
                return 0.0f;

            _cacheProfile = _inputProfiles[axis];

            if (_cacheProfile.Axis != InputProfileAxis.None)
            {
                if (Input.Touch.TouchCount == 0)
                {
                    if (_cacheProfile.HasKeyboard)
                    {
                        if (Input.Keys.Pressed(_cacheProfile.NegativeKey))
                            _cacheFloat = -1.0f;
                        else if (Input.Keys.Pressed(_cacheProfile.PositiveKey))
                            _cacheFloat = 1.0f;
                    }

                    if (_cacheProfile.HasMouse)
                    {
                        if (_cacheProfile.Axis == InputProfileAxis.X)
                            _cacheFloat = MathHelper.Clamp(Input.Mouse.Delta.X, -1.0f, 1.0f);
                        else
                            _cacheFloat = MathHelper.Clamp(Input.Mouse.Delta.Y, -1.0f, 1.0f);
                    }

                    if (_cacheProfile.HasGamepad)
                    {
                        _cacheVector2 = Input.Gamepad.GetAxis(_cacheProfile.PositiveButton);

                        if (_cacheProfile.Axis == InputProfileAxis.X)
                            _cacheFloat = _cacheVector2.X;
                        else
                            _cacheFloat = _cacheVector2.Y;
                    }
                }
                else if (_cacheProfile.HasTouch)
                {
                    if (_cacheProfile.TouchZone.Contains(Input.Touch.GetPosition()))
                        _cacheFloat = MathHelper.Clamp(Input.Touch.Delta().X, -1.0f, 1.0f);
                }
            }

            return _cacheFloat;
        }
    }
}
