using C3DE.Components;
using C3DE.Materials;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class HexaGridBuilder : Behaviour
    {
        private ModelPrefab _gridPrefab;
        private float _hexWidth;
        private float _hexDepth;

        public int GridWidth { get; set; }
        public int GridDepth { get; set; }
        public float Margin { get; set; }

        public HexaGridBuilder()
            : base()
        {
            GridWidth = 25;
            GridDepth = 25;
            Margin = 1.1f;
        }

        public override void Start()
        {
            _gridPrefab = new ModelPrefab("Hexa Tile");
            _gridPrefab.Transform.LocalScale = new Vector3(5);
            _gridPrefab.LoadModel("Models/HexGrid");
            _gridPrefab.Renderer.MainMaterial = new SimpleMaterial(sceneObject.Scene);
            _gridPrefab.Renderer.MainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/hexa_tex");
            _gridPrefab.Enabled = false;
            sceneObject.Scene.Add(_gridPrefab);

            _hexWidth = _gridPrefab.Renderer.BoundingSphere.Radius * 2 * 0.85f * Margin;
            _hexDepth = _gridPrefab.Renderer.BoundingSphere.Radius * 2 * Margin;

            GenerateHexaGrid();
        }

        private Vector3 CalculateInitialPosition()
        {
            Vector3 position = new Vector3();
            position.X = -_hexWidth * GridWidth / 2.0f + _hexWidth / 2.0f;
            position.Y = 0;
            position.Z = GridDepth / 2.0f * _hexDepth - _hexDepth / 2.0f;
            return position;
        }

        public Vector3 GetWorldCoordinate(float x, float z)
        {
            Vector3 position = CalculateInitialPosition();

            float offset = 0;

            if (z % 2 != 0)
                offset = _hexWidth / 2.0f;

            var px = position.X + offset + x * _hexWidth;
            var pz = position.Z - z * _hexDepth * 0.75f;

            return new Vector3(px, 0, pz);
        }

        private void GenerateHexaGrid()
        {
            SceneObject cache = null;

            for (int z = 0; z < GridDepth; z++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    cache = Scene.Instanciate(_gridPrefab, GetWorldCoordinate(x, z), Vector3.Zero);
                    cache.Transform.Parent = transform;
                }
            }
        }
    }
}
