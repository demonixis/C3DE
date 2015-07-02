using C3DE.Components;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    public class PreLightRenderer : Renderer
    {
        private Vector2 _viewport;
        internal RenderTarget2D _depthRT;
        internal RenderTarget2D _normalRT;
        internal RenderTarget2D _lightRT;
        private MeshRenderer _lightMesh;
        private Effect _depthNormalFX;
        private Effect _lightingFX;

        public PreLightRenderer()
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            _viewport.X = graphicsDevice.Viewport.Width;
            _viewport.Y = graphicsDevice.Viewport.Height;
            CreateRenderTargets(graphicsDevice);

            _depthNormalFX = content.Load<Effect>("FX/PreLighting/PL_DepthNormal");
            _lightingFX = content.Load<Effect>("FX/PreLighting/PL_LightMap");

            var so = new SceneObject("LightMesh");
            _lightMesh = so.AddComponent<MeshRenderer>();
            _lightMesh.Geometry = new SphereGeometry();
            _lightMesh.Geometry.Generate();
        }

        public void CreateRenderTargets(GraphicsDevice device)
        {
            _depthRT = new RenderTarget2D(device, (int)_viewport.X, (int)_viewport.Y, false, SurfaceFormat.Single, DepthFormat.Depth24);
            _normalRT = new RenderTarget2D(device, (int)_viewport.X, (int)_viewport.Y, false, SurfaceFormat.Color, DepthFormat.Depth24);
            _lightRT = new RenderTarget2D(device, (int)_viewport.X, (int)_viewport.Y, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void PreLightingPass(GraphicsDevice device, Scene scene, Camera camera)
        {
            _viewport.X = device.Viewport.Width;
            _viewport.Y = device.Viewport.Height;

            DrawDepthNormalMap(device, scene, camera);
            DrawLightMap(device, scene, camera);
            PrepareEffects(scene.materials);
        }

        private void DrawDepthNormalMap(GraphicsDevice device, Scene scene, Camera camera)
        {
            _depthNormalFX.Parameters["View"].SetValue(camera.view);
            _depthNormalFX.Parameters["Projection"].SetValue(camera.projection);

            device.SetRenderTargets(_normalRT, _depthRT);
            device.Clear(Color.White);

            for (int i = 0, l = scene.renderList.Count; i < l; i++)
            {
                _depthNormalFX.Parameters["World"].SetValue(scene.renderList[i].Transform.world);
                _depthNormalFX.CurrentTechnique.Passes[0].Apply();
                scene.renderList[i].Draw(device);
            }

            device.SetRenderTarget(null);
        }

        private void DrawLightMap(GraphicsDevice device, Scene scene, Camera camera)
        {
            int i = 0;
            int l = 0;
            float distance = 0;
            Matrix viewProjection = camera.view * camera.projection;
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            Matrix worldViewProjection = Matrix.Identity;
            Light light = null;

            _lightingFX.Parameters["NormalTexture"].SetValue(_normalRT);
            _lightingFX.Parameters["DepthTexture"].SetValue(_depthRT);
            _lightingFX.Parameters["InvViewProjection"].SetValue(invViewProjection);
            _lightingFX.Parameters["Viewport"].SetValue(_viewport);

            device.SetRenderTarget(_lightRT);
            device.Clear(Color.Black);
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;

            for (i = 0, l = scene.lights.Count; i < l; i++)
            {
                light = scene.lights[i];

                if (light.Backing != LightRenderMode.RealTime)
                {
                    worldViewProjection = (Matrix.CreateScale(light.Range) * light.Transform.world) * viewProjection;
                    _lightingFX.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
                    _lightingFX.Parameters["LightColor"].SetValue(light.diffuseColor);
                    _lightingFX.Parameters["LightAttenuation"].SetValue(light.FallOf);
                    _lightingFX.Parameters["LightPosition"].SetValue(light.Transform.Position);
                    _lightingFX.Parameters["LightRange"].SetValue(light.Range);
                    _lightingFX.Parameters["LightIntensity"].SetValue(light.Intensity);

                    distance = Vector3.Distance(camera.Transform.Position, light.Transform.Position);

                    if (distance < light.Range)
                        device.RasterizerState = RasterizerState.CullClockwise;

                    _lightingFX.CurrentTechnique.Passes[0].Apply();
                    _lightMesh.Draw(device);

                    device.RasterizerState = RasterizerState.CullCounterClockwise;
                }
            }

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SetRenderTarget(null);
        }

        private void PrepareEffects(List<Material> materials)
        {
            Effect effect = null;

            for (int i = 0, l = materials.Count; i < l; i++)
            {
                effect = materials[i].effect;

                if (effect != null && effect.Parameters["LightMap"] != null)
                {
                    effect.Parameters["LightMap"].SetValue(_lightRT);
                    effect.Parameters["Viewport"].SetValue(_viewport);
                }
            }
        }

        public override void Render(Scene scene)
        {
            RebuildRenderTargets();
            PreLightingPass(graphicsDevice, scene, scene.cameras[0]);
            RenderSceneForCamera(scene, scene.cameras[0]);
        }
    }
}

