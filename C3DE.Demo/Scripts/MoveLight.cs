using C3DE.Components;
using C3DE.Components.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class MoveLight : Behaviour
    {
        private Vector3 translation;
        private Light _light;

        public override void Start()
        {
            _light = GetComponent<Light>();
        }

        public override void Update()
        {
            translation = Vector3.Zero;

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
                translation.Y += Input.Mouse.Delta.Y * 0.1f;
            else
                translation.Z += Input.Mouse.Delta.Y * 0.1f;

            translation.X += Input.Mouse.Delta.X * 0.1f;

            if (Input.Keys.Pressed(Keys.Add))
                _light.Range += 0.1f;
            else if (Input.Keys.Pressed(Keys.Subtract))
                _light.Range -= 0.1f;

            if (Input.Keys.Pressed(Keys.Divide))
                _light.Intensity += 0.1f;
            else if (Input.Keys.Pressed(Keys.Multiply))
                _light.Intensity -= 0.1f;

            if (Input.Keys.Pressed(Keys.P))
                _light.FallOf += 0.1f;
            else if (Input.Keys.Pressed(Keys.M))
                _light.FallOf -= 0.1f;
            
            transform.Translate(ref translation);
        }
    }
}
