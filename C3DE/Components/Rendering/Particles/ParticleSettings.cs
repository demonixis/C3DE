using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering.Particles
{
    public class ParticleSettings
    {
        public string TextureName = null;
        public int MaxParticles = 100;
        public TimeSpan Duration = TimeSpan.FromSeconds(1);
        public float DurationRandomness = 0;
        public float EmitterVelocitySensitivity = 1;
        public float MinHorizontalVelocity = 0;
        public float MaxHorizontalVelocity = 0;
        public float MinVerticalVelocity = 0;
        public float MaxVerticalVelocity = 0;
        public Vector3 Gravity = Vector3.Zero;
        public float EndVelocity = 1;
        public Color MinColor = Color.White;
        public Color MaxColor = Color.White;
        public float MinRotateSpeed = 0;
        public float MaxRotateSpeed = 0;
        public float MinStartSize = 100;
        public float MaxStartSize = 100;
        public float MinEndSize = 100;
        public float MaxEndSize = 100;
        public BlendState BlendState = BlendState.NonPremultiplied;
    }
}
