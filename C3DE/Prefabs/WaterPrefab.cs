using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Prefabs
{
    public class WaterPrefab : Prefab
    {
        protected MeshRenderer renderer;
        protected WaterMaterial material;
        protected BoxCollider collider;

        public MeshRenderer Renderer
        {
            get { return renderer; }
        }

        public BoxCollider Collider
        {
            get { return collider; }
        }

        public WaterPrefab(string name, Scene scene)
            : base(name, scene)
        {
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
                material.MainTexture = Application.Content.Load<Texture2D>(waterTexture);

            if (!string.IsNullOrEmpty(bumpTexture))
                material.BumpTexture = Application.Content.Load<Texture2D>(bumpTexture);

            renderer.Material = material;
            renderer.Geometry.Size = size;
            renderer.Geometry.Generate();
            collider.Box = new BoundingBox(transform.Position, size);
        }
    }
}
