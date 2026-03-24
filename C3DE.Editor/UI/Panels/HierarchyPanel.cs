using Gwen;
using Gwen.Control;

namespace C3DE.Editor.UI.Panels
{
    public sealed class HierarchyPanel : ControlBase
    {
        private readonly TreeControl _tree;

        public event System.Action<string> GameObjectSelected;

        public HierarchyPanel(ControlBase parent) : base(parent)
        {
            Dock = Dock.Fill;
            _tree = new TreeControl(this)
            {
                Dock = Dock.Fill
            };
            _tree.Selected += (_, __) =>
            {
                var node = _tree.SelectedNode;
                if (node?.UserData is string id)
                    GameObjectSelected?.Invoke(id);
            };
        }

        public void SetScene(EditorScene scene)
        {
            _tree.RemoveAllNodes();
            if (scene == null)
                return;

            foreach (var gameObject in scene.GetRootGameObjects())
                AddNode(null, gameObject);

            _tree.ExpandAll();
        }

        public void SelectGameObject(GameObject gameObject, bool selected)
        {
            var node = gameObject == null ? null : _tree.FindNodeByUserData(gameObject.Id, true);
            if (node != null)
                node.IsSelected = selected;
        }

        private void AddNode(TreeNode parentNode, GameObject gameObject)
        {
            var node = parentNode == null
                ? _tree.AddNode(gameObject.Name, userData: gameObject.Id)
                : parentNode.AddNode(gameObject.Name, userData: gameObject.Id);

            foreach (var child in gameObject.Transform.Transforms)
            {
                if (child?.GameObject != null && child.GameObject.Tag != EditorGame.EditorTag)
                    AddNode(node, child.GameObject);
            }
        }
    }
}
