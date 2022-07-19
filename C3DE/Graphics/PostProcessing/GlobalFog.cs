using System;
using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class GlobalFog : PostProcessPass
    {
        private QuadRenderer _quadRenderer;

        public FogMode Mode { get; set; } = FogMode.Linear;
        public float Density { get; set; } = 0.0085f;
        public float Start { get; set; } = 10;
        public float End { get; set; } = 500;

        public bool DistanceFog { get; set; } = true;
        public bool ExcludeFarPixels { get; set; } = true;
        public bool UseRadialDistance { get; set; } = false;
        public bool HeightFog { get; set; } = true;
        public float Height { get; set; } = 10;
        public float HeightDensity { get; set; } = 0.5f;

        public bool ExcludeSkybox
        {
            get
            {
                var renderer = Application.Engine.Renderer;
                var forward = renderer as ForwardRenderer;
                //if (forward != null)
                //return forward.DepthRenderer.ExcludeSkybox;

                return true;
            }
            set
            {
                var renderer = Application.Engine.Renderer;
                var forward = renderer as ForwardRenderer;
                //if (forward != null)
                //forward.DepthRenderer.ExcludeSkybox = value;
            }
        }

        public GlobalFog(GraphicsDevice graphics, bool excludeSkybox = false)
            : base(graphics)
        {
            ExcludeSkybox = excludeSkybox;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/GlobalFog");
            _quadRenderer = new QuadRenderer(_graphics);
        }

        public override void SetupEffect()
        {
            base.SetupEffect();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            if (Mode == FogMode.None)
                return;

            var camera = Camera.Main;
            var cameraTransform = camera._transform;
            var frustumCorners = camera.CalculateFrustumCorners(new Rectangle(0, 0, 1, 1), camera.Far, 0);
            var bottomLeft = cameraTransform.TransformVector(frustumCorners[4]);
            var topLeft = cameraTransform.TransformVector(frustumCorners[5]);
            var topRight = cameraTransform.TransformVector(frustumCorners[6]);
            var bottomRight = cameraTransform.TransformVector(frustumCorners[7]);
            var frustumCornersArray = MatrixExtensions.CreateFromVector3(bottomLeft, bottomRight, topLeft, topRight);
            var camPos = cameraTransform.Position;
            var FdotC = camPos.Y - Height;
            var paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
            var excludeDepth = (ExcludeFarPixels ? 1.0f : 2.0f);
            var sceneMode = Mode;
            var sceneDensity = Density;
            var heightParams = new Vector4(Height, FdotC, paramK, HeightDensity * 0.5f);
            var distanceParams = new Vector4(-Math.Max(Start, 0.0f), excludeDepth, 0, 0);
            var linear = sceneMode == FogMode.Linear;
            var diff = linear ? End - Start : 0.0f;
            var invDiff = Math.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
            var sceneParams = new Vector4(sceneDensity * 1.2011224087f, sceneDensity * 1.4426950408f, linear ? -invDiff : 0.0f, linear ? End * invDiff : 0.0f);
            var fogMode = new Vector4((int)sceneMode, UseRadialDistance ? 1 : 0, 0, 0);
            var projectionParams = new Vector4(1.0f, Camera.Main.Near, Camera.Main.Far, 1.0f / Camera.Main.Far);

            _graphics.SetRenderTarget(_mainRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            _effect.Parameters["FrustumCornersWS"].SetValue(frustumCornersArray);
            _effect.Parameters["CameraWS"].SetValue(camPos);
            _effect.Parameters["HeightParams"].SetValue(heightParams);
            _effect.Parameters["DistanceParams"].SetValue(distanceParams);
            _effect.Parameters["FogParams"].SetValue(sceneParams);
            _effect.Parameters["FogMode"].SetValue(fogMode);
            _effect.Parameters["FogColor"].SetValue(Scene.current.RenderSettings.FogColor.ToVector4());
            _effect.Parameters["TextureSamplerTexelSize"].SetValue(_textureSamplerTexelSize);
            _effect.Parameters["ProjectionParams"].SetValue(projectionParams);
            _effect.Parameters["TargetTexture"].SetValue(renderTarget);
            _effect.Parameters["DepthTexture"].SetValue(GetDepthBuffer());

            var passIndex = 2;
            if (DistanceFog && HeightFog)
                passIndex = 0;
            else if (DistanceFog)
                passIndex = 1;

            _effect.CurrentTechnique.Passes[passIndex].Apply();
            _quadRenderer.RenderFullscreenQuad();

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _mainRenderTarget;
            _graphics.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, _mainRenderTarget, _mainRenderTarget.Width, _mainRenderTarget.Height, null);
        }
    }
}
