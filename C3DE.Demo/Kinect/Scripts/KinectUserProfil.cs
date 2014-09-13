using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Demo.Kinect.Scripts
{
    public class KinectUserProfile
    {
        private int _id;
        private Vector3[] _positions;
        private Vector3[] _lastPositions;
        private int _size;
        private Vector3 _cacheDelta;

        public int Id
        {
            get { return _id; }
        }

        public KinectUserProfile(int id)
        {
            _id = id;

            var elem = Enum.GetValues(typeof(JointType));
            _size = elem.Length;

            _positions = new Vector3[_size];
            _lastPositions = new Vector3[_size];
        }

        public void SetVector3(JointType jointType, SkeletonPoint position)
        {
            _lastPositions[(int)jointType] = _positions[(int)jointType];
            _positions[(int)jointType] = new Vector3(position.X, position.Y, position.Z);
        }

        public void Update()
        {
            for (int i = 0; i < _size; i++)
                _lastPositions[i] = _positions[i];
        }

        public Vector3 GetPosition(JointType type)
        {
            _cacheDelta.X = _positions[(int)type].X;
            _cacheDelta.Y = _positions[(int)type].Y;
            _cacheDelta.Z = _positions[(int)type].Z;
            return _cacheDelta;
        }

        public Vector3 Delta(JointType type)
        {
            _cacheDelta.X = _positions[(int)type].X - _lastPositions[(int)type].X;
            _cacheDelta.Y = _positions[(int)type].Y - _lastPositions[(int)type].Y;
            _cacheDelta.Z = _positions[(int)type].Z - _lastPositions[(int)type].Z;
            return _cacheDelta;
        }
    }
}