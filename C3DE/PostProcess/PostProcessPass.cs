using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.PostProcess
{
    public abstract class PostProcessPass : IComparable
    {
        protected Effect effect;
        protected int order;

        public abstract void Initialize(ContentManager content);
        public abstract void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget);

        public int CompareTo(object obj)
        {
            var pass = obj as PostProcessPass;

            if (pass == null)
                return 1;

            if (order == pass.order)
                return 0;
            else if (order > pass.order)
                return 1;
            else
                return -1;
        }
    }
}
