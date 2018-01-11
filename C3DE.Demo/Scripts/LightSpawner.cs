using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public sealed class LightSpawner : Behaviour
    {
        public bool ShowDebugMesh { get; set; } = false;
        public bool ShadowMapEnabled { get; set; } = false;
        public float Intensity { get; set; } = 1.0f;
        public Color? Color { get; set; } = null;

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space))
            {
                var color = Color.HasValue ? Color.Value : RandomHelper.GetColor();
                var lightGo = GameObjectFactory.CreateLight(LightType.Point, color, Intensity, 1024);
                lightGo.Transform.LocalPosition = m_Transform.LocalPosition;
                Scene.current.Add(lightGo);

                var light = lightGo.GetComponent<Light>();
                light.Range = 5;
                light.ShadowGenerator.ShadowStrength = 1;
                light.ShadowGenerator.Enabled = ShadowMapEnabled;

                if (ShowDebugMesh)
                {
                    var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                    ligthSphere.Geometry = new SphereMesh(0.05f, 12);
                    ligthSphere.Geometry.Build();
                    ligthSphere.CastShadow = true;
                    ligthSphere.ReceiveShadow = false;

                    var sphereMaterial = new UnlitMaterial();
                    sphereMaterial.DiffuseColor = color;
                    ligthSphere.Material = sphereMaterial;
                }
            }
        }
    }
}
