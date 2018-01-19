using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    /// <summary>
    /// A component responible to manage transform operations (translation, rotation and scaling).
    /// </summary>
    [DataContract]
    public class Transform : Component
    {
        internal Matrix m_WorldMatrix;
        private Vector3 m_LocalRotation;
        private Vector3 m_LocalPosition;
        private Vector3 m_LocalScale;
        private Transform m_Parent;
        private Transform m_Root;
        private bool m_Dirty;
        protected List<Transform> m_Transforms;

        public Transform Root
        {
            get { return m_Root; }
            internal set { m_Root = value; }
        }

        public Transform Parent
        {
            get { return m_Parent; }
            set
            {
                if (m_Parent != null)
                    m_Parent.Transforms.Remove(this);

                m_Parent = value;

                if (m_Parent != null)
                    m_Parent.Transforms.Add(this);
            }
        }

        public List<Transform> Transforms
        {
            get { return m_Transforms; }
            protected set { m_Transforms = value; }
        }

        public Vector3 Position
        {
            get
            {
                UpdateWorldMatrix();
                return m_WorldMatrix.Translation;
            }
            set
            {
                UpdateWorldMatrix();
                m_LocalPosition = Vector3.Transform(value, Matrix.Invert(m_WorldMatrix));
            }
        }

        [DataMember]
        public Vector3 LocalPosition
        {
            get { return m_LocalPosition; }
            set { m_LocalPosition = value; }
        }

        public Vector3 Rotation
        {
            get
            {
                UpdateWorldMatrix();
                var rotation = Quaternion.Identity;
                m_WorldMatrix.ExtractRotation(ref rotation);
                return rotation.ToEuler();
            }
            set
            {
                var result = Quaternion.Euler(value) * Quaternion.Inverse(m_Parent.Quaternion);
                m_LocalRotation = result.ToEuler();
            }
        }

        public Matrix RotationMatrix => Matrix.CreateFromYawPitchRoll(m_LocalRotation.Y, m_LocalRotation.X, m_LocalRotation.Z);

        public Quaternion Quaternion
        {
            get
            {
                var rotation = Rotation;
                return Quaternion.Euler(rotation);
            }
        }

        public Quaternion LocalQuaternion => Quaternion.Euler(m_LocalRotation);

        [DataMember]
        public Vector3 LocalRotation
        {
            get { return m_LocalRotation; }
            set { m_LocalRotation = value; }
        }

        [DataMember]
        public Vector3 LocalScale
        {
            get { return m_LocalScale; }
            set { m_LocalScale = value; }
        }

        public Matrix WorldMatrix => m_WorldMatrix;

        public Vector3 Forward => m_WorldMatrix.Forward;
        public Vector3 Backward => m_WorldMatrix.Backward;
        public Vector3 Right => m_WorldMatrix.Right;
        public Vector3 Left => m_WorldMatrix.Left;
        public Vector3 Up => Vector3.Transform(Vector3.Up, m_WorldMatrix);

        public Transform()
            : base()
        {
            m_LocalPosition = Vector3.Zero;
            m_LocalRotation = Vector3.Zero;
            m_LocalScale = Vector3.One;
            m_Parent = null;
            m_Root = null;
            m_Transforms = new List<Transform>();
            m_Dirty = false;
            m_WorldMatrix = Matrix.Identity;
        }

        public void Translate(float x, float y, float z)
        {
            m_LocalPosition.X += x;
            m_LocalPosition.Y += y;
            m_LocalPosition.Z += z;
        }

        public void Translate(ref Vector3 translation)
        {
            Translate(translation.X, translation.Y, translation.Z);
        }

        public void Translate(Vector3 translation)
        {
            Translate(ref translation);
        }

        public void Rotate(float rx, float ry, float rz)
        {
            m_LocalRotation.X += rx;
            m_LocalRotation.Y += ry;
            m_LocalRotation.Z += rz;
        }

        public void Rotate(ref Vector3 rotation)
        {
            Rotate(rotation.X, rotation.Y, rotation.Z);
        }

        public void Rotate(Vector3 rotation)
        {
            Rotate(ref rotation);
        }

        public void SetPosition(float? x, float? y, float? z)
        {
            m_LocalPosition.X = x.HasValue ? x.Value : m_LocalPosition.X;
            m_LocalPosition.Y = y.HasValue ? y.Value : m_LocalPosition.Y;
            m_LocalPosition.Z = z.HasValue ? z.Value : m_LocalPosition.Z;
        }

        public void SetPosition(Vector3 position)
        {
            m_LocalPosition.X = position.X;
            m_LocalPosition.Y = position.Y;
            m_LocalPosition.Z = position.Z;
        }

        public void SetRotation(float? x, float? y, float? z)
        {
            m_LocalRotation.X = x.HasValue ? x.Value : m_LocalRotation.X;
            m_LocalRotation.Y = y.HasValue ? y.Value : m_LocalRotation.Y;
            m_LocalRotation.Z = z.HasValue ? z.Value : m_LocalRotation.Z;
        }

        public void SetRotation(Matrix matrix)
        {
            var quaternion = Quaternion.CreateFromRotationMatrix(matrix);
            var rotation = quaternion.ToEuler();
            SetRotation(rotation.X, rotation.Y, rotation.Z);
        }

        public void SetScale(float? x, float? y, float? z)
        {
            m_LocalScale.X = x.HasValue ? x.Value : m_LocalScale.X;
            m_LocalScale.Y = y.HasValue ? y.Value : m_LocalScale.Y;
            m_LocalScale.Z = z.HasValue ? z.Value : m_LocalScale.Z;
        }

        public override void Update()
        {
            if (m_GameObject.IsStatic && !m_Dirty)
                return;

            UpdateWorldMatrix();

            m_Dirty = false;
        }

        public void UpdateWorldMatrix()
        {
            m_WorldMatrix = Matrix.Identity;
            m_WorldMatrix *= Matrix.CreateScale(m_LocalScale);
            m_WorldMatrix *= Matrix.CreateFromYawPitchRoll(m_LocalRotation.Y, m_LocalRotation.X, m_LocalRotation.Z);
            m_WorldMatrix *= Matrix.CreateTranslation(m_LocalPosition);

            if (m_Parent != null)
                m_WorldMatrix *= m_Parent.m_WorldMatrix;
        }

        public Vector3 TransformVector(Vector3 direction)
        {
            UpdateWorldMatrix();
            return Vector3.Transform(direction, m_WorldMatrix);
        }

        public override object Clone()
        {
            var tr = new Transform();
            tr.m_Parent = Parent;
            tr.m_Root = m_Root;
            tr.m_GameObject = m_GameObject;

            foreach (var t in m_Transforms)
                tr.m_Transforms.Add(t);

            tr.m_LocalPosition = m_LocalPosition;
            tr.m_LocalRotation = m_LocalRotation;
            tr.m_LocalScale = m_LocalScale;
            tr.m_Dirty = true;

            return tr;
        }
    }
}