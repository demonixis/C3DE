using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;
using C3DE.Net;
using C3DE.Prefabs.Meshes;
using C3DE.Geometries;
using C3DE.Components.Net;
using C3DE.Components.Controllers;

namespace C3DE.Demo.Scripts
{
    public class NetworkManager : Behaviour
    {
        private NetworkView _netView;

        public override void Start()
        {
            Application.Game.Components.Add(new Network(Application.Game));
            _netView = AddComponent<NetworkView>();
        }

        public override void OnGUI(GUI gui)
        {
            if (!Network.IsClient && !Network.IsServer)
            {
                if (gui.Button(new Rectangle(15, 15, 100, 45), "Create Server"))
                {
                    Network.StartServer("C3DE Network Game", "127.0.0.1", 4096, 4);
                    Network.JoinServer("C3DE Network Game", "127.0.0.1", 4096);
                    SpawnPlayer();
                }

                if (gui.Button(new Rectangle(15, 75, 100, 45), "Join Game"))
                {
                    Network.JoinServer("C3DE Network Game", "127.0.0.1", 4096);
                    SpawnPlayer();
                }
            }
        }

        private void SpawnPlayer()
        {
            sceneObject.Scene.MainCamera.GetComponent<OrbitController>().Enabled = false;

            var so = Network.Instanciate<MeshPrefab<CubeGeometry>>(new Vector3(1, 0.5f, 2), Vector3.Zero);
            so.AddComponent<ThirdPersonController>();
        }
    }
}
