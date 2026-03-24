using System;
using System.Linq;
using C3DE.Editor.Assets;
using Gwen.Control;

namespace C3DE.Editor.UI.Panels
{
    public sealed class AssetBrowserPanel : ControlBase
    {
        private readonly ListBox _listBox;

        public event Action<AssetMeta> AssetSelected;

        public AssetBrowserPanel(ControlBase parent) : base(parent)
        {
            _listBox = new ListBox(this)
            {
                Dock = Gwen.Dock.Fill
            };
            _listBox.RowSelected += (_, args) =>
            {
                if ((args.SelectedItem as ListBoxRow)?.UserData is AssetMeta meta)
                    AssetSelected?.Invoke(meta);
            };
        }

        public void Refresh(AssetDatabase database)
        {
            _listBox.DeleteAllChildren();
            if (database == null)
                return;

            foreach (var meta in database.Assets.OrderBy(a => a.SourceRelativePath))
                _listBox.AddRow($"{meta.AssetType}: {meta.SourceRelativePath}", userData: meta);
        }
    }
}
