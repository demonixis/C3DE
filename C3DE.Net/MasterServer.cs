using C3DE.Components;
using C3DE.Components.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace C3DE.Net
{
    public enum MSPacketType
    {
        Login = 0, New, Remove, Transform, WorldState
    }

    public enum MSTransformType
    {
        Translation, Rotation, Scale
    }

    public class MasterServer
    {
        protected Thread _mainThread;
        protected NetServer _server;
        protected NetPeerConfiguration _netConfig;
        protected int _playerCount;
        protected List<NetworkView> _gameWorldState;
        protected Dictionary<int, int> _instancedPrefabs;
        protected bool _running;
        protected int _worldCount;
        protected HostData _hostData;
        private Vector3 _cacheVec3;
        private int _netCounter;

        public MasterServer(string gameName, string ip, int port, int maxConnections)
        {
            _netConfig = new NetPeerConfiguration(gameName);
            _netConfig.Port = port;
            _netConfig.MaximumConnections = maxConnections;
            _netConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _server = new NetServer(_netConfig);
            _running = false;
            _worldCount = 0;
            _cacheVec3 = Vector3.Zero;
            _mainThread = new Thread(ProcessServer);
            _netCounter = 0;
            _hostData = new HostData()
            {
                Comment = "",
                ConnectedPlayers = 0,
                GameName = gameName,
                GUID = System.Guid.NewGuid().ToString(),
                IpAdress = ip,
                PlayerLimit = maxConnections,
                Port = port
            };
        }

        public void Start()
        {
            if (_running)
                _running = false;

            _server.Start();
            _mainThread.Start();
            Debug.Log("Server Started");
        }

        public void Stop()
        {
            _running = false;
            _server.Shutdown("Server shuting down...");
            Debug.Log("Server Stopped");
        }

        public HostData RequestHostList()
        {
            return _hostData;
        }

        protected string GetPrefabs()
        {
            StringBuilder sb = new StringBuilder();
            int size = _instancedPrefabs.Count;
            int i = 0;

            foreach (var keyValue in _instancedPrefabs)
            {
                sb.Append(string.Concat(keyValue.Key, "_", keyValue.Value));

                if (i < size - 1)
                    sb.Append("|");
                i++;
            }

            return sb.ToString();
        }

        protected int IndexOfState(NetConnection connection)
        {
            for (int i = 0; i < _worldCount; i++)
            {
                if (_gameWorldState[i].Connection == connection)
                    return i;
            }

            return -1;
        }

        protected int IndexOfState(int uuid)
        {
            for (int i = 0; i < _worldCount; i++)
            {
                if (_gameWorldState[i].uniqId == uuid)
                    return i;
            }

            return -1;
        }

        protected virtual void ProcessServer()
        {
            _running = true;
            _gameWorldState = new List<NetworkView>();
            _instancedPrefabs = new Dictionary<int, int>();
            _playerCount = 0;
            NetIncomingMessage incMessage;
            NetOutgoingMessage outMessage;
            DateTime time = DateTime.Now;
            TimeSpan timeToPass = new TimeSpan(0, 0, 0, 0, 30); // 30ms

            Debug.Log("Waiting for new connections and updating current world entities");

            while (_running)
            {
                incMessage = _server.ReadMessage();

                if (incMessage != null)
                {
                    switch (incMessage.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            Debug.Log("Incoming Login");

                            incMessage.SenderConnection.Approve();

                            Debug.Log("Approved new connection and updated world");

                            outMessage = _server.CreateMessage();
                            outMessage.Write((byte)MSPacketType.Login);
                            outMessage.Write(++_playerCount);

                            outMessage.Write(GetPrefabs());

                            _server.SendMessage(outMessage, incMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;

                        case NetIncomingMessageType.Data:
                            var messageType = (byte)incMessage.ReadByte();

                            // 0: Add code   (byte)
                            // 1: PrefabID   (int)
                            // 3: Network Id (int)
                            // 3: Position   (String)
                            // 4: Roation    (String)
                            // 5: Scale      (String)
                            if (messageType == (byte)MSPacketType.New)
                            {
                                var prefabId = incMessage.ReadInt32();
                                var networkId = incMessage.ReadInt32();

                                if (_instancedPrefabs.ContainsKey(prefabId))
                                    _instancedPrefabs[prefabId]++;
                                else
                                    _instancedPrefabs.Add(prefabId, 1);

                                var network = new NetworkView();
                                network.uniqId = _netCounter++;
                                network.networkId = networkId;
                                network.Connection = incMessage.SenderConnection;

                                var position = incMessage.ReadString();
                                var rotation = incMessage.ReadString();
                                var scale = incMessage.ReadString();

                                var transform = new Transform();
                                transform.Position = NetHelper.StringToVector3(position);
                                transform.Rotation = NetHelper.StringToVector3(rotation);
                                transform.LocalScale = NetHelper.StringToVector3(scale);
                                network.SetTransform(transform);

                                _gameWorldState.Add(network);
                                _worldCount++;

                                outMessage = _server.CreateMessage();
                                outMessage.Write((byte)MSPacketType.New);
                                outMessage.Write(_worldCount);
                                outMessage.Write(prefabId);
                                outMessage.Write(networkId);
                                outMessage.Write(position);
                                outMessage.Write(rotation);
                                outMessage.Write(scale);

                                for (int i = 0; i < _worldCount; i++)
                                    outMessage.WriteAllProperties(_gameWorldState[i]);

                                _server.SendMessage(outMessage, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                            }

                            // 0: Remove code
                            // 1: Network ID
                            else if (messageType == (byte)MSPacketType.Remove)
                            {
                                // FIXME
                            }

                            else if (messageType == (byte)MSPacketType.Transform)
                            {
                                var index = IndexOfState(incMessage.SenderConnection);

                                if (index > -1)
                                {
                                    byte type = incMessage.ReadByte();
                                    int id = incMessage.ReadInt32();
                                    var str = incMessage.ReadString();
                                    Vector3 vec3 = NetHelper.StringToVector3(str);

                                    // Update the correct entity
                                    if ((byte)MSTransformType.Translation == type)
                                        _gameWorldState[index].Transform.Position = vec3;

                                    else if ((byte)MSTransformType.Rotation == type)
                                        _gameWorldState[index].Transform.Rotation = vec3;

                                    else if ((byte)MSTransformType.Scale == type)
                                        _gameWorldState[index].Transform.LocalScale = vec3;

                                    outMessage = _server.CreateMessage();
                                    outMessage.Write((byte)MSPacketType.Transform);
                                    outMessage.Write(type);
                                    outMessage.Write(id);
                                    outMessage.Write(NetHelper.Vector3ToString(vec3));

                                    for (int i = 0; i < _worldCount; i++)
                                        outMessage.WriteAllProperties(_gameWorldState[i]);

                                    _server.SendMessage(outMessage, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                                }
                            }

                            break;

                        case NetIncomingMessageType.StatusChanged:

                            Debug.Log(incMessage.SenderConnection.ToString() + " status changed. " + (NetConnectionStatus)incMessage.SenderConnection.Status);

                            if (incMessage.SenderConnection.Status == NetConnectionStatus.Disconnected || incMessage.SenderConnection.Status == NetConnectionStatus.Disconnecting)
                            {
                                var index = IndexOfState(incMessage.SenderConnection);

                                if (index > 0)
                                {
                                    _gameWorldState.RemoveAt(index);
                                    _worldCount--;
                                }
                            }

                            break;

                        default:
                            break;
                    }
                }

                // If 30ms has passed
                if ((time + timeToPass) < DateTime.Now)
                {
                    if (_server.ConnectionsCount != 0)
                    {
                        outMessage = _server.CreateMessage();
                        outMessage.Write((byte)MSPacketType.WorldState);
                        outMessage.Write(_worldCount);

                        for (int i = 0; i < _worldCount; i++)
                            outMessage.WriteAllProperties(_gameWorldState[i]);

                        _server.SendMessage(outMessage, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                    }

                    time = DateTime.Now;
                }
            }
        }
    }
}
