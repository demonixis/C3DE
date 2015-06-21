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
    /// <summary>
    /// Interaction logic for SceneListControl.xaml
    /// </summary>
    public partial class SceneListControl : UserControl
    {
        private Dictionary<string, int> _itemMapping;

        public SceneListControl()
        {
            InitializeComponent();
            _itemMapping = new Dictionary<string, int>();
            Messenger.Register("Editor.SceneObjectChanged", OnSceneObjectChanged);
        }

        public void AddItem(string name)
        {
            var index = sceneTreeView.Items.Add(name);
            //if (index > -1)
                //_itemMapping.Add(name, index);
        }

        public void RemoveItem(string name)
        {
            var index = sceneTreeView.Items.IndexOf(name);
            if (index > -1)
            {
                sceneTreeView.Items.RemoveAt(index);
                //_itemMapping.Remove(name);
            }
        }

        public void RenameItem(string name, string newName)
        {
            var index = sceneTreeView.Items.IndexOf(name);
            if (index > -1)
            {
                sceneTreeView.Items[index] = newName;
                //_itemMapping.Remove(name);
                //_itemMapping.Add(newName, index);
            }
        }

        private void OnSceneObjectChanged(BasicMessage m)
        {
            var data = m as SceneObjectControlChanged;
        }
    }
}
