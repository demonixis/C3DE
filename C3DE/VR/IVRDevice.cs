using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.VR
{
    public interface IVRDevice
    {
        SpriteEffects PreviewRenderEffect { get; }

        RenderTarget2D CreateRenderTargetForEye(int eye);
        Matrix GetProjectionMatrix(int eye);
        Matrix GetViewMatrix(int eye, Matrix playerScale);
        int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT);
		float GetRenderTargetAspectRatio(int eye);
        
        //Vector3 GetHeadPosition(int eye);
        //Quaternion GetHeadRotation(int eye);
        //Vector3 GetControllerPosition(bool left);
        //Quaternion GetControllerRotation(bool left);
    }
}
