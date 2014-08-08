using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.UI
{
    public class GUIManager
    {
        private ContentManager _content;
        private GUISkin _skin;
        private List<GUIElement> _items;
        private int _count;
        private bool _initialized;

        public GUIManager(ContentManager content)
        {
            _items = new List<GUIElement>();
            _content = content;
            _initialized = false;
            _skin = new GUISkin(null);  
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                _skin.LoadContent(_content);
                _initialized = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i].Enabled)
                    _items[i].Draw(spriteBatch, _skin);
            }
        }

        public void Add(GUIElement item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
                _count++;

                if (_initialized)
                    item.LoadContent(_content);
            }
        }

        public void Remove(GUIElement item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
                _count--;
            }
        }
    }
}
