using C3DE.Components;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class ReflectionProbe : Component
    {
        public enum RenderingMode
        {
            Backed = 0, Realtime
        }

        private TextureCube m_ReflectionTexture;
        private Camera[] m_Cameras;
        private float m_FieldOfView = MathHelper.PiOver4;
        private float m_NearClip = 1.0f;
        private float m_FarClip = 50.0f;
        private int m_Size = 64;
        private Color[] m_ColorBuffer;
        private RenderingMode m_RenderingMode = RenderingMode.Backed;

        public RenderingMode Mode
        {
            get => m_RenderingMode;
            set
            {
                m_RenderingMode = value;
                Dirty = true;
            }
        }

        public float FieldOfView
        {
            get => m_FieldOfView;
            set
            {
                m_FieldOfView = value;
                UpdateMatrix();
            }
        }

        public float NearClip
        {
            get => m_NearClip;
            set
            {
                m_NearClip = value;
                UpdateMatrix();
            }
        }

        public float FarClip
        {
            get => m_FarClip;
            set
            {
                m_FarClip = value;
                UpdateMatrix();
            }
        }

        public int Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                UpdateRenderTargets();
            }
        }

        public TextureCube ReflectionTexture => m_ReflectionTexture;

        public bool Dirty { get; set; } = true;

        public override void Start()
        {
            base.Start();

            m_Cameras = new Camera[6];

            GameObject go = null;
            for (var i = 0; i < 6; i++)
            {
                go = new GameObject($"ReflectionProbe_{i}_{GameObject.Id}");
                go.IsStatic = GameObject.IsStatic;
                go.Transform.Parent = Transform;
                go.Transform.LocalPosition = m_Transform.LocalPosition;
                go.Transform.LocalRotation = GetFacingVector(ref i) * 90.0f;
                m_Cameras[i] = go.AddComponent<Camera>();
            }

            UpdateMatrix();
            UpdateRenderTargets();
        }

        private void UpdateMatrix()
        {
            for (var i = 0; i < 6; i++)
            {
                m_Cameras[i].m_ClearColor = Color.Transparent;
                m_Cameras[i].Near = NearClip;
                m_Cameras[i].Far = FarClip;
                m_Cameras[i].AspectRatio = 1.0f;
                m_Cameras[i].FieldOfView = FieldOfView;
                m_Cameras[i].Setup(m_Transform.Position, GetFacingVector(ref i), Vector3.Up);
                m_Cameras[i].Update();
            }

            Dirty = true;
        }

        private void UpdateRenderTargets()
        {
            for (var i = 0; i < 6; i++)
            {
                m_Cameras[i].RenderTarget = new RenderTarget2D(Application.GraphicsDevice, m_Size, m_Size);

                if (i == 0)
                {
                    m_ColorBuffer = new Color[m_Size * m_Size];
                    m_Cameras[i].RenderTarget.GetData<Color>(m_ColorBuffer);
                }
            }

            m_ReflectionTexture = new TextureCube(Application.GraphicsDevice, m_Size, false, SurfaceFormat.Color);

            Dirty = true;
        }

        public Vector3 GetFacingVector(ref int index)
        {
            // PosX/NegX/PosY/NegY/PosZ/NegZ to fit the CubeMapFace enum
            if (index == 0)
                return Vector3.Left;
            else if (index == 1)
                return Vector3.Right;
            else if (index == 2)
                return Vector3.Up;
            else if (index == 3)
                return Vector3.Down;
            else if (index == 4)
                return Vector3.Backward;
            else
                return Vector3.Forward;
        }

        public void Draw(BaseRenderer renderer)
        {
            if (!Dirty)
                return;

            for (var i = 0; i < 6; i++)
            {
                renderer.RenderReflectionProbe(m_Cameras[i]);
                
                m_Cameras[i].RenderTarget.GetData<Color>(m_ColorBuffer);
                m_ReflectionTexture.SetData<Color>((CubeMapFace)i, m_ColorBuffer);
            }

            Dirty = Mode != RenderingMode.Backed;
        }

        public RenderTarget2D GetRenderTarget(CubeMapFace face) => m_Cameras[(int)face].RenderTarget;
    }
}
