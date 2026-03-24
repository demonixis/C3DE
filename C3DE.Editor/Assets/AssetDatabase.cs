using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace C3DE.Editor.Assets
{
    public sealed class AssetDatabase
    {
        private readonly string _projectRoot;
        private readonly string _assetsRoot;
        private readonly Dictionary<string, AssetMeta> _byGuid = new Dictionary<string, AssetMeta>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AssetMeta> _byPath = new Dictionary<string, AssetMeta>(StringComparer.OrdinalIgnoreCase);

        public AssetDatabase(string projectRoot)
        {
            _projectRoot = projectRoot;
            _assetsRoot = Path.Combine(projectRoot, "Assets");
        }

        public IReadOnlyCollection<AssetMeta> Assets => _byGuid.Values.ToArray();

        public void Scan()
        {
            _byGuid.Clear();
            _byPath.Clear();

            if (!Directory.Exists(_assetsRoot))
                Directory.CreateDirectory(_assetsRoot);

            foreach (var file in Directory.GetFiles(_assetsRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                var relativePath = Path.GetRelativePath(_projectRoot, file).Replace('\\', '/');
                var meta = LoadOrCreateMeta(relativePath);
                _byGuid[meta.Guid] = meta;
                _byPath[relativePath] = meta;
            }
        }

        public AssetMeta FindByGuid(string guid)
        {
            if (guid == null)
                return null;

            _byGuid.TryGetValue(guid, out var meta);
            return meta;
        }

        public AssetMeta FindMetaByPath(string relativePath)
        {
            if (relativePath == null)
                return null;

            relativePath = relativePath.Replace('\\', '/');
            _byPath.TryGetValue(relativePath, out var meta);
            return meta;
        }

        public AssetMeta EnsureMetaForAsset(string relativePath)
        {
            var meta = FindMetaByPath(relativePath);
            if (meta != null)
                return meta;

            meta = CreateMeta(relativePath);
            SaveMeta(meta);
            _byGuid[meta.Guid] = meta;
            _byPath[relativePath] = meta;
            return meta;
        }

        public AssetMeta[] ImportFiles(IEnumerable<string> files)
        {
            var imported = new List<AssetMeta>();

            foreach (var file in files)
            {
                if (!File.Exists(file))
                    continue;

                var target = Path.Combine(_assetsRoot, Path.GetFileName(file));
                File.Copy(file, target, true);

                var relativePath = Path.GetRelativePath(_projectRoot, target).Replace('\\', '/');
                imported.Add(EnsureMetaForAsset(relativePath));
            }

            return imported.ToArray();
        }

        public bool RenameAsset(string relativePath, string newFileName)
        {
            var meta = FindMetaByPath(relativePath);
            if (meta == null)
                return false;

            var source = Path.Combine(_projectRoot, meta.SourceRelativePath);
            var targetRelativePath = Path.Combine(Path.GetDirectoryName(meta.SourceRelativePath) ?? string.Empty, newFileName).Replace('\\', '/');
            var target = Path.Combine(_projectRoot, targetRelativePath);

            if (!File.Exists(source))
                return false;

            File.Move(source, target);

            var sourceMeta = GetMetaFilePath(meta.SourceRelativePath);
            var targetMeta = GetMetaFilePath(targetRelativePath);
            if (File.Exists(sourceMeta))
                File.Move(sourceMeta, targetMeta);

            _byPath.Remove(meta.SourceRelativePath);
            meta.SourceRelativePath = targetRelativePath;
            SaveMeta(meta);
            _byPath[targetRelativePath] = meta;
            return true;
        }

        public bool MoveAsset(string relativePath, string targetDirectoryRelativePath)
        {
            var meta = FindMetaByPath(relativePath);
            if (meta == null)
                return false;

            var source = Path.Combine(_projectRoot, meta.SourceRelativePath);
            var targetDirectory = Path.Combine(_projectRoot, targetDirectoryRelativePath);
            Directory.CreateDirectory(targetDirectory);
            var targetRelativePath = Path.Combine(targetDirectoryRelativePath, Path.GetFileName(meta.SourceRelativePath)).Replace('\\', '/');
            var target = Path.Combine(_projectRoot, targetRelativePath);

            if (!File.Exists(source))
                return false;

            File.Move(source, target);

            var sourceMeta = GetMetaFilePath(meta.SourceRelativePath);
            var targetMeta = GetMetaFilePath(targetRelativePath);
            if (File.Exists(sourceMeta))
                File.Move(sourceMeta, targetMeta);

            _byPath.Remove(meta.SourceRelativePath);
            meta.SourceRelativePath = targetRelativePath;
            SaveMeta(meta);
            _byPath[targetRelativePath] = meta;
            return true;
        }

        private AssetMeta LoadOrCreateMeta(string relativePath)
        {
            var metaFilePath = GetMetaFilePath(relativePath);
            if (File.Exists(metaFilePath))
            {
                var json = File.ReadAllText(metaFilePath);
                var meta = JsonConvert.DeserializeObject<AssetMeta>(json);
                if (meta != null)
                    return meta;
            }

            var created = CreateMeta(relativePath);
            SaveMeta(created);
            return created;
        }

        private AssetMeta CreateMeta(string relativePath)
        {
            return new AssetMeta
            {
                Guid = Guid.NewGuid().ToString("N"),
                SourceRelativePath = relativePath,
                AssetType = ResolveType(relativePath)
            };
        }

        private void SaveMeta(AssetMeta meta)
        {
            var path = GetMetaFilePath(meta.SourceRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(meta, Formatting.Indented));
        }

        private string GetMetaFilePath(string relativePath)
        {
            return Path.Combine(_projectRoot, relativePath) + ".meta";
        }

        private static AssetType ResolveType(string relativePath)
        {
            if (relativePath.EndsWith(".scene.json", StringComparison.OrdinalIgnoreCase))
                return AssetType.Scene;

            var ext = Path.GetExtension(relativePath).ToLowerInvariant();
            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                    return relativePath.IndexOf("height", StringComparison.OrdinalIgnoreCase) >= 0
                        ? AssetType.Heightmap
                        : AssetType.Texture;
                case ".fbx":
                case ".obj":
                case ".xnb":
                    return AssetType.Model;
                case ".fx":
                case ".fxh":
                    return AssetType.Shader;
                case ".cs":
                    return AssetType.Script;
                case ".mat":
                    return AssetType.Material;
                default:
                    return AssetType.Unknown;
            }
        }
    }
}
