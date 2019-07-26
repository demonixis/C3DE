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

        internal TextureCube _reflectionTexture;
        internal Camera[] _cameras;
        private float _fov = MathHelper.ToRadians(75);
        private float _nearClip = 1.0f;
        private float _farClip = 500.0f;
        private int _size = 64;
        private RenderingMode _renderingMode = RenderingMode.Backed;

        public RenderingMode Mode
        {
            get => _renderingMode;
            set
            {
                _renderingMode = value;
                Dirty = true;
            }
        }

        public float FieldOfView
        {
            get => _fov;
            set
            {
                _fov = value;
                UpdateMatrix();
            }
        }

        public float NearClip
        {
            get => _nearClip;
            set
            {
                _nearClip = value;
                UpdateMatrix();
            }
        }

        public float FarClip
        {
            get => _farClip;
            set
            {
                _farClip = value;
                UpdateMatrix();
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                UpdateRenderTargets();
            }
        }

        public TextureCube ReflectionMap => _reflectionTexture;

        public bool Dirty { get; set; } = true;

        public override void Start()
        {
            base.Start();

            _cameras = new Camera[6];

            GameObject go = null;
            for (var i = 0; i < 6; i++)
            {
                go = new GameObject($"ReflectionProbe_{i}_{GameObject.Id}");
                go.IsStatic = GameObject.IsStatic;
                go.Transform.Parent = Transform;
                go.Transform.LocalPosition = _transform.LocalPosition;
                go.Transform.LocalRotation = GetFacingVector(ref i) * 90.0f;
                _cameras[i] = go.AddComponent<Camera>();
            }

            UpdateMatrix();
            UpdateRenderTargets();
        }

        private void UpdateMatrix()
        {
            for (var i = 0; i < 6; i++)
            {
                _cameras[i]._clearColor = Color.Transparent;
                _cameras[i].Near = NearClip;
                _cameras[i].Far = FarClip;
                _cameras[i].AspectRatio = 1.0f;
                _cameras[i].FieldOfView = FieldOfView;
                _cameras[i].Setup(_transform.Position, Vector3.Forward, Vector3.Up);
                _cameras[i].Update();
            }

            Dirty = true;
        }

        private void UpdateRenderTargets()
        {
            for (var i = 0; i < 6; i++)
                _cameras[i].RenderTarget = new RenderTarget2D(Application.GraphicsDevice, _size, _size);                    

            _reflectionTexture = new TextureCube(Application.GraphicsDevice, _size, false, SurfaceFormat.Color);

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
                return Vector3.Forward;
            else
                return Vector3.Backward;
        }

        public RenderTarget2D GetRenderTarget(CubeMapFace face) => _cameras[(int)face].RenderTarget;
    }
}
