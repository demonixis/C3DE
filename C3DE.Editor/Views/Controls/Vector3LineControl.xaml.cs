using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C3DE.Editor.Views.Controls
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using XnaVector3 = Microsoft.Xna.Framework.Vector3;

    public partial class Vector3LineControl : UserControl, INotifyPropertyChanged
    {
        private XnaVector3 _vec3;

        public XnaVector3 Vector3
        {
            get { return _vec3; }
            set
            {
                if (value != _vec3)
                {
                    _vec3 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public float X
        {
            get { return _vec3.X; }
            set
            {
                if (_vec3.X != value)
                {
                    _vec3.X = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public float Y
        {
            get { return _vec3.Y; }
            set
            {
                if (_vec3.Y != value)
                {
                    _vec3.Y = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public float Z
        {
            get { return _vec3.Z; }
            set
            {
                if (_vec3.Z != value)
                {
                    _vec3.Z = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Vector3LineControl()
        {
            InitializeComponent();
        }
    }
}
