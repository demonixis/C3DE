using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Controllers.Mobile
{
	public class SwipeZone : Behaviour
	{
		private Texture2D _borderTexture;
		private Rectangle _borderZone;

		public Texture2D BorderTexture
		{
			get { return _borderTexture; }
			set { _borderTexture = value; }
		}

		public Rectangle Zone 
		{ 
			get { return _borderZone; }
			set { _borderZone = value; }
		}

		public bool ShowBordersLimit { get;	set; }

		public Vector2 Delta
		{
			get 
			{ 
				for (int i = 0, l = Input.Touch.MaxFingerPoints; i < l; i++) 
				{
					if (Zone.Contains (Input.Touch.GetPosition (i)))
						return Input.Touch.Delta (i);
				}

				return Vector2.Zero;
			}
		}

		public SwipeZone ()
		{
			_borderZone = Rectangle.Empty;
			ShowBordersLimit = true;
		}

		public override void Start()
		{
			if (_borderZone == Rectangle.Empty)
				_borderZone = new Rectangle (Screen.WidthPerTwo, 150, Screen.WidthPerTwo - 50, Screen.Height - Screen.Height / 3);

			if (_borderTexture == null)
				_borderTexture = GraphicsHelper.CreateBorderTexture (new Color(0.3f, 0.3f, 0.3f, 0.6f), Color.Transparent, _borderZone.Width, _borderZone.Height, 2);
		}

		public override void OnGUI(GUI ui)
		{
			if (ShowBordersLimit)
				ui.DrawTexture (ref _borderZone, _borderTexture);
		}
	}
}

