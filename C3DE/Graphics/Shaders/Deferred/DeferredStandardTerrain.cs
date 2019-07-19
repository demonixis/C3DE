﻿using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandardTerrain : ForwardStandardTerrain
    {
        public DeferredStandardTerrain(StandardTerrainMaterial material) : base(material)
        {
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/StandardTerrain");
        }
    }
}