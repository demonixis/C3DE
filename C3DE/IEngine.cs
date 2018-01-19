using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using System;

namespace C3DE
{
    public interface IEngine
    {
        Game Game { get; }
        BaseRenderer Renderer { get; set; }
        GameComponentCollection Components { get; }
        bool IsFixedTimeStep { get; set; }
        bool IsMouseVisible { get; set; }
        event Action<BaseRenderer> RendererChanged;
    }
}
