using C3DE.Components;
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
        private Dictionary<string, int> _prefabs;
        private List<NetworkView> netViews;
        private List<Behaviour> _behaviours;
        public static int Id = -1;

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
            _prefabs = new Dictionary<string, int>();
            game.Exiting += OnGameExiting;
        }

        private void OnGameExiting(object sender, EventArgs e)
        {
            if (server != null)
                server.Stop();
        }

        private int GetNetViewBySceneObjectId(string id)
        {
            for (int i = 0, l = netViews.Count; i < l; i++)
            {
                if (netViews[i].GameObject.Id == id)
                    return i;
            }
            return -1;
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
                            var messageType = incMessage.ReadByte();

                            if (messageType == (byte)MSPacketType.Login)
                            {
                                var con = incMessage.SenderConnection;

                                Id = incMessage.ReadInt32(); ;

                                if (client.ServerConnection.RemoteUniqueIdentifier == con.RemoteUniqueIdentifier)
                                {
                                    var scripts = new List<Behaviour>(Application.SceneManager.ActiveScene.Behaviours);

                                    foreach (Behaviour script in scripts)
                                        script.OnConnectedToServer();
                                }

                                string prefabs = incMessage.ReadString();

                                if (!string.IsNullOrEmpty(prefabs))
                                {
                                    string[] temp = prefabs.Split(new char[] { '|' });
                                    string[] temp2 = null;

                                    for (int i = 0, l = temp.Length; i < l; i++)
                                    {
                                        temp2 = temp[i].Split(new char[] { '_' });

                                        var prefabId = temp2[0];
                                        var prefabCount = int.Parse(temp2[1]);

                                        _prefabs.Add(prefabId, prefabCount);
                                    }
                                }
                            }
                            else if (messageType == (byte)MSPacketType.New)
                            {
                                var wSize = incMessage.ReadInt32();
                                var prefabId = incMessage.ReadString();
                                var networkId = incMessage.ReadInt32();
                                var position = NetHelper.StringToVector3(incMessage.ReadString());
                                var rotation = NetHelper.StringToVector3(incMessage.ReadString());
                                var scale = NetHelper.StringToVector3(incMessage.ReadString());

                                // FIXME
                                if (networkId != Id)
                                    Instanciate(Scene.FindById(prefabId), position, rotation, networkId, false);

                                if (_prefabs.ContainsKey(prefabId))
                                    _prefabs[prefabId]++;
                                else
                                    _prefabs.Add(prefabId, 1);

                                if (netViews.Count == wSize)
                                    foreach (var nv in netViews)
                                        incMessage.ReadAllProperties(nv);
                            }
                            else if (messageType == (byte)MSPacketType.Transform)
                            {
                                var type = (MSTransformType)incMessage.ReadByte();
                                var id = incMessage.ReadString();
                                var vec = NetHelper.StringToVector3(incMessage.ReadString());
                                var index = GetNetViewBySceneObjectId(id);

                                if (!netViews[index].IsMine())
                                    netViews[index].SetTransform(type, vec);

                                foreach (var nv in netViews)
                                    incMessage.ReadAllProperties(nv);
                            }
                            else if (messageType == (byte)MSPacketType.WorldState)
                            {
                                int worldSize = incMessage.ReadInt32();

                                if (netViews.Count < worldSize)
                                {
                                    GameObject[] soCache = null;

                                    foreach (var keyValue in _prefabs)
                                    {
                                        // FIXME
                                        soCache = Scene.FindGameObjectsById(keyValue.Key);
                                        int max = keyValue.Value;

                                        if (soCache.Length > 0)
                                            max = max - soCache.Length;

                                        // FIXME
                                        for (int i = 0; i < max; i++)
                                            Instanciate(Scene.FindById(keyValue.Key), Vector3.Zero, Vector3.Zero, -1, false);
                                    }
                                }

                                for (int i = 0; i < worldSize; i++)
                                    incMessage.ReadAllProperties(netViews[i]);

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
                NetPeerConfiguration config = new NetPeerConfiguration(gameName);

                client = new NetClient(config);
                client.Start();
                client.Connect(ipAdress, port, outMessage);

                _instance._behaviours = Application.SceneManager.ActiveScene.Behaviours;

                IsClient = true;
            }
        }

        public static void SendMessage(NetOutgoingMessage message)
        {
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public static GameObject Instanciate(GameObject sceneObject, Vector3 position, Vector3 rotation)
        {
            return Instanciate(sceneObject, position, rotation, Id, true);
        }

        private static GameObject Instanciate(GameObject sceneObject, Vector3 position, Vector3 rotation, int uniqId, bool notifyServer)
        {
            var clone = Scene.Instanciate(sceneObject, position, rotation);
            clone.Transform.Position = position;
            clone.Transform.Rotation = rotation;

            NetworkView netView = clone.GetComponent<NetworkView>();
            if (netView == null)
                netView = clone.AddComponent<NetworkView>();

            netView.networkId = uniqId;
            _instance.netViews.Add(netView);

            if (notifyServer)
            {
                outMessage = client.CreateMessage();
                outMessage.Write((byte)MSPacketType.New);
                outMessage.Write(sceneObject.Id);
                outMessage.Write(netView.networkId);
                outMessage.Write(NetHelper.Vector3ToString(clone.Transform.Position));
                outMessage.Write(NetHelper.Vector3ToString(clone.Transform.Rotation));
                outMessage.Write(NetHelper.Vector3ToString(clone.Transform.LocalScale));

                client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered, 0);
            }

            return sceneObject;
        }

        private static void Destroy(GameObject sceneObject)
        {
            var netView = sceneObject.GetComponent<NetworkView>();

            if (netView != null)
            {
                outMessage = client.CreateMessage();
                outMessage.Write((byte)MSPacketType.Remove);
                outMessage.Write(netView.uniqId);

                client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered, 0);

                _instance.netViews.Remove(netView);
            }

            Scene.Destroy(sceneObject);
        }
    }
}
