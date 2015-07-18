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

        public TypeSelectedEventArgs(Type type)
            : base()
        {
            Type = type;
        }
    }

    public partial class AskXnbWindow : Window
    {
        public EventHandler<TypeSelectedEventArgs> TypeSelected = null;

        public AskXnbWindow(string assetName)
        {
            InitializeComponent();
            AskTitle.Text = assetName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Type type = null;

            if (IsChecked(AskEffect))
                type = typeof(Effect);
            else if (IsChecked(AskModel))
                type = typeof(Model);
            else if (IsChecked(AskSpriteFont))
                type = typeof(SpriteFont);
            else if (IsChecked(AskSoundEffect))
                type = typeof(SoundEffect);
            else if (IsChecked(AskSong))
                type = typeof(Song);
            else if (IsChecked(AskTexture2D))
                type = typeof(Texture2D);
            else
                MessageBox.Show("You must select a type", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            if (TypeSelected != null)
                TypeSelected(this, new TypeSelectedEventArgs(type));

            Close();
        }

        private bool IsChecked(RadioButton radio)
        {
            if (radio.IsChecked.HasValue)
                return radio.IsChecked.Value;

            return false;
        }
    }
}
