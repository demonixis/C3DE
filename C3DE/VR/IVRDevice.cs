using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.VR
{
    public interface IVRDevice
    {
        RenderTarget2D CreateRenderTargetForEye(int eye);
        Matrix GetProjectionMatrix(int eye);
        Matrix GetViewMatrix(int eye, Matrix playerScale);
        int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT);
		float GetRenderTargetAspectRatio(int eye);
    }
}
