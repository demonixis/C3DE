using C3DE.Components;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts.Diagnostic
{
    public class StatsDisplay : Behaviour
    {
        public override void OnGUI(UI.GUI gui)
        {
            var metrics = Application.GraphicsDevice.Metrics;

            gui.Box(new Rectangle(10, 10, 220, 230), "Stats");

            gui.Label(new Vector2(15, 35), $"Clear");
            gui.Label(new Vector2(150, 35), $"{metrics.ClearCount}");

            gui.Label(new Vector2(15, 55), $"Primitive"); 
            gui.Label(new Vector2(150, 55), $"{metrics.PrimitiveCount}");

            gui.Label(new Vector2(15, 75), $"Target");
            gui.Label(new Vector2(150, 75), $"{metrics.TargetCount}");
       
            gui.Label(new Vector2(15, 95), $"Texture");
            gui.Label(new Vector2(150, 95), $"{metrics.TextureCount}");
     
            gui.Label(new Vector2(15, 115), $"Draw");
            gui.Label(new Vector2(150, 115), $"{metrics.DrawCount}");
            
            gui.Label(new Vector2(15, 135), $"VertexShader");
            gui.Label(new Vector2(150, 135), $"{metrics.VertexShaderCount}");
           
            gui.Label(new Vector2(15, 155), $"PixelShader");
            gui.Label(new Vector2(150, 155), $"{metrics.PixelShaderCount}");

            gui.Label(new Vector2(15, 175), $"Lights");
            gui.Label(new Vector2(150, 175), $"{Scene.current?.LightCount ?? 0}");

            gui.Label(new Vector2(15, 195), $"Sprite");
            gui.Label(new Vector2(150, 195), $"{metrics.SpriteCount}");

            gui.Label(new Vector2(15, 215), $"FPS");
            gui.Label(new Vector2(150, 215), $"{Application.Engine.FPS}");

        }
    }
}
