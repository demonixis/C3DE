using C3DE.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;

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
        private NetIncomingMessage _incMessage;
        private NetOutgoingMessage _outMessage;
        private float _elapsedTime;

        public string Name { get; set; }
        public NetConnection Connection { get; set; }
/*
        public bool IsMine
        {
            get
            {
                //if (Connection != null && Network.Client != null)
                  //  return Connection.RemoteUniqueIdentifier == Network.Client.UniqueIdentifier;

                return false;
            }
        }*/

        public NetworkView()
            : base()
        {

        }

        public override void Start()
        {

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
