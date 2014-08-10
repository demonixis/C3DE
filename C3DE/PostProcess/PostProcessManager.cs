using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.PostProcess
{
    public class PostProcessManager
    {
        private bool _enabled;
        private List<PostProcessPass> _passes;
        private int _size;

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
            _size = 0;
        }

        public void Add(PostProcessPass pass)
        {
            _passes.Add(pass);
            _size++;
        }

        public void Remove(PostProcessPass pass)
        {
            _passes.Remove(pass);
            _size--;
        }

        public void LoadContent(ContentManager content)
        {
            if (_size > 0)
            {
                for (int i = 0; i < _size; i++)
                    _passes[i].LoadContent(content);
            }
        }

        public void Draw(SpriteBatch spriteBatch, RenderTarget2D target)
        {
            if (_size > 0)
            {
                for (int i = 0; i < _size; i++)
                    _passes[i].Apply(spriteBatch, target);
            }
        }
    }
}
