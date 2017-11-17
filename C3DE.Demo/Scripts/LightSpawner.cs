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
    public class LightSpawner : Behaviour
    {
        private Color _color;
        private float _range;
        private bool _spawnSphere;

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space))
            {
                var color = RandomHelper.GetColor();
                var lightGo = GameObjectFactory.CreateLight(LightType.Point, color, 0.5f, 1024);
                lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
                lightGo.Transform.LocalPosition = transform.LocalPosition;
                Scene.current.Add(lightGo);

                var light = lightGo.GetComponent<Light>();
                light.Range = 10;
                light.ShadowGenerator.ShadowStrength = 1;

                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Geometry = new SphereMesh(0.5f, 16);
                ligthSphere.Geometry.Build();
                ligthSphere.CastShadow = false;
                ligthSphere.ReceiveShadow = false;

                var sphereMaterial = new UnlitMaterial(Scene.current);
                sphereMaterial.DiffuseColor = color;
                ligthSphere.Material = sphereMaterial;

                ligthSphere.AddComponent<LightMover>();

                Debug.Log(transform.LocalPosition);
            }
        }
    }
}
