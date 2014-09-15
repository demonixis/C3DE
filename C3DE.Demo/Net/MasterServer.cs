using C3DE.Components;
using C3DE.Components.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
        protected string _gameName;
        protected NetServer _server;
        protected NetPeerConfiguration _netConfig;
        protected List<NetworkView> _gameWorldState;
        protected bool _running;
        protected int _worldCount;
        protected HostData[] _hostData;
        private Vector3 _cacheVec3;

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
            _gameName = gameName;
            _mainThread = new Thread(ProcessServer);

            _hostData = new HostData[1];
            _hostData[0] = new HostData()
            {
                Comment = "",
                ConnectedPlayers = 0,
                GameName = _gameName,
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

        public HostData[] RequestHostList()
        {
            return _hostData;
        }

        protected Vector3 GetVector3(NetIncomingMessage message)
        {
            _cacheVec3.X = message.ReadFloat();
            _cacheVec3.Y = message.ReadFloat();
            _cacheVec3.Z = message.ReadFloat();
            return _cacheVec3;
        }

        protected int IndexOf(NetConnection connection)
        {
            for (int i = 0; i < _worldCount; i++)
            {
                if (_gameWorldState[i].Connection == connection)
                    return i;
            }

            return -1;
        }

        protected virtual void ProcessServer()
        {
            _running = true;
            _gameWorldState = new List<NetworkView>();
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

                            var network = new NetworkView();
                            network.Name = incMessage.ReadString();
                            network.SetTransform(new Transform());
                            network.Connection = incMessage.SenderConnection;

                            _gameWorldState.Add(network);
                            _worldCount++;

                            outMessage = _server.CreateMessage();
                            outMessage.Write((byte)MSPacketType.WorldState);
                            outMessage.Write(_worldCount);

                            for (int i = 0; i < _worldCount; i++)
                                outMessage.WriteAllProperties(_gameWorldState[i]);

                            _server.SendMessage(outMessage, incMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            Debug.Log("Approved new connection and updated world");

                            break;

                        case NetIncomingMessageType.Data:

                            if (incMessage.ReadByte() == (byte)MSPacketType.Transform)
                            {
                                var index = IndexOf(incMessage.SenderConnection);

                                if (index > -1)
                                {
                                    byte type = incMessage.ReadByte();

                                    // Update the correct entity
                                    if ((byte)MSTransformType.Translation == type)
                                        _gameWorldState[index].Transform.Position = GetVector3(incMessage);

                                    else if ((byte)MSTransformType.Rotation == type)
                                        _gameWorldState[index].Transform.Rotation = GetVector3(incMessage);

                                    else if ((byte)MSTransformType.Scale == type)
                                        _gameWorldState[index].Transform.LocalScale = GetVector3(incMessage);

                                    outMessage = _server.CreateMessage();
                                    outMessage.Write((byte)MSPacketType.WorldState);
                                    outMessage.Write(_worldCount);

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
                                var index = IndexOf(incMessage.SenderConnection);

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
