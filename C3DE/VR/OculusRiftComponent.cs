using Microsoft.Xna.Framework;

namespace C3DE.VR
{
    public class OculusRiftComponent : GameComponent
    {
        public OculusRift rift;

        public OculusRiftComponent(Game game) 
            : base(game)
        {
            rift = new OculusRift();
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            base.Initialize();
            rift.Init(Game.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            rift.TrackHead();
            base.Update(gameTime);
        }
    }
}
