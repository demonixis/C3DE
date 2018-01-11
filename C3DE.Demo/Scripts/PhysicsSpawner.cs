using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class PhysicsSpawner : Behaviour
    {
        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space) || Input.Keys.Pressed(Keys.LeftControl))
                SpawnCubeAtPosition(Camera.Main.Transform.Position + new Vector3(0, 0, 5));
        }

        private void SpawnCubeAtPosition(Vector3 position)
        {
            var go = new GameObject("Cube");
            Scene.current.Add(go);
            go.Transform.LocalPosition = position;

            var cube = go.AddComponent<MeshRenderer>();
            cube.Geometry = new CubeMesh();
            cube.Geometry.Size = new Vector3(1.0f);
            cube.Geometry.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = false;

             var material = new StandardMaterial();
            material.DiffuseColor = RandomHelper.GetColor();
            material.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            material.NormalTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock_Normal");
            cube.Material = material;

            var collider = cube.AddComponent<BoxCollider>();
            var rb = cube.AddComponent<Rigidbody>();
            rb.AddComponent<RigidbodyRenderer>();
        }
    }
}
