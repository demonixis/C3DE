using System;
using Microsoft.Xna.Framework;
using OSVR.RenderManager;

namespace C3DE.VR
{
	public class MonoGameGraphicsToolkit : OpenGLToolkitFunctions
	{
		public MonoGameGraphicsToolkit() { }

		protected override bool AddOpenGLContext(ref OpenGLContextParams p)
		{
			var window = Application.Engine.Window;
			window.Position = new Point(p.XPos, p.YPos);
			window.Title = p.WindowTitle;
			Screen.Setup(p.Width, p.Height, null, null);
			Screen.Fullscreen = p.FullScreen;
			return true;
		}

		protected override bool MakeCurrent(UIntPtr display) => true;
		protected override bool SetVerticalSync(bool verticalSync) => true;
		protected override bool SwapBuffers(UIntPtr display) => true;
	}
}
