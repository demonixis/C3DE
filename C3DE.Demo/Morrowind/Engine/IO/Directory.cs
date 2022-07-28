using System.Collections.Generic;

namespace VirtualFileSystem
{
    public class Directory : Entry
    {
        private static char[] pathSeparators = new char[] { '/', '\\' };
        public List<Entry> children = new List<Entry>();
        public Directory(string name = "") : base(name) { }

        public void AddChild(Entry entry)
        {
            entry.parent = this;
            children.Add(entry);
        }

        public Directory CreateChildDirectory(string directoryName)
        {
            var childDir = new Directory(directoryName);
            AddChild(childDir);

            return childDir;
        }

        public File CreateChildFile(string fileName)
        {
            var childFile = new File(fileName);
            AddChild(childFile);

            return childFile;
        }

        /// <summary>
        /// Adds a file to a descendant directory. Creates directories if necessary.
        /// </summary>
        /// <remarks>TODO: This doesn't properly handle invalid paths.</remarks>
        public void CreateDescendantFile(string descendantFilePath)
        {
            var firstPathSeparatorIndex = descendantFilePath.IndexOfAny(pathSeparators);

            if (firstPathSeparatorIndex >= 0)
            {
                var childDirName = descendantFilePath.Substring(0, firstPathSeparatorIndex);
                var restOfDescendantFilePath = descendantFilePath.Substring(firstPathSeparatorIndex + 1);

                var childDir = FindChildDirectory(childDirName);
                if (childDir == null)
                {
                    childDir = CreateChildDirectory(childDirName);
                }

                childDir.CreateDescendantFile(restOfDescendantFilePath);
            }
            else
            {
                CreateChildFile(descendantFilePath);
            }
        }

        public Entry FindChildEntry(string entryName)
        {
            return children.Find(delegate (Entry entry)
            {
                return entry.name == entryName;
            });
        }

        public Directory FindChildDirectory(string directoryName)
        {
            var childEntry = FindChildEntry(directoryName);

            return (childEntry is Directory) ? (Directory)childEntry : null;
        }

        public File FindChildFile(string fileName)
        {
            var childEntry = FindChildEntry(fileName);

            return (childEntry is File) ? (File)childEntry : null;
        }

        public Entry[] FindDescendantEntries(string entryName)
        {
            var descendantEntries = new List<Entry>();
            FindDescendantEntries(entryName, descendantEntries);

            return descendantEntries.ToArray();
        }

        public bool ContainsChildEntry(string entryName)
        {
            return FindChildEntry(entryName) != null;
        }

        public bool ContainsChildDirectory(string directoryName)
        {
            return FindChildDirectory(directoryName) != null;
        }

        public bool ContainsChildFile(string fileName)
        {
            return FindChildFile(fileName) != null;
        }

        private void FindDescendantEntries(string entryName, List<Entry> descendantEntries)
        {
            var childEntry = FindChildEntry(entryName);
            if (childEntry != null)
            {
                descendantEntries.Add(childEntry);
            }

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                if (child is Directory)
                {
                    ((Directory)child).FindDescendantEntries(entryName, descendantEntries);
                }
            }
        }
    }
}
