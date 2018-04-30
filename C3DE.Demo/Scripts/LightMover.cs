using C3DE.Components;
using C3DE.Components.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class LightMover : Behaviour
    {
        private Vector3 translation;
        private Vector3 rotation;
        private Light _light;

        public override void Start()
        {
            _light = GetComponent<Light>();
        }

        public override void Update()
        {
            translation = Vector3.Zero;

            if (Input.Keys.Pressed(Keys.Up))
                translation.Z++;
            else if (Input.Keys.Pressed(Keys.Down))
                translation.Z--;

            if (Input.Keys.Pressed(Keys.Left))
                translation.X++;
            else if (Input.Keys.Pressed(Keys.Right))
                translation.X--;

            if (Input.Keys.Pressed(Keys.A))
                translation.Y++;
            else if (Input.Keys.Pressed(Keys.E))
                translation.Y--;

            if (Input.Keys.Pressed(Keys.I))
                _light.Angle += 0.01f;
            else if (Input.Keys.Pressed(Keys.K))
                _light.Angle -= 0.01f;

            if (Input.Keys.Pressed(Keys.J))
                rotation.X += 0.01f;
            else if (Input.Keys.Pressed(Keys.L))
                rotation.X -= 0.01f;

            if (Input.Keys.Pressed(Keys.Add))
                _light.Radius += 0.1f;
            else if (Input.Keys.Pressed(Keys.Subtract))
                _light.Radius -= 0.1f;

            if (Input.Keys.Pressed(Keys.Divide))
                _light.Intensity += 0.1f;
            else if (Input.Keys.Pressed(Keys.Multiply))
                _light.Intensity -= 0.1f;

            if (Input.Keys.Pressed(Keys.P))
                _light.FallOf += 0.1f;
            else if (Input.Keys.Pressed(Keys.M))
                _light.FallOf -= 0.1f;

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
                translation.Y += Input.Mouse.Delta.Y * 0.1f;

            m_Transform.Translate(ref translation);
        }
    }
}
