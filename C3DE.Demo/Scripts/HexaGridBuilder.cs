using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
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
        public float TileScale { get; set; }
        public float Margin { get; set; }

        public HexaGridBuilder()
            : base()
        {
            GridWidth = 10;
            GridDepth = 10;
            Margin = 0.86f;
            TileScale = 0.5f;
        }

        public override void Start()
        {
            _gridPrefab = new ModelPrefab("Hexa Tile");
            _gridPrefab.Transform.LocalScale = new Vector3(TileScale, 0.5f * TileScale, TileScale);
            _gridPrefab.Transform.Rotation = new Vector3(0, MathHelper.Pi / 6, 0);
            _gridPrefab.LoadModel("Models/hexagone");
            _gridPrefab.Renderer.Material = new SimpleMaterial(sceneObject.Scene);
            _gridPrefab.Renderer.Material.Texture = Application.Content.Load<Texture2D>("Models/hexagone_basic");
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
            GameObject cache = null;

            var waterMaterial = _gridPrefab.Renderer.Material;

            var groundMaterial = new StandardMaterial(Scene.current);
            groundMaterial.Texture = Application.Content.Load<Texture2D>("Models/hexagone_green");

            var montainMaterial = new StandardMaterial(Scene.current);
            montainMaterial.Texture = Application.Content.Load<Texture2D>("Models/hexagone_brown");
            montainMaterial.DiffuseColor = Color.Red;

            int rand = 0;
            ModelRenderer mRenderer = null;

            for (int z = 0; z < GridDepth; z++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    rand = RandomHelper.Range(0, 10);

                    cache = Scene.Instanciate(_gridPrefab);
                    cache.Transform.Position = GetWorldCoordinate(x, z);
                    cache.Transform.Parent = transform;

                    mRenderer = cache.GetComponent<ModelRenderer>();

                    if (rand % 2 == 0)
                    {
                        mRenderer.Material = groundMaterial;
                        cache.Transform.LocalScale  += new Vector3(0, 0.5f, 0);
                    }
                    else if (rand % 5 == 0)
                    {
                        mRenderer.Material = montainMaterial;
                        cache.Transform.LocalScale += new Vector3(0.0f, 1.5f, 0.0f);
                    }

                    cache.Transform.SetPosition(null, _gridPrefab.Renderer.BoundingSphere.Radius * cache.Transform.LocalScale.Y / 2, null);
                }
            }
        }
    }
}
