using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Components.Rendering
{
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
        public static float GlowSize = 400;
        public static float QuerySize = 100;

        private GraphicsDevice m_GraphicsDevice;
        private SpriteBatch m_SpriteBatch;
        private Camera m_Camera;
        private Vector2 m_LightPosition;
        private bool m_LightBehindCamera;
        private BasicEffect m_Effect;
        private VertexPositionColor[] m_QueryVertices;
        private OcclusionQuery m_OcclusionQuery;
        private bool m_OcclusionQueryActive;
        private float m_OcclusionAlpha;

        private readonly Flare[] m_Flares =
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

        public Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));
        public Texture2D GlowTexture { get; set; }
        public Texture2D[] FlareTextures { get; set; }

        public override void Start()
        {
            base.Start();

            m_GraphicsDevice = Application.GraphicsDevice;

            ColorWriteDisable = new BlendState()
            {
                ColorWriteChannels = ColorWriteChannels.None
            };

            m_Effect = new BasicEffect(m_GraphicsDevice);
            m_Effect.View = Matrix.Identity;
            m_Effect.VertexColorEnabled = true;

            m_QueryVertices = new VertexPositionColor[4];
            m_QueryVertices[0].Position = new Vector3(-QuerySize / 2, -QuerySize / 2, -1);
            m_QueryVertices[1].Position = new Vector3(QuerySize / 2, -QuerySize / 2, -1);
            m_QueryVertices[2].Position = new Vector3(-QuerySize / 2, QuerySize / 2, -1);
            m_QueryVertices[3].Position = new Vector3(QuerySize / 2, QuerySize / 2, -1);

            m_OcclusionQuery = new OcclusionQuery(m_GraphicsDevice);

            m_SpriteBatch = new SpriteBatch(m_GraphicsDevice);
            m_Camera = GetComponent<Camera>();

            if (m_Camera == null)
                throw new Exception("A LensFlare component have to be attached on a camera.");
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
            var infiniteView = m_Camera.view;
            infiniteView.Translation = Vector3.Zero;

            // Project the light position into 2D screen space.
            var viewport = m_GraphicsDevice.Viewport;
            var projectedPosition = viewport.Project(-LightDirection, m_Camera.projection, infiniteView, Matrix.Identity);

            // Don't draw any flares if the light is behind the camera.
            if ((projectedPosition.Z < 0) || (projectedPosition.Z > Math.PI))
            {
                m_LightBehindCamera = true;
                return;
            }

            m_LightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
            m_LightBehindCamera = false;

            if (m_OcclusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!m_OcclusionQuery.IsComplete)
                    return;

                var queryArea = QuerySize * QuerySize;
                m_OcclusionAlpha = Math.Min(m_OcclusionQuery.PixelCount / queryArea, 1);
            }

            m_GraphicsDevice.BlendState = ColorWriteDisable;
            m_GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Set up our BasicEffect to center on the current 2D light position.
            m_Effect.World = Matrix.CreateTranslation(m_LightPosition.X, m_LightPosition.Y, 0);
            m_Effect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            m_Effect.CurrentTechnique.Passes[0].Apply();

            // Issue the occlusion query.
            m_OcclusionQuery.Begin();
            m_GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_QueryVertices, 0, 2);
            m_OcclusionQuery.End();
            m_OcclusionQueryActive = true;
        }

        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        private void DrawGlow()
        {
            if (m_LightBehindCamera || m_OcclusionAlpha <= 0)
                return;

            var color = Color.White * m_OcclusionAlpha;
            var origin = new Vector2(GlowTexture.Width, GlowTexture.Height) / 2;
            var scale = GlowSize * 2 / GlowTexture.Width;

            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(GlowTexture, m_LightPosition, null, color, 0, origin, scale, SpriteEffects.None, 0);
            m_SpriteBatch.End();
        }


        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        private void DrawFlares()
        {
            if (m_LightBehindCamera || m_OcclusionAlpha <= 0)
                return;

            var viewport = m_GraphicsDevice.Viewport;
            var screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;
            var flareVector = screenCenter - m_LightPosition;

            // Draw the flare sprites using additive blending.
            m_SpriteBatch.Begin(0, BlendState.Additive);

            foreach (Flare flare in m_Flares)
            {
                var flarePosition = m_LightPosition + flareVector * flare.Position;
                var flareColor = flare.Color.ToVector4();
                flareColor.W *= m_OcclusionAlpha;

                var flareTexture = FlareTextures[flare.FlareId];
                var flareOrigin = new Vector2(flareTexture.Width, flareTexture.Height) / 2;

                m_SpriteBatch.Draw(flareTexture, flarePosition, null, new Color(flareColor), 1, flareOrigin, flare.Scale, SpriteEffects.None, 0);
            }

            m_SpriteBatch.End();
        }

        private void RestoreRenderStates()
        {
            m_GraphicsDevice.BlendState = BlendState.Opaque;
            m_GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            m_GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
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
