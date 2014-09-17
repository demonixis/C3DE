using C3DE.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Components.Net
{
    public class NetworkView : Behaviour
    {
        internal int uniqId;
        internal int networkId;
        private Vector3 _position;
        private Vector3 _lastPosition;
        private Vector3 _rotation;
        private Vector3 _lastRotation;
        private Vector3 _scale;
        private Vector3 _lastScale;
        private NetOutgoingMessage _outMessage;
        private float _elapsedTime;
        private NetConnection _connection;

        public int UniqId
        {
            get { return uniqId; }
            internal protected set { uniqId = value; }
        }

        public int NetworkId
        {
            get { return networkId; }
            internal protected set { networkId = value; }
        }

        public NetConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public new Transform Transform
        {
            get { return transform; }
            set { SetTransform(value); }
        }

        public NetworkView()
            : base()
        {
            uniqId = -1;
            networkId = -1;
        }

        public bool IsMine()
        {
            if (Network.IsClient)
                return networkId == Network.Id;

            return false;
        }

        public override void Update()
        {
            if (Network.IsClient)
            {
                _elapsedTime += Time.DeltaTime;

                if (_elapsedTime >= Network.SendRate)
                {
                    _lastPosition = _position;
                    _position = transform.Position;

                    _lastRotation = _rotation;
                    _rotation = transform.Rotation;

                    _lastScale = _scale;
                    _scale = transform.LocalScale;

                    if (_lastPosition != _position)
                        SendTransformChange(MSTransformType.Translation, ref _position);

                    if (_lastRotation != _rotation)
                        SendTransformChange(MSTransformType.Rotation, ref _position);

                    if (_lastScale != _scale)
                        SendTransformChange(MSTransformType.Scale, ref _position);

                    _elapsedTime = 0;
                }
            }
        }

        protected void SendTransformChange(MSTransformType type, ref Vector3 vector)
        {
            // 0: Packet type
            // 1: Type
            // 2: Uniq Id
            // 3-5: X/Y/Z
            _outMessage = Network.Client.CreateMessage();
            _outMessage.Write((byte)MSPacketType.Transform);
            _outMessage.Write((byte)type);
            _outMessage.Write(uniqId);
            _outMessage.Write(vector.X);
            _outMessage.Write(vector.Y);
            _outMessage.Write(vector.Z);
            Network.SendMessage(_outMessage);
        }

        internal void SetTransform(Transform tr)
        {
            transform = tr;
        }

        internal void SetTransform(MSTransformType type, Vector3 value)
        {
            if (type == MSTransformType.Translation)
                transform.Position = value;
            else if (type == MSTransformType.Rotation)
                transform.Rotation = value;
            else
                transform.LocalScale = value;
        }

        public override object Clone()
        {
            NetworkView netView = new NetworkView();

            netView.uniqId = -1;
            netView.networkId = networkId;
            netView._elapsedTime = _elapsedTime;
            netView._lastPosition = _lastPosition;
            netView._lastRotation = _lastRotation;
            netView._lastScale = _lastScale;
            netView._position = _position;
            netView._rotation = _rotation;
            netView._scale = _scale;
            netView.Connection = null;

            return netView;
        }
    }
}
