using C3DE.Components.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering
{
#if ANDROID
    class OcclusionQuery
    {
        public int PixelCount => 0;
        public bool IsComplete => false;

        public OcclusionQuery(GraphicsDevice device) { }

        public void Begin() { }
        public void End() { }
    }
#endif

    public class LensFlare : Renderer
    {
        public class Flare
        {
            public int FlareId;
            public float Position;
            public float Scale;
            public Color Color;

            public Flare(float position, float scale, Color color, int flareId)
            {
                Position = position;
                Scale = scale;
                Color = color;
                FlareId = flareId;
            }
        }

        private BlendState ColorWriteDisable;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private Camera _camera;
        private Vector2 _lightPosition;
        private bool _lightBehindCamera;
        private BasicEffect _effect;
        private VertexPositionColor[] _queryVertices;
        private OcclusionQuery _occlusionQuery;
        private bool _occlusionQueryActive;
        private float _occlusionAlpha;
        private Light _light;
        private Vector3 _direction = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));

        private readonly Flare[] _flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), 0),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), 0),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), 0),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), 0),
            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), 1),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), 1),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), 1),
            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), 2),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), 2),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), 2),
        };

        public Vector3 LightDirection
        {
            get
            {
                if (_light == null)
                    _light = GetComponent<Light>();

                return _light?.Direction ?? _direction;
            }
        }

        public Texture2D GlowTexture { get; set; }
        public Texture2D[] FlareTextures { get; set; }
        public float GlowSize { get; set; } = 400;
        public float QuerySize { get; set; } = 100;

        public override void Start()
        {
            base.Start();

            _graphicsDevice = Application.GraphicsDevice;

            ColorWriteDisable = new BlendState()
            {
                ColorWriteChannels = ColorWriteChannels.None
            };

            _effect = new BasicEffect(_graphicsDevice);
            _effect.View = Matrix.Identity;
            _effect.VertexColorEnabled = true;

            _queryVertices = new VertexPositionColor[4];
            _queryVertices[0].Position = new Vector3(-QuerySize / 2, -QuerySize / 2, -1);
            _queryVertices[1].Position = new Vector3(QuerySize / 2, -QuerySize / 2, -1);
            _queryVertices[2].Position = new Vector3(-QuerySize / 2, QuerySize / 2, -1);
            _queryVertices[3].Position = new Vector3(QuerySize / 2, QuerySize / 2, -1);

            _occlusionQuery = new OcclusionQuery(_graphicsDevice);

            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _light = GetComponent<Light>();
        }

        public void Setup(Texture2D glow, Texture2D[] flares)
        {
            if (flares.Length != 3)
                throw new Exception("The array of flare must contains 3 textures");

            GlowTexture = glow;
            FlareTextures = flares;
        }

        public override void Draw(GraphicsDevice graphics)
        {
            UpdateOcclusion();
            DrawGlow();
            DrawFlares();
            RestoreRenderStates();
        }

        private void UpdateOcclusion()
        {
            var camera = Camera.Main;
            if (camera == null)
                return;

            var infiniteView = camera._viewMatrix;
            infiniteView.Translation = Vector3.Zero;

            // Project the light position into 2D screen space.
            var viewport = _graphicsDevice.Viewport;
            var projectedPosition = viewport.Project(-LightDirection, camera._projectionMatrix, infiniteView, Matrix.Identity);

            // Don't draw any flares if the light is behind the camera.
            if ((projectedPosition.Z < 0) || (projectedPosition.Z > Math.PI))
            {
                _lightBehindCamera = true;
                return;
            }

            _lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
            _lightBehindCamera = false;

            if (_occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!_occlusionQuery.IsComplete)
                    return;

                var queryArea = QuerySize * QuerySize;
                _occlusionAlpha = Math.Min(_occlusionQuery.PixelCount / queryArea, 1);
            }

            _graphicsDevice.BlendState = ColorWriteDisable;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Set up our BasicEffect to center on the current 2D light position.
            _effect.World = Matrix.CreateTranslation(_lightPosition.X, _lightPosition.Y, 0);
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            _effect.CurrentTechnique.Passes[0].Apply();

            // Issue the occlusion query.
            _occlusionQuery.Begin();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _queryVertices, 0, 2);
            _occlusionQuery.End();
            _occlusionQueryActive = true;
        }

        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        private void DrawGlow()
        {
            if (_lightBehindCamera || _occlusionAlpha <= 0)
                return;

            var color = Color.White * _occlusionAlpha;
            var origin = new Vector2(GlowTexture.Width, GlowTexture.Height) / 2;
            var scale = GlowSize * 2 / GlowTexture.Width;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            _spriteBatch.Draw(GlowTexture, _lightPosition, null, color, 0, origin, scale, SpriteEffects.None, 0);
            _spriteBatch.End();
        }

        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        private void DrawFlares()
        {
            if (_lightBehindCamera || _occlusionAlpha <= 0)
                return;

            var viewport = _graphicsDevice.Viewport;
            var screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;
            var flareVector = screenCenter - _lightPosition;

            // Draw the flare sprites using additive blending.
            _spriteBatch.Begin(0, BlendState.Additive);

            foreach (Flare flare in _flares)
            {
                var flarePosition = _lightPosition + flareVector * flare.Position;
                var flareColor = flare.Color.ToVector4();
                flareColor.W *= _occlusionAlpha;

                var flareTexture = FlareTextures[flare.FlareId];
                var flareOrigin = new Vector2(flareTexture.Width, flareTexture.Height) / 2;

                _spriteBatch.Draw(flareTexture, flarePosition, null, new Color(flareColor), 1, flareOrigin, flare.Scale, SpriteEffects.None, 0);
            }

            _spriteBatch.End();
        }

        private void RestoreRenderStates()
        {
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override int CompareTo(object obj)
        {
            var renderer = obj as Renderer;
            if (renderer == null)
                return 0;

            if (renderer is LensFlare)
                return 1;
            else
                return -1;
        }
    }
}
