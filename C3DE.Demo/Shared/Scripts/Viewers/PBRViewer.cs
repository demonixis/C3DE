using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.UI;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
    public class PBRViewer : Behaviour
    {
        private PBRMaterial _material;
        private PBRTerrainAtlasedMaterial _terrainMaterial;
        private Rectangle _rect;

        public override void Start()
        {
            var renderer = GetComponent<MeshRenderer>();

            if (renderer == null)
                throw new Exception("You need to attach a mesh renderer first.");

            _material = renderer.Material as PBRMaterial;
            _terrainMaterial = renderer.Material as PBRTerrainAtlasedMaterial;
            _rect = new Rectangle(0, Screen.VirtualHeight - 200, 150, 150);
        }

        public override void OnGUI(GUI gui)
        {
            if (_terrainMaterial != null)
            {
                gui.DrawTexture(new Rectangle(0, 0, 200, 200), _terrainMaterial.CombinedAlbedos);
                gui.DrawTexture(new Rectangle(0, 200, 200, 200), _terrainMaterial.CombinedNormals);
                gui.DrawTexture(new Rectangle(0, 400, 200, 200), _terrainMaterial.CombinedRMAO);
            }
            else if (_material != null)
                gui.DrawTexture(_rect, _material.RoughnessMetalicAOMap);
        }
    }
}
