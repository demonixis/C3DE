using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace C3DE.Editor
{
    public class AskXnbWindow
    {
        public string Filepath { get; set; }

        public void Show() { }

        public event Action<string, Type> TypeSelected = null;
    }

    public class AssetImporter
    {
        public static string ActiveRootFolder { get; set; }

        public static readonly Dictionary<string, string> Folders = new Dictionary<string, string>
        {
            { "Sound", "Audio/Sounds" },
            { "Music", "Audio/Musics" },
            { "Effect", "Effects" },
            { "Font", "Fonts" },
            { "Model", "Models" },
            { "Script", "Scripts" },
            { "Texture", "Textures" }
        };

        private static AskXnbWindow askXnbWindow = null;

        public static void CreateFolderStructure(string rootFolder)
        {
            if (!Directory.Exists(rootFolder))
                Directory.CreateDirectory(rootFolder);

            foreach (var folder in Folders)
            {
                if (!Directory.Exists(Path.Combine(rootFolder, folder.Value)))
                    Directory.CreateDirectory(Path.Combine(rootFolder, folder.Value));
            }
        }

        public static void ImportFiles(string rootFolder, string[] files)
        {
            if (askXnbWindow == null)
            {
                askXnbWindow = new AskXnbWindow();
                askXnbWindow.TypeSelected += OnTypeSelected;
            }

            ActiveRootFolder = rootFolder;

            foreach (var file in files)
            {
                if (file.Contains("xnb"))
                {
                    askXnbWindow.Filepath = file;
                    askXnbWindow.Show();
                }
            }
        }

        private static void OnTypeSelected(string path, Type type)
        {
            var filename = Path.GetFileName(path);
            var assetKey = string.Empty;

            if (type == typeof(Effect))
                assetKey = Folders["Effect"];

            else if (type == typeof(SpriteFont))
                assetKey = Folders["Font"];

            else if (type == typeof(Model))
                assetKey = Folders["Model"];

            else if (type == typeof(SoundEffect))
                assetKey = Folders["Sound"];

            else if (type == typeof(Song))
                assetKey = Folders["Music"];

            else if (type == typeof(Texture2D))
                assetKey = Folders["Texture"];

            if (!string.IsNullOrEmpty(assetKey))
                File.Copy(path, Path.Combine(ActiveRootFolder, Folders[assetKey], filename), true);
        }
    }
}