using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class PhysicsSpawner : Behaviour
    {
        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space))
            {
                var go = new GameObject("Cube");
                Scene.current.Add(go);
                go.Transform.Position = transform.Position;
                go.Transform.Translate(0, 5, -5);

                var cube = go.AddComponent<MeshRenderer>();
                cube.Geometry = new CubeMesh();
                cube.Geometry.Size = new Microsoft.Xna.Framework.Vector3(0.25f);
                cube.Geometry.Build();
                cube.CastShadow = true;
                cube.ReceiveShadow = false;

                var material = new UnlitMaterial(Scene.current);
                material.DiffuseColor = RandomHelper.GetColor();
                cube.Material = material;

                var collider = cube.AddComponent<BoxCollider>();
                var rb = cube.AddComponent<Rigidbody>();
            }
        }
    }
}
