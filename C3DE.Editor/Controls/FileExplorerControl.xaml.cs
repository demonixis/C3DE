using System;
using System.Collections.Generic;
using System.IO;
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

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Interaction logic for FileExplorerControl.xaml
    /// </summary>
    public partial class FileExplorerControl : UserControl
    {
        private static TreeViewItem dummyNode = null;

        public FileExplorerControl()
        {
            InitializeComponent();
        }

        public void ShowFilesForDirectory(string path)
        {
            FileExplorerRoot.Items.Clear();
            TreeViewItem item = null;
            TreeViewItem root = new TreeViewItem();
            root.Header = "Assets";

            foreach (string directory in Directory.GetDirectories(path))
            {
                item = new TreeViewItem();
                item.Header = directory.Substring(directory.LastIndexOf("\\") + 1);
                item.Tag = directory;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += OnFolderExpanded;
                root.Items.Add(item);
            }

            FileExplorerRoot.Items.Add(root);
        }

        private void OnFolderExpanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();

                try
                {
                    foreach (string directory in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = directory.Substring(directory.LastIndexOf("\\") + 1);
                        subitem.Tag = directory;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += OnFolderExpanded;
                        item.Items.Add(subitem);
                    }

                    foreach (string file in Directory.GetFiles(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = file.Substring(file.LastIndexOf("\\") + 1);
                        subitem.Tag = file;
                        subitem.FontWeight = FontWeights.Light;
                        subitem.Expanded += OnSubItemClick;
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }

        private void OnSubItemClick(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            Debug.Log(item.Tag);
        }
    }
}
