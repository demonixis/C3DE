using C3DE.Components;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class RayPicking : Behaviour
    {
        private Scene scene;
        private Camera camera;
        private bool _hit;

        public override void Start()
        {
            scene = sceneObject.Scene;
            camera = scene.MainCamera;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Mouse.Clicked())
            {
                var ray = camera.GetRay(Input.Mouse.Position);

                _hit = scene.Raycast(ray.Position, ray.Direction, 50);
            }
        }

        public override void OnGUI(UI.GUI gui)
        {
            gui.Box(new Rectangle(10, 10, 70, 30), _hit ? "Hit" : "Nop");
        }
    }
}
