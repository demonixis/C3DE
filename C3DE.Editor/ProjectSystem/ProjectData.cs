namespace C3DE.Editor.ProjectSystem
{
    public sealed class ProjectData
    {
        public string Name { get; set; }

        public int Version { get; set; } = 1;

        public string StartupSceneGuid { get; set; }

        public string StartupProject { get; set; }

        public string AssetRoot { get; set; } = "Assets";

        public string SceneRoot { get; set; } = "Scenes";

        public string SettingsRoot { get; set; } = "Settings";

        public string LibraryRoot { get; set; } = "Library";
    }
}
