using C3DE.Graphics;

namespace C3DE.Components.Rendering
{
    public class ProceduralSkyController : Component
    {
        public override void Update()
        {
            base.Update();

            var scene = _gameObject?.Scene;
            if (scene == null)
                return;

            var skybox = scene.RenderSettings.Skybox;
            if (skybox.Mode != SkyboxMode.Procedural || !skybox.ProceduralSettings.Enabled)
                return;

            skybox.Update(scene);
        }
    }
}
