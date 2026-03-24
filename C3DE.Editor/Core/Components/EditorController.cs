using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Editor
{
    public sealed class EditorController : Controller
    {
        private enum CameraInteractionMode
        {
            None = 0,
            FreeLook,
            Pan,
            OrbitSelection
        }

        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        private Vector3 _translation;
        private Vector3 _rotation;
        private CameraInteractionMode _interactionMode;
        private Vector3 _orbitPivot;
        private float _orbitDistance;
        private bool _hasOrbitPivot;

        public bool ViewportInputEnabled { get; set; }

        public bool KeyboardShortcutsEnabled { get; set; }

        public bool IsInteracting => _interactionMode != CameraInteractionMode.None;

        public float OrbitSpeed { get; set; }

        public float DollySpeed { get; set; }

        public float FocusPadding { get; set; }

        public EditorController()
            : base()
        {
            MoveSpeed = 5.0f;
            RotationSpeed = 0.45f;
            LookSpeed = 0.25f;
            StrafeSpeed = 1.5f;
            MouseSensibility = new Vector2(1.0f);
            OrbitSpeed = 0.005f;
            DollySpeed = 0.00075f;
            FocusPadding = 1.6f;
            _orbitDistance = 5.0f;
        }

        public override void Update()
        {
            UpdateInputs();

            // Limits on X axis
            if (_transform.Rotation.X <= -MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(-MathHelper.PiOver2 + 0.001f, null, null);
                _rotation = Vector3.Zero;
            }
            else if (_transform.Rotation.X >= MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(MathHelper.PiOver2 - 0.001f, null, null);
                _rotation = Vector3.Zero;
            }

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);
            _transformedReference = Vector3.Transform(_translation, _rotationMatrix);

            // Translate and rotate
            _transform.Translate(ref _transformedReference);
            _transform.Rotate(ref _rotation);

            _translation *= Velocity;
            _rotation *= AngularVelocity;
        }

        protected override void UpdateInputs()
        {
            _interactionMode = CameraInteractionMode.None;

            UpdateMouseInput();
            UpdateKeyboardInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (!ViewportInputEnabled)
                return;

            if (Input.Keys.Pressed(Keys.Up))
                _translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down))
                _translation.Z += MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                _translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.Right))
                _translation.X += MoveSpeed * Time.DeltaTime / 2.0f;
        }

        protected override void UpdateMouseInput()
        {
            if (!ViewportInputEnabled)
                return;

            var delta = Input.Mouse.Delta;
            var wheel = Input.Mouse.WheelDelta;
            var horizontalWheel = Input.Mouse.HorizontalWheelDelta;
            var shift = Input.Keys.Pressed(Keys.LeftShift) || Input.Keys.Pressed(Keys.RightShift);
            var alt = Input.Keys.Pressed(Keys.LeftAlt) || Input.Keys.Pressed(Keys.RightAlt);
            var dollyFactor = shift ? 3.0f : 1.0f;

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                _interactionMode = CameraInteractionMode.FreeLook;

                if (Input.Mouse.JustClicked(Inputs.MouseButton.Right))
                {
                    if (System.Math.Abs(delta.X) > 2)
                        delta.X = 0;

                    if (System.Math.Abs(delta.Y) > 2)
                        delta.Y = 0;
                }

                _rotation.Y -= delta.X * LookSpeed * MouseSensibility.Y * Time.DeltaTime;
                _rotation.X -= delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
            {
                _interactionMode = CameraInteractionMode.Pan;
                _translation.Y += delta.Y * StrafeSpeed * MouseSensibility.Y * Time.DeltaTime;
                _translation.X -= delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (alt)
            {
                _interactionMode = CameraInteractionMode.OrbitSelection;
                ApplyOrbit(new Vector2(horizontalWheel, wheel));
            }
            else
            {
                if (horizontalWheel != 0)
                {
                    _interactionMode = CameraInteractionMode.Pan;
                    _translation.X -= horizontalWheel * DollySpeed * StrafeSpeed;
                }

                if (wheel != 0)
                    _translation.Z -= MoveSpeed * dollyFactor * DollySpeed * wheel;
            }
        }

        protected override void UpdateGamepadInput()
        {
        }

        protected override void UpdateTouchInput()
        {
        }

        public void Focus(GameObject target)
        {
            if (target == null)
                return;

            var (pivot, radius) = GetTargetBounds(target);
            _orbitPivot = pivot;
            _orbitDistance = Math.Max(radius * FocusPadding, 2.0f);
            _hasOrbitPivot = true;

            var forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f));
            if (forward == Vector3.Zero)
                forward = Vector3.Forward;

            forward.Normalize();
            _transform.Position = pivot - forward * _orbitDistance;
            _transform.UpdateWorldMatrix();
        }

        public void SetOrbitPivot(GameObject target)
        {
            if (target == null)
                return;

            var (pivot, radius) = GetTargetBounds(target);
            _orbitPivot = pivot;
            _orbitDistance = Math.Max(_orbitDistance, Math.Max(radius * FocusPadding, 2.0f));
            _hasOrbitPivot = true;
        }

        private void ApplyOrbit(Vector2 scrollDelta)
        {
            if (!_hasOrbitPivot)
            {
                _orbitPivot = _transform.Position + Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f)) * _orbitDistance;
                _hasOrbitPivot = true;
            }

            _transform.LocalRotation += new Vector3(-scrollDelta.Y * OrbitSpeed, -scrollDelta.X * OrbitSpeed, 0.0f);
            var rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);
            var forward = Vector3.Transform(Vector3.Forward, rotationMatrix);
            if (forward == Vector3.Zero)
                forward = Vector3.Forward;

            forward.Normalize();
            _transform.Position = _orbitPivot - forward * _orbitDistance;
            _transform.UpdateWorldMatrix();
        }

        private static (Vector3 pivot, float radius) GetTargetBounds(GameObject target)
        {
            var pivot = target.Transform.Position;
            var radius = 1.0f;

            var renderer = target.GetComponent<Components.Rendering.Renderer>();
            if (renderer != null)
            {
                renderer.ComputeBoundingInfos();
                var sphere = renderer.BoundingSphere;
                if (sphere.Radius > 0.0f)
                    return (sphere.Center, sphere.Radius);
            }

            var sphereCollider = target.GetComponent<Components.Physics.SphereCollider>();
            if (sphereCollider != null && sphereCollider.Sphere.Radius > 0.0f)
                return (target.Transform.Position + sphereCollider.Sphere.Center, sphereCollider.Sphere.Radius);

            var boxCollider = target.GetComponent<Components.Physics.BoxCollider>();
            if (boxCollider != null)
            {
                var extents = boxCollider.Maximum - boxCollider.Center;
                radius = Math.Max(1.0f, extents.Length());
                pivot = target.Transform.Position + boxCollider.Center;
            }

            return (pivot, radius);
        }
    }
}
