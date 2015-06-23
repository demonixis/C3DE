using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;

namespace C3DE.Editor
{
    public class ObjectSerializer : Behaviour
    {
        public struct SerializedData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public float[][] Transform { get; set; }
            public string Geometry { get; set; }
            public float[] GeometrySize { get; set; }
        }

        public void SetGeometry(string name)
        {
            try
            {
                var meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                    meshRenderer = sceneObject.AddComponent<MeshRenderer>();

                var type = Type.GetType(name);
                var geometry = Activator.CreateInstance(type) as Geometry;

                meshRenderer.Geometry = geometry;
                meshRenderer.Geometry.Generate();
            }
            catch (Exception ex)
            {
                Debug.Log("Can't create the geometry", ex.Message);
            }
        }

        public SerializedData Serialize()
        {
            var data = new SerializedData();
            data.Name = sceneObject.Name;
            data.Id = sceneObject.Id;

            data.Transform = new float[3][];
            
            data.Transform[0] = ToFloatArray(transform.Position);
            data.Transform[1] = ToFloatArray(transform.Rotation);
            data.Transform[2] = ToFloatArray(transform.LocalScale);

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.Geometry != null)
            {
                data.Geometry = meshRenderer.Geometry.ToString();
                data.GeometrySize = ToFloatArray(meshRenderer.Geometry.Size);
            }

            return data;
        }

        private float[] ToFloatArray(Vector3 vector)
        {
            return new float[3] { vector.X, vector.Y, vector.Z };
        }
    }
}
