using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.UI;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
    public class WeightMapViewer : Behaviour
    {
        private StandardTerrainMaterial _material;
        private Rectangle _rect;

        public override void Start()
        {
            var renderer = GetComponent<MeshRenderer>();

            if (renderer == null)
                throw new Exception("You need to attach a mesh renderer first.");
                
            var mat = renderer.Material as StandardTerrainMaterial;
            _material = mat ?? throw new Exception("You need to use a TerrainMaterial with a non null weightMap.");
            _rect = new Rectangle(0, Screen.VirtualHeight - 200, 150, 150);
        }

        public override void OnGUI(GUI gui)
        {
            gui.DrawTexture(_rect, _material.WeightTexture);
        }
    }
}
