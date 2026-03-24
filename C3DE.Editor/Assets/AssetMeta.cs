using Newtonsoft.Json.Linq;

namespace C3DE.Editor.Assets
{
    public enum AssetType
    {
        Unknown = 0,
        Texture,
        Heightmap,
        Model,
        Scene,
        Material,
        Shader,
        Script
    }

    public sealed class AssetMeta
    {
        public string Guid { get; set; }

        public AssetType AssetType { get; set; }

        public string SourceRelativePath { get; set; }

        public JObject ImportSettings { get; set; } = new JObject();
    }
}
