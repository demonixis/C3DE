namespace VirtualFileSystem
{
    public class Entry
    {
        public string name;
        public Directory parent;

        public Entry(string name = "")
        {
            this.name = name;
        }

        public string GetAbsolutePath()
        {
            var absolutePath = name;
            var curParent = parent;

            while (curParent != null)
            {
                if (curParent.name != "")
                {
                    absolutePath = curParent.name + '/' + absolutePath;
                }

                curParent = curParent.parent;
            }

            return absolutePath;
        }

        public override string ToString() => name;
    }
}
