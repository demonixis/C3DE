using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Kinect.Scripts
{
    public class KinectManager
    {
        private bool _isAvailable;
        private Vector3 _max;
        private Vector3 _screenSizes;
        private KinectUserProfile _userProfil;
        private KinectSensor _kinectSensor;
        private Skeleton[] _cacheSkeletons;
        private Skeleton _cacheSkeleton;

        public bool IsAvailable
        {
            get { return _isAvailable; }
            protected set { _isAvailable = value; }
        }

        public KinectUserProfile User
        {
            get { return _userProfil; }
        }

        public KinectManager(int width, int height, int depth)
        {
            _max = new Vector3(0.4f);
            _userProfil = new KinectUserProfile(1);
            SetScreenSize(width, height, depth);
            Initialize();
        }

        public void SetScreenSize(int width, int height, int depth)
        {
            _screenSizes = new Vector3(width, height, depth);
        }

        private void Initialize()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                // We take the first (for now)
                _kinectSensor = KinectSensor.KinectSensors[0];

                // We adjust the defaults parameters for the sensor
                TransformSmoothParameters parameters = new TransformSmoothParameters
                {
                    Correction = 0.3f,
                    JitterRadius = 1.0f,
                    MaxDeviationRadius = 0.5f,
                    Prediction = 0.4f,
                    Smoothing = 0.7f
                };

                _kinectSensor.SkeletonStream.Enable(parameters);
                _kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

                try
                {
                    _kinectSensor.Start();
                    _isAvailable = true;
                }
                catch (System.IO.IOException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    _isAvailable = false;
                }
            }
            else
                _isAvailable = false;
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // No player
                if (skeletonFrame == null)
                    return;

                int skeletonsSize = skeletonFrame.SkeletonArrayLength;

                _cacheSkeletons = new Skeleton[skeletonsSize];

                skeletonFrame.CopySkeletonDataTo(_cacheSkeletons);

                Joint scaledJoint;

                for (int i = 0; i < skeletonsSize; i++)
                {
                    _cacheSkeleton = _cacheSkeletons[i];
                
                    if (_cacheSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        for (int j = 0, k = _cacheSkeleton.Joints.Count; j < k; j++)
                        {
                            scaledJoint = _cacheSkeleton.Joints[(JointType)j].ScaleTo((int)_screenSizes.X, (int)_screenSizes.Y, _max.X, _max.Y);
                            _userProfil.SetVector3(scaledJoint.JointType, scaledJoint.Position);
                        }
                    }

                    return;
                }
            }
        }
    }
}
