using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts.Utils
{
    public sealed class SinIntensity : Behaviour
    {
        private Light _light;
        private float _Y = 0;

        public float Min { get; set; } = 0.5f;
        public float Max { get; set; } = 0.5f;
        public float Frequency { get; set; } = 0.25f;
        public float Phase { get; set; } = 0.0f;

        public override void Start()
        {
            base.Start();

            if (Phase == 0.0f)
                Phase = RandomHelper.Range(0.0f, 8000.0f);

            _light = GetComponent<Light>();
        }

        public override void Update()
        {
            if (_light == null)
                return;

            _Y = (Time.TotalTime + Phase) * Frequency;
            _Y = _Y - (float)Math.Floor((double)_Y); // normalized value to 0..1
            _light.Intensity = (float)((Max * Math.Sin(2 * MathHelper.Pi * _Y)) + Min);
        }
    }
}