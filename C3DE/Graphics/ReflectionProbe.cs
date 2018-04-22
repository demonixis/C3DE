using C3DE.Components;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics
{
    public class ReflectionProbe : Component
    {
        public enum RenderingMode
        {
            Backed = 0, Realtime
        }

        private Camera[] m_Cameras;

        public RenderingMode Mode { get; set; }
        public float FieldOfView { get; set; } = MathHelper.PiOver4;
        public float NearClip { get; set; } = 1.0f;
        public float FarClip { get; set; } = 50.0f;
        public int Width { get; set; } = 64;
        public TextureCube ReflectionTexture { get; private set; }

        public bool HaveToRender { get; private set; } = true;

        public override void Start()
        {
            base.Start();

            m_Cameras = new Camera[6];

            for (var i = 0; i < 6; i++)
            {
                m_Cameras[i] = new Camera();
                m_Cameras[i].Near = NearClip;
                m_Cameras[i].Far = FarClip;
                m_Cameras[i].AspectRatio = 1.0f;
                m_Cameras[i].FieldOfView = FieldOfView;
                m_Cameras[i].Setup(m_Transform.Position, GetFacingVector(ref i), Vector3.Up);
                m_Cameras[i].RenderTarget = new RenderTarget2D(Application.GraphicsDevice, Width, Width);
            }

            ReflectionTexture = new TextureCube(Application.GraphicsDevice, Width, false, SurfaceFormat.Color);
        }

        public Vector3 GetFacingVector(ref int index)
        {
            if (index == 0)
                return Vector3.Left;
            else if (index == 1)
                return Vector3.Down;
            else if (index == 2)
                return Vector3.Backward;
            else if (index == 3)
                return Vector3.Right;
            else if (index == 4)
                return Vector3.Up;
            else
                return Vector3.Forward;
        }

        public void Draw(BaseRenderer renderer)
        {
            if (!HaveToRender)
                return;

            Color[] data = null;

            for (var i = 0; i < 6; i++)
            {
                renderer.Render(Scene.current, m_Cameras[i]);

                m_Cameras[i].RenderTarget.GetData<Color>(data);
                ReflectionTexture.SetData<Color>((CubeMapFace)i, data);
            }

            if (Mode == RenderingMode.Backed)
                HaveToRender = false;
        }
    }
}
