using System;
using System.IO;
using Newtonsoft.Json;

namespace C3DE.Editor.ProjectSystem
{
    public static class ProjectService
    {
        public const string ProjectFileName = "project.json";

        public static ProjectData CreateProject(string selectedPath)
        {
            var root = NormalizeProjectRoot(selectedPath);
            Directory.CreateDirectory(root);

            var data = new ProjectData
            {
                Name = Path.GetFileNameWithoutExtension(root)
            };

            Directory.CreateDirectory(Path.Combine(root, data.AssetRoot));
            Directory.CreateDirectory(Path.Combine(root, data.SceneRoot));
            Directory.CreateDirectory(Path.Combine(root, data.SettingsRoot));
            Directory.CreateDirectory(Path.Combine(root, data.LibraryRoot));

            SaveProject(data, root);
            return data;
        }

        public static ProjectData OpenProject(string path)
        {
            var root = NormalizeOpenPath(path);
            var projectFile = Path.Combine(root, ProjectFileName);

            if (!File.Exists(projectFile))
                return null;

            var json = File.ReadAllText(projectFile);
            return JsonConvert.DeserializeObject<ProjectData>(json);
        }

        public static void SaveProject(ProjectData project, string root)
        {
            var projectFile = Path.Combine(root, ProjectFileName);
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(projectFile, json);
        }

        public static string NormalizeProjectRoot(string selectedPath)
        {
            if (string.IsNullOrWhiteSpace(selectedPath))
                return GetDefaultProjectRoot();

            if (selectedPath.EndsWith(".c3de", StringComparison.OrdinalIgnoreCase))
                return selectedPath;

            return selectedPath + ".c3de";
        }

        public static string NormalizeOpenPath(string path)
        {
            if (File.Exists(path))
                return Path.GetDirectoryName(path);

            return path;
        }

        public static string GetDefaultProjectRoot()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(docs, "C3DEProjects", "Sandbox.c3de");
        }
    }
}
