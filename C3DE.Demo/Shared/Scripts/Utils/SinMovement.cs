using C3DE.Components;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts.Utils
{
    public sealed class SinMovement : Behaviour
    {
        private Vector3 _position = Vector3.Zero;
        private float _Y = 0;

        public float Min { get; set; } = 15.5f;
        public float Max { get; set; } = 15.5f;
        public float Frequency { get; set; } = 0.25f;
        public float Phase { get; set; } = 0.0f;


        public override void Awake()
        {
            base.Awake();

            if (Phase == 0.0f)
                Phase = RandomHelper.Range(0.0f, 8000.0f);
        }

        public override void Update()
        {
            _position = _transform.LocalPosition;
            _Y = (Time.TotalTime + Phase) * Frequency;
            _Y = _Y - (float)Math.Floor((double)_Y); // normalized value to 0..1
            _position.Y = (float)((Max * Math.Sin(2 * MathHelper.Pi * _Y)) + Min);

            _transform.LocalPosition = _position;
        }
    }
}