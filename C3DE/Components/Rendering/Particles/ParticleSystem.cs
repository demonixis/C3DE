using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering.Particles
{
    public class ParticleSystem : Renderer
    {
        private ParticleSettings _settings;
        private ParticleVertex[] _particles;
        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private float _currentTime;
        private int _firstActiveParticle;
        private int _firstNewParticle;
        private int _firstFreeParticle;
        private int _firstRetiredParticle;
        private int _drawCounter;

        public void Setup(GraphicsDevice graphics, ParticleSettings settings)
        {
            _settings = settings;
            _particles = new ParticleVertex[settings.MaxParticles * 4];

            for (int i = 0; i < settings.MaxParticles; i++)
            {
                _particles[i * 4 + 0].Corner = new Vector2(-1, -1);
                _particles[i * 4 + 1].Corner = new Vector2(1, -1);
                _particles[i * 4 + 2].Corner = new Vector2(1, 1);
                _particles[i * 4 + 3].Corner = new Vector2(-1, 1);
            }

            _vertexBuffer = new DynamicVertexBuffer(graphics, ParticleVertex.VertexDeclaration, settings.MaxParticles * 4, BufferUsage.WriteOnly);

            var indices = new ushort[settings.MaxParticles * 6];

            for (int i = 0; i < settings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            _indexBuffer = new IndexBuffer(graphics, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            if (_vertexBuffer.IsContentLost)
                _vertexBuffer.SetData(_particles);

            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (_firstNewParticle != _firstFreeParticle)
                AddNewParticlesToVertexBuffer();

            // If there are any active particles, draw them now!
            if (_firstActiveParticle != _firstFreeParticle)
            {
                device.BlendState = _settings.BlendState;
                device.DepthStencilState = DepthStencilState.DepthRead;

                // Set the particle vertex and index buffer.
                device.SetVertexBuffer(_vertexBuffer);
                device.Indices = _indexBuffer;

                if (_firstActiveParticle < _firstFreeParticle)
                {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, _firstActiveParticle * 6, (_firstFreeParticle - _firstActiveParticle) * 2);
                }
                else
                {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, _firstActiveParticle * 6, (_settings.MaxParticles - _firstActiveParticle) * 2);

                    if (_firstFreeParticle > 0)
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _firstFreeParticle * 2);
                }

                device.DepthStencilState = DepthStencilState.Default;
            }

            _drawCounter++;
        }

        public override void Update()
        {
            _currentTime += Time.TotalTime;

            RetireActiveFreeRetiredParticles();

            if (_firstActiveParticle == _firstFreeParticle)
                _currentTime = 0;

            if (_firstRetiredParticle == _firstActiveParticle)
                _drawCounter = 0;
        }

        private void RetireActiveFreeRetiredParticles()
        {
            float particleDuration = (float)_settings.Duration.TotalSeconds;

            while (_firstActiveParticle != _firstNewParticle)
            {
                float particleAge = _currentTime - _particles[_firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                _particles[_firstActiveParticle * 4].Time = _drawCounter;

                // Move the particle from the active to the retired queue.
                _firstActiveParticle++;

                if (_firstActiveParticle >= _settings.MaxParticles)
                    _firstActiveParticle = 0;
            }

            while (_firstRetiredParticle != _firstActiveParticle)
            {
                int age = _drawCounter - (int)_particles[_firstRetiredParticle * 4].Time;
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                _firstRetiredParticle++;

                if (_firstRetiredParticle >= _settings.MaxParticles)
                    _firstRetiredParticle = 0;
            }
        }

        private void AddNewParticlesToVertexBuffer()
        {
            var stride = ParticleVertex.SizeInBytes;

            if (_firstNewParticle < _firstFreeParticle)
            {
                _vertexBuffer.SetData(_firstNewParticle * stride * 4, _particles, _firstNewParticle * 4, (_firstFreeParticle - _firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                _vertexBuffer.SetData(_firstNewParticle * stride * 4, _particles, _firstNewParticle * 4, (_settings.MaxParticles - _firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);

                if (_firstFreeParticle > 0)
                    _vertexBuffer.SetData(0, _particles, 0, _firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
            }

            _firstNewParticle = _firstFreeParticle;
        }

        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            int nextFreeParticle = _firstFreeParticle + 1;

            if (nextFreeParticle >= _settings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == _firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= _settings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(_settings.MinHorizontalVelocity, _settings.MaxHorizontalVelocity, RandomHelper.ValueF);

            double horizontalAngle = RandomHelper.ValueF * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(_settings.MinVerticalVelocity, _settings.MaxVerticalVelocity, RandomHelper.ValueF);

            var randomValues = RandomHelper.GetColor(-1);
            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                _particles[_firstFreeParticle * 4 + i].Position = position;
                _particles[_firstFreeParticle * 4 + i].Velocity = velocity;
                _particles[_firstFreeParticle * 4 + i].Random = randomValues;
                _particles[_firstFreeParticle * 4 + i].Time = _currentTime;
            }

            _firstFreeParticle = nextFreeParticle;
        }
    }
}
