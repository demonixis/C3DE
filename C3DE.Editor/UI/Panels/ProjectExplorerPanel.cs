using System;
using System.IO;
using Gwen.Control;

namespace C3DE.Editor.UI.Panels
{
    public sealed class ProjectExplorerPanel : ControlBase
    {
        private readonly TreeControl _tree;

        public event Action<string> SceneOpenRequested;

        public ProjectExplorerPanel(ControlBase parent) : base(parent)
        {
            _tree = new TreeControl(this)
            {
                Dock = Gwen.Dock.Fill
            };
            _tree.NodeDoubleClicked += (_, __) =>
            {
                var path = _tree.SelectedNode?.UserData as string;
                if (path != null && path.EndsWith(".scene.json", StringComparison.OrdinalIgnoreCase))
                    SceneOpenRequested?.Invoke(path);
            };
        }

        public void RefreshProject(string projectPath)
        {
            _tree.RemoveAllNodes();
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return;

            var root = _tree.AddNode(Path.GetFileName(projectPath), userData: projectPath);
            Build(root, projectPath);
            _tree.ExpandAll();
        }

        private static void Build(TreeNode parentNode, string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                var name = Path.GetFileName(directory);
                if (string.Equals(name, "Library", StringComparison.OrdinalIgnoreCase))
                    continue;

                var node = parentNode.AddNode(name, userData: directory);
                Build(node, directory);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                parentNode.AddNode(Path.GetFileName(file), userData: file);
            }
        }
    }
}
