using C3DE.Graphics.Rendering;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering
{
    public class ParticleSystem : Behaviour
    {
        private ParticleSettings _settings;
        private ParticleVertex[] particles;
        private Effect _particleEffect;
        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer indexBuffer;
        private int _firstActiveParticle;
        private int _firstNewParticle;
        private int _firstFreeParticle;
        private int _firstRetiredParticle;
        private float _currentTime;
        private int _drawCounter;
        private bool _ready;

        public void Setup(ref ParticleSettings settings)
        {
            var graphics = Application.GraphicsDevice;

            particles = new ParticleVertex[settings.MaxParticles * 4];

            for (var i = 0; i < settings.MaxParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Vector2(-1, -1);
                particles[i * 4 + 1].Corner = new Vector2(1, -1);
                particles[i * 4 + 2].Corner = new Vector2(1, 1);
                particles[i * 4 + 3].Corner = new Vector2(-1, 1);
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

            indexBuffer = new IndexBuffer(graphics, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            var effect = Application.Content.Load<Effect>("ParticleEffect");

            _particleEffect = effect.Clone();

            _particleEffect.Parameters["Duration"].SetValue((float)settings.Duration.TotalSeconds);
            _particleEffect.Parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
            _particleEffect.Parameters["Gravity"].SetValue(settings.Gravity);
            _particleEffect.Parameters["EndVelocity"].SetValue(settings.EndVelocity);
            _particleEffect.Parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
            _particleEffect.Parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());
            _particleEffect.Parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));
            _particleEffect.Parameters["StartSize"].SetValue(new Vector2(settings.MinStartSize, settings.MaxStartSize));
            _particleEffect.Parameters["EndSize"].SetValue(new Vector2(settings.MinEndSize, settings.MaxEndSize));

            var texture = Application.Content.Load<Texture2D>(settings.TextureName);

            _particleEffect.Parameters["Texture"].SetValue(texture);
            _ready = true;
        }

        public override void Update()
        {
            if (!_ready)
                return;

            _currentTime += Time.TotalTime;

            RetireActiveParticles();
            FreeRetiredParticles();

            if (_firstActiveParticle == _firstFreeParticle)
                _currentTime = 0;

            if (_firstRetiredParticle == _firstActiveParticle)
                _drawCounter = 0;
        }

        public void Draw(GraphicsDevice graphics)
        {
            if (!_ready)
                return;

            if (_vertexBuffer.IsContentLost)
                _vertexBuffer.SetData(particles);

            if (_firstNewParticle != _firstFreeParticle)
                AddNewParticlesToVertexBuffer();

            // If there are any active particles, draw them now!
            if (_firstActiveParticle != _firstFreeParticle)
            {
                graphics.BlendState = _settings.BlendState;
                graphics.DepthStencilState = DepthStencilState.DepthRead;

                _particleEffect.Parameters["ViewportScale"].SetValue(new Vector2(0.5f / graphics.Viewport.AspectRatio, -0.5f));
                _particleEffect.Parameters["CurrentTime"].SetValue(_currentTime);

                graphics.SetVertexBuffer(_vertexBuffer);
                graphics.Indices = indexBuffer;

                foreach (EffectPass pass in _particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (_firstActiveParticle < _firstFreeParticle)
                    {
                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, _firstActiveParticle * 6, (_firstFreeParticle - _firstActiveParticle) * 2);
                    }
                    else
                    {
                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, _firstActiveParticle * 6, (_settings.MaxParticles - _firstActiveParticle) * 2);

                        if (_firstFreeParticle > 0)
                            graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _firstFreeParticle * 2);
                    }
                }

                graphics.DepthStencilState = DepthStencilState.Default;
            }

            _drawCounter++;
        }

        private void RetireActiveParticles()
        {
            float particleDuration = (float)_settings.Duration.TotalSeconds;

            while (_firstActiveParticle != _firstNewParticle)
            {
                float particleAge = _currentTime - particles[_firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[_firstActiveParticle * 4].Time = _drawCounter;

                // Move the particle from the active to the retired queue.
                _firstActiveParticle++;

                if (_firstActiveParticle >= _settings.MaxParticles)
                    _firstActiveParticle = 0;
            }
        }

        private void FreeRetiredParticles()
        {
            while (_firstRetiredParticle != _firstActiveParticle)
            {
                var age = _drawCounter - (int)particles[_firstRetiredParticle * 4].Time;
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
            int stride = ParticleVertex.SizeInBytes;

            if (_firstNewParticle < _firstFreeParticle)
            {
                _vertexBuffer.SetData(_firstNewParticle * stride * 4, particles, _firstNewParticle * 4, (_firstFreeParticle - _firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                _vertexBuffer.SetData(_firstNewParticle * stride * 4, particles, _firstNewParticle * 4, (_settings.MaxParticles - _firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);

                if (_firstFreeParticle > 0)
                {
                    _vertexBuffer.SetData(0, particles, 0, _firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            _firstNewParticle = _firstFreeParticle;
        }

        public void SetCamera(Matrix view, Matrix projection)
        {
            _particleEffect.Parameters["View"].SetValue(view);
            _particleEffect.Parameters["Projection"].SetValue(projection);
        }

        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
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

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            var randomValues = RandomHelper.GetColor(RandomHelper.Range(0.0f, 1.0f));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[_firstFreeParticle * 4 + i].Position = position;
                particles[_firstFreeParticle * 4 + i].Velocity = velocity;
                particles[_firstFreeParticle * 4 + i].Random = randomValues;
                particles[_firstFreeParticle * 4 + i].Time = _currentTime;
            }

            _firstFreeParticle = nextFreeParticle;
        }
    }
}
