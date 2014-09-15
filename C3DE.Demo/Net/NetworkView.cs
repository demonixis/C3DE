using C3DE.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Components.Net
{
    public class NetworkView : Behaviour
    {
        private Vector3 _position;
        private Vector3 _lastPosition;
        private Vector3 _rotation;
        private Vector3 _lastRotation;
        private Vector3 _scale;
        private Vector3 _lastScale;
        private NetOutgoingMessage _outMessage;
        private float _elapsedTime;
        private bool _ready;
        private NetConnection _connection;
        public string Name { get; set; }

        public new Transform Transform
        {
            get { return transform; }
            set { SetTransform(value); }
        }

        internal string uid;

        public NetConnection Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                _ready = _connection != null;
            }
        }

        public bool IsMine()
        {
            if (Network.IsClient)
                return uid == Network.UniqId;

            return false;
        }

        public NetworkView() : base() { }

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
            _outMessage = Network.Client.CreateMessage();
            _outMessage.Write((byte)MSPacketType.Transform);
            _outMessage.Write((byte)type);
            _outMessage.Write(vector.X);
            _outMessage.Write(vector.Y);
            _outMessage.Write(vector.Z);
            Network.SendMessage(_outMessage);
        }

        internal void SetTransform(Transform tr)
        {
            transform = tr;
        }
    }
}
