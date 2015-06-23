using C3DE.Components;
using C3DE.Components.Lights;
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
            public string Type { get; set; }
        }

        public Geometry CreateGeometry(Type geometryType)
        {
            Geometry geometry = null;

            try
            {
                geometry = Activator.CreateInstance(geometryType) as Geometry;
            }
            catch (Exception ex)
            {
                Debug.Log("Can't create the geometry", ex.Message);
            }

            return geometry;
        }

        public void Deserialize(SerializedData data)
        {
            sceneObject.Id = data.Id;
            sceneObject.Name = data.Name;
            sceneObject.Transform.Position = FromFloatArray(data.Transform[0]);
            sceneObject.Transform.Rotation = FromFloatArray(data.Transform[1]);
            sceneObject.Transform.LocalScale = FromFloatArray(data.Transform[2]);
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
                data.Type = meshRenderer.Geometry.GetType().Name.Replace("Geometry", "");
                return data;
            }

            var light = GetComponent<Light>();
            if (light != null)
            {
                data.Type = light.GetType().Name.Replace("Light", "");
                return data;
            }

            var camera = GetComponent<Camera>();
            if (camera != null)
            {
                data.Type = camera.GetType().AssemblyQualifiedName;
                return data;
            }

            return data;
        }

        private float[] ToFloatArray(Vector3 vector)
        {
            return new float[3] { vector.X, vector.Y, vector.Z };
        }

        private Vector3 FromFloatArray(float[] array)
        {
            return new Vector3(array[0], array[1], array[2]);
        }
    }
}
