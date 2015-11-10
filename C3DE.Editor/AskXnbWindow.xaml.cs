using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    public class TypeSelectedEventArgs : EventArgs
    {
        public Type Type { get; protected set; }
        public string Path { get; protected set; }

        public TypeSelectedEventArgs(Type type, string path)
            : base()
        {
            Type = type;
            Path = path;
        }
    }

    public partial class AskXnbWindow : Window
    {
        private string _filepath;

        public string Filepath
        {
            get { return _filepath; }
            set { _filepath = value; AskTitle.Text = System.IO.Path.GetFileName(value); }
        }

        public EventHandler<TypeSelectedEventArgs> TypeSelected = null;

        public AskXnbWindow(string root = "", string path = "")
        {
            InitializeComponent();
            Filepath = path;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Type type = null;

            var listView = (ListBox)sender;
            var item = (ListBoxItem)listView.SelectedItem;

            switch (item.Content.ToString())
            {
                case "Effect": type = typeof(Effect); break;
                case "Model": type = typeof(Model); break;
                case "SpriteFont": type = typeof(SpriteFont); break;
                case "SoundEffect": type = typeof(SoundEffect); break;
                case "Song": type = typeof(Song); break;
                case "Texture2D": type = typeof(Texture2D); break;
            }

            if (TypeSelected != null && type != null)
                TypeSelected(this, new TypeSelectedEventArgs(type, Filepath));

            Close();
        }
    }
}
