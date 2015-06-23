using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;

namespace C3DE.Editor.Editor
{
    public class ObjectSerializer : Component
    {
        private MeshRenderer _meshRenderer;

        public string SceneObjectId
        {
            get { return sceneObject.Id; }
            set { sceneObject.Id = value; }
        }

        public string SceneObjectName
        {
            get { return sceneObject.Name; }
            set { sceneObject.Name = value; }
        }

        public float X
        {
            get { return transform.Position.X; }
            set { transform.SetPosition(value, null, null); }
        }

        public float Y
        {
            get { return transform.Position.Y; }
            set { transform.SetPosition(null, value, null); }
        }
        
        public float Z
        {
            get { return transform.Position.Z; }
            set { transform.SetPosition(null, null, value); }
        }
        
        public float RX
        {
            get { return transform.Rotation.X; }
            set { transform.SetRotation(value, null, null); }
        }
        
        public float RY
        {
            get { return transform.Rotation.Y; }
            set { transform.SetRotation(null, value, null); }
        }
        
        public float RZ
        {
            get { return transform.Rotation.Z; }
            set { transform.SetRotation(null, null, value); }
        }
        
        public float SX
        {
            get { return transform.LocalScale.X; }
            set { transform.SetScale(value, null, null); }
        }
        
        public float SY
        {
            get { return transform.LocalScale.Y; }
            set { transform.SetScale(null, value, null); }
        }
        
        public float SZ
        {
            get { return transform.LocalScale.Z; }
            set { transform.SetScale(null, null, value); }
        }

        public string GeometryName
        {
            get
            {
                if (_meshRenderer == null ||_meshRenderer.Geometry == null)
                    return string.Empty;

                return _meshRenderer.Geometry.ToString();
            }
            set
            {
                if (_meshRenderer != null && _meshRenderer.Geometry != null)
                    SetGeometry(value);
            }
        }

        public Vector3 GeometrySize
        {
            get
            {
                if (_meshRenderer == null || _meshRenderer.Geometry == null)
                    return Vector3.One;

                return _meshRenderer.Geometry.Size;
            }
            set
            {
                if (_meshRenderer != null && _meshRenderer.Geometry != null)
                    _meshRenderer.Geometry.Size = value;
            }
        }

        public override void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetGeometry(string name)
        {
            try
            {
                var type = Type.GetType(name);
                var geometry = Activator.CreateInstance(type) as Geometry;
                _meshRenderer.Geometry = geometry;
                _meshRenderer.Geometry.Generate();
            }
            catch (Exception ex)
            {
                Debug.Log("Can't create the geometry", ex.Message);
            }
        }

        public string GetSerializedData()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void DeserializeData(string data)
        {
            JsonConvert.PopulateObject(data, this);
        }
    }
}
