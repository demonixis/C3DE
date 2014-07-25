using System.Collections.Generic;

namespace C3DE.PostProcess
{
    public class PostProcessManager
    {
        private bool _enabled;
        private List<PostProcessPass> _passes;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public List<PostProcessPass> Passes
        {
            get { return _passes; }
        }

        public PostProcessPass this[int index]
        {
            get { return _passes[index]; }
        }

        public PostProcessManager()
        {
            _enabled = true;
            _passes = new List<PostProcessPass>();
        }

        public void Add(PostProcessPass pass)
        {
            _passes.Add(pass);
        }

        public void Remove(PostProcessPass pass)
        {
            _passes.Remove(pass);
        }
    }
}
