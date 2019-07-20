using C3DE.Components;
using C3DE.Components.Physics;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
    public class Headbob : Behaviour
    {
        private float _elapsedTime = 0.0f;
        private Vector3 _lastPosition;
        private float _originalYPos;

        public float BobSpeed { get; set; } = 0.18f;
        public float BobAmount { get; set; } = 0.2f;

        public override void Start()
        {
            base.Start();
            _lastPosition = _transform.LocalPosition;
            _originalYPos = _transform.LocalPosition.Y;
        }

        public override void Update()
        {
            var velicity = (_transform.LocalPosition - _lastPosition) / Time.DeltaTime;
            velicity.Y = 0;
      
            _lastPosition = _transform.LocalPosition;

            var waveSlice = 0.0f;
            var velocity = velicity;
            var horizontal = velocity.Z;
            var vertical = velocity.X;

            if (Math.Abs(horizontal) == 0 && Math.Abs(vertical) == 0)
            {
                _elapsedTime = 0.0f;
            }
            else
            {
                waveSlice = (float)Math.Sin(_elapsedTime);
                _elapsedTime = _elapsedTime + BobSpeed;
                if (_elapsedTime > MathHelper.Pi * 2.0f)
                {
                    _elapsedTime = _elapsedTime - (MathHelper.Pi * 2.0f);
                }
            }

            var position = _transform.LocalPosition;

            if (waveSlice != 0)
            {
                var translation = waveSlice * BobAmount;
                var totalAxes = Math.Abs(horizontal) + Math.Abs(vertical);
                totalAxes = MathHelper.Clamp(totalAxes, 0.0f, 1.0f);
                translation = totalAxes * translation;
                position.Y = _originalYPos + translation;
            }
            else
                position.Y = _originalYPos;

            _transform.LocalPosition = position;
        }
    }
}
