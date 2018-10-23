using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;
using C3DE.Net;
using C3DE.Components.Net;
using C3DE.Components.Controllers;
using C3DE.Utils;

namespace C3DE.Demo.Scripts
{
    public class Player : GameObject
    {
        public Player()
            : base()
        {
            AddComponent<NetworkView>();
            AddComponent<NetThirdPersonController>();
            Transform.Position = new Vector3(0, -5, 0);
            IsPrefab = true;
        }
    }

    public class NetworkManager : Behaviour
    {
        private NetworkView _netView;
        private Player _player;

        public override void Start()
        {
            Application.Engine.Components.Add(new Network(Application.Engine));
            _netView = AddComponent<NetworkView>();

            _player = new Player();

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
            Camera.Main.GetComponent<OrbitController>().Enabled = false;
            Network.Instanciate(_player, RandomHelper.GetVector3(-5, 0.5f, -5, 5, 0.5f, 5), Vector3.Zero);
        }
    }
}
