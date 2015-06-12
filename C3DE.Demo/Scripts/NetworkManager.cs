using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;
using C3DE.Net;
using C3DE.Prefabs.Meshes;
using C3DE.Geometries;
using C3DE.Components.Net;
using C3DE.Components.Controllers;
using C3DE.Utils;

namespace C3DE.Demo.Scripts
{
    public class NetworkManager : Behaviour
    {
        private NetworkView _netView;
        private MeshPrefab<CubeGeometry> _player;

        public override void Start()
        {
            Application.Engine.Components.Add(new Network(Application.Engine));
            _netView = AddComponent<NetworkView>();

            _player = new MeshPrefab<CubeGeometry>("Player");
            _player.Transform.Position = new Vector3(0, -5, 0);
            _player.AddComponent<NetworkView>();
            _player.AddComponent<NetThirdPersonController>();
            //_player.Enabled = false;

            Application.SceneManager.ActiveScene.Add(_player);
        }

        public override void OnGUI(GUI gui)
        {
            if (!Network.IsClient && !Network.IsServer)
            {
                if (gui.Button(new Rectangle(15, 15, 100, 45), "Create Server"))
                {
                    Network.StartServer("C3DE Network Game", "127.0.0.1", 13554, 4);
                    Network.JoinServer("C3DE Network Game", "127.0.0.1", 13554);
                }

                if (gui.Button(new Rectangle(15, 75, 100, 45), "Join Game"))
                {
                    Network.JoinServer("C3DE Network Game", "127.0.0.1", 13554);
                }
            }
        }

        public override void OnConnectedToServer()
        {
            SpawnPlayer();
        }

        public override void OnServerInitialized()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            sceneObject.Scene.MainCamera.GetComponent<OrbitController>().Enabled = false;
            Network.Instanciate(_player, RandomHelper.GetVector3(-5, 0.5f, -5, 5, 0.5f, 5), Vector3.Zero);
        }
    }
}
