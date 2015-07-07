using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Prefabs
{
    public class WaterPrefab : SceneObject
    {
        protected MeshRenderer renderer;
        protected WaterMaterial material;
        protected BoxCollider collider;

        public MeshRenderer Renderer
        {
            get { return renderer; }
        }

        public WaterMaterial Material
        {
            get { return material; }
        }

        public BoxCollider Collider
        {
            get { return collider; }
        }

        public WaterPrefab(string name)
            : this()
        {
            Name = name;
        }

        public WaterPrefab()
            : base()
        {
            Name = "WaterPrefab-" + System.Guid.NewGuid();
            renderer = AddComponent<MeshRenderer>();
            renderer.CastShadow = false;
            renderer.ReceiveShadow = false;
            renderer.Geometry = new PlaneGeometry();
            collider = AddComponent<BoxCollider>();
            collider.IsPickable = false;
        }

        public void Generate(string waterTexture, string bumpTexture, Vector3 size)
        {
            if (scene == null)
                throw new Exception("You need to attach this prefab to the scene.");

            material = new WaterMaterial(scene);

            if (!string.IsNullOrEmpty(waterTexture))
                material.Texture = Application.Content.Load<Texture2D>(waterTexture);

            if (!string.IsNullOrEmpty(bumpTexture))
                material.NormalMap = Application.Content.Load<Texture2D>(bumpTexture);

            renderer.Material = material;
            renderer.Geometry.Size = size;
            renderer.Geometry.Buid();
            collider.Box = new BoundingBox(transform.Position, size);
        }
    }
}
