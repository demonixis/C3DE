using C3DE.Components.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Net
{
    public class Network : GameComponent
    {
        private static Network _instance;
        private static MasterServer server;
        private static NetClient client;
        private static NetIncomingMessage incMessage;
        private static NetOutgoingMessage outMessage;
        private static float _sendRate;
        private float _elapsedTime;
        private List<NetworkView> netViews;

        public static float SendRate
        {
            get { return _sendRate; }
            set { _sendRate = value; }
        }

        public static bool IsClient
        {
            get;
            protected set;
        }

        public static bool IsServer
        {
            get;
            protected set;
        }

        internal static NetClient Client
        {
            get { return client; }
        }

        public Network(Game game)
            : base(game)
        {
            _instance = this;
            _sendRate = 0.05f; // 50 ms 
            _elapsedTime = 0;
            netViews = new List<NetworkView>();
            game.Exiting += OnGameExiting;
        }

        private void OnGameExiting(object sender, EventArgs e)
        {
            if (server != null)
                server.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsClient)
            {
                _elapsedTime += Time.DeltaTime;

                if (_elapsedTime >= _sendRate)
                {
                    // Check for server messages.
                    while ((incMessage = client.ReadMessage()) != null)
                    {
                        if (incMessage.MessageType == NetIncomingMessageType.Data)
                        {
                            if (incMessage.ReadByte() == (byte)MSPacketType.WorldState)
                            {
                                int worldSize = incMessage.ReadInt32();

                                if (netViews.Count != worldSize)
                                {
                                    var diff = worldSize - netViews.Count;

                                    // Just for test...
                                    for (int i = 0; i < diff; i++)
                                    {
                                        var so = Network.Instanciate<C3DE.Prefabs.Meshes.MeshPrefab<C3DE.Geometries.CubeGeometry>>(Vector3.Zero, Vector3.Zero);
                                        so.AddComponent<C3DE.Components.Controllers.ThirdPersonController>();
                                    }
                                }

                                for (int i = 0; i < worldSize; i++)
                                {
                                    incMessage.ReadAllProperties(netViews[i]);
                                }

                                // Check what has changed
                                // Apply to all objects
                            }
                        }
                    }

                    _elapsedTime = 0;
                }
            }
        }

        public static void StartServer(string gameName, string ipAdress, int port, int maxPlayers)
        {
            if (!IsServer)
            {
                server = new MasterServer(gameName, ipAdress, port, maxPlayers);
                server.Start();
                IsServer = true;
            }
        }

        public static void StopServer()
        {
            if (IsServer)
            {
                server.Stop();
                IsServer = false;
            }
        }

        public static void JoinServer(string gameName, string ipAdress, int port)
        {
            if (!IsServer || !IsClient)
            {
                IsClient = true;

                NetPeerConfiguration config = new NetPeerConfiguration(gameName);

                client = new NetClient(config);
                client.Start();

                outMessage = client.CreateMessage();
                outMessage.Write((byte)MSPacketType.Login);
                outMessage.Write(gameName);

                client.Connect(ipAdress, port, outMessage);
            }
        }

        public static void SendMessage(NetOutgoingMessage message)
        {
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public static SceneObject Instanciate<T>(Vector3 position, Vector3 rotation) where T : SceneObject, new()
        {
            var sceneObject = Activator.CreateInstance<T>();
            sceneObject.Transform.Position = position;
            sceneObject.Transform.Rotation = rotation;

            var netView = sceneObject.AddComponent<NetworkView>();
            _instance.netViews.Add(netView);

            Application.SceneManager.ActiveScene.Add(sceneObject);

            return sceneObject;
        }
    }
}
