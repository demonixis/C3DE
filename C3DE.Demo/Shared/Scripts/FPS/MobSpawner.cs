using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scenes;
using C3DE.Demo.Scripts.Utils;
using C3DE.Graphics.Materials;
using C3DE.Navigation;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace C3DE.Demo.Scripts.FPS
{
    public class MobSpawner : DemoBehaviour
    {
        public const int MaxSpawn = 10;
        private Vector2 _mapOrigin;
        private int _mapPadding;
        private List<Transform> _mobs;
        private Material _mobMaterial;
        private (int, int)[] _walkableIndex;
        private AStar _astar;

        public override void Start()
        {
            _mobs = new List<Transform>(5);
        }

        public void SetGrid(int[,] grid, int padding, Vector2 startPosition)
        {
            _mapOrigin = startPosition;
            _mapPadding = padding;

            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            var nodes = new Node[width, height];
            var walkable = new List<(int, int)>();

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var canWalk = grid[x, y] != 1;

                    nodes[x, y] = new Node(new Vector2(x, y), canWalk);

                    if (canWalk)
                        walkable.Add((x, y));
                }
            }

            _walkableIndex = walkable.ToArray();

            _astar = new AStar(nodes);

            var content = Application.Content;

            _mobMaterial = GetMobMaterial(content);

            for (var i = 0; i < MaxSpawn; i++)
            {
                var model = content.Load<Model>("Models/Drone/drone");
                var go = model.ToMeshRenderers();
                var renderers = go.GetComponentsInChildren<Renderer>();

                foreach (var renderer in renderers)
                    renderer.Material = _mobMaterial;

                GetRandomPoint(out float tileX, out float tileY, out float worldX, out float worldZ);

                go.Transform.Position = new Vector3(worldX, 1.5f, worldZ);
                go.Transform.LocalScale = new Vector3(1f);

                //var bob = go.AddComponent<Headbob>();
                //bob.BobSpeed = 0.005f;
                //bob.BobAmount = 0.1f;

                //var rb = go.AddComponent<Rigidbody>();
                //rb.IsKinematic = true;
                //rb.AddComponent<BoxCollider>();

                //if (FPSDemo.DebugPhysics)
                //rb.AddComponent<BoundingBoxRenderer>();

                var paths = GetPaths(tileX, tileY);
                var nav = go.AddComponent<SimplePath>();

                nav.Begin();
                foreach (var path in paths)
                    nav.AddPath(new Vector3((path.Position.X * padding) + startPosition.X, go.Transform.Position.Y, (path.Position.Y * padding) + startPosition.Y));
                nav.End();
            }
        }

        private Stack<Node> GetPaths(float tileX, float tileY)
        {
            Stack<Node> paths;
            (int, int) destination;

            do
            {
                destination = _walkableIndex[RandomHelper.Range(0, _walkableIndex.Length)];
                paths = _astar.FindPath(tileX, tileY, destination.Item1, destination.Item2);

            } while (paths == null || paths.Count < 2);

            return paths;
        }

        private void GetRandomPoint(out float tileX, out float tileY, out float worldX, out float worldZ)
        {
            var randomPoint = _walkableIndex[RandomHelper.Range(0, _walkableIndex.Length)];

            tileX = randomPoint.Item1;
            tileY = randomPoint.Item2;
            worldX = (randomPoint.Item1 * _mapPadding) + _mapOrigin.X;
            worldZ = (randomPoint.Item2 * _mapPadding) + _mapOrigin.Y;
        }

        private Material GetMobMaterial(ContentManager content)
        {
            if (FPSDemo.PreferePBR)
            {
                var pbr = new PBRMaterial()
                {
                    MainTexture = content.Load<Texture2D>("Models/Drone/drone_Albedo"),
                    NormalMap = content.Load<Texture2D>("Models/Drone/drone_Normal"),
                    EmissiveMap = content.Load<Texture2D>("Models/Drone/drone_Emission")
                };

                pbr.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Models/Drone/drone_Roughness"),
                    content.Load<Texture2D>("Models/Drone/drone_Metallic"),
                    content.Load<Texture2D>("Models/Drone/drone_Occlusion"));

                return pbr;
            }

            return new StandardMaterial
            {
                MainTexture = content.Load<Texture2D>("Models/Drone/drone_Albedo"),
                NormalMap = content.Load<Texture2D>("Models/Drone/drone_Normal"),
                EmissiveMap = content.Load<Texture2D>("Models/Drone/drone_Emission"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 2,
                SpecularIntensity = 2,
                EmissiveColor = Color.White,
                EmissiveIntensity = 2.5f
            };
        }
    }
}
