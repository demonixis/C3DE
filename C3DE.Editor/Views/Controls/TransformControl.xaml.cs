using C3DE.Components;
using System;
using System.Windows.Controls;

namespace C3DE.Editor.Views.Controls
{
    public partial class TransformControl : UserControl
    {
        private TransformChanged _eventCache;
        private bool _initialized;

        public float PositionX
        {
            get { return ParseValue(transformPositionX.Text); }
            set { transformPositionX.Text = value.ToString(); }
        }

        public float PositionY
        {
            get { return ParseValue(transformPositionY.Text); }
            set { transformPositionY.Text = value.ToString(); }
        }

        public float PositionZ
        {
            get { return ParseValue(transformPositionZ.Text); }
            set { transformPositionZ.Text = value.ToString(); }
        }

        public float RotationX
        {
            get { return ParseValue(transformRotationX.Text); }
            set { transformRotationX.Text = value.ToString(); }
        }

        public float RotationY
        {
            get { return ParseValue(transformRotationY.Text); }
            set { transformRotationY.Text = value.ToString(); }
        }

        public float RotationZ
        {
            get { return ParseValue(transformRotationZ.Text); }
            set { transformRotationZ.Text = value.ToString(); }
        }

        public float ScaleX
        {
            get { return ParseValue(transformScaleX.Text); }
            set { transformScaleX.Text = value.ToString(); }
        }

        public float ScaleY
        {
            get { return ParseValue(transformScaleY.Text); }
            set { transformScaleY.Text = value.ToString(); }
        }

        public float ScaleZ
        {
            get { return ParseValue(transformScaleZ.Text); }
            set { transformScaleZ.Text = value.ToString(); }
        }

        private void Notify(TransformChangeType type, float x, float y, float z)
        {
            _eventCache.Set(type, x, y, z);
            Messenger.Notify("Editor.TransformChanged", _eventCache);
        }

        public TransformControl()
        {
            _eventCache = new TransformChanged();
            InitializeComponent();
            Reset();
            Messenger.Register("Editor.TransformUpdated", OnTransformUpdated);
            _initialized = true;
        }

        public float ParseValue(string value)
        {
            float result = 0.0f;
            float.TryParse(value, out result);
            return result;
        }

        public void Set(float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz)
        {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
            RotationX = rx;
            RotationY = ry;
            RotationZ = rz;
            ScaleX = sx;
            ScaleY = sy;
            ScaleZ = sz;
        }

        public void Reset()
        {
            PositionX = 0;
            PositionY = 0;
            PositionZ = 0;
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;
            ScaleX = 1;
            ScaleY = 1;
            ScaleZ = 1;
        }

        private void OnValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && _initialized)
            {
                var tag = textBox.Tag.ToString();

                if (tag == "Position")
                    Notify(TransformChangeType.Position, PositionX, PositionY, PositionZ);
                else if (tag == "Rotation")
                    Notify(TransformChangeType.Rotation, RotationX, RotationY, RotationZ);
                else if (tag == "Scale")
                    Notify(TransformChangeType.Scale, ScaleX, ScaleY, ScaleZ);
            }
        }

        private void OnTransformUpdated(BasicMessage m)
        {
            var data = m as TransformChanged;
            if (data != null)
            {
                if (data.ChangeType == TransformChangeType.Position)
                {
                    PositionX = data.X;
                    PositionY = data.Y;
                    PositionZ = data.Z;
                }
                else if (data.ChangeType == TransformChangeType.Rotation)
                {
                    RotationX = data.X;
                    RotationY = data.Y;
                    RotationZ = data.Z;
                }
                else if (data.ChangeType == TransformChangeType.Scale)
                {
                    ScaleX = data.X;
                    ScaleY = data.Y;
                    ScaleZ = data.Z;
                }
            }
        }
    }
}
