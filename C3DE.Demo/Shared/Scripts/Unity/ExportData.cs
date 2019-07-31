using System;

namespace Demonixis.UnityJSONSceneExporter
{
    public enum ColliderType
    {
        Box = 0, Sphere, Capsule, Mesh
    }

    [Serializable]
    public class UTransform
    {
        public string Parent;
        public float[] LocalPosition;
        public float[] LocalRotation;
        public float[] LocalScale;
    }

    [Serializable]
    public class UMeshRenderer
    {
        public bool Enabled;
        public string Name;
        public UMeshFilter[] MeshFilters;
        public UMaterial[] Materials;
    }

    [Serializable]
    public class UMeshFilter
    {
        public float[] Positions;
        public float[] Normals;
        public float[] UVs;
        public int[] Indices;
        public int MeshFormat;
    }

    [Serializable]
    public class UMaterial
    {
        public float[] Scale;
        public float[] Offset;
        public string MainTexture;
        public string ShaderName;
    }

    [Serializable]
    public class UCollider
    {
        public bool Enabled;
        public float[] Min;
        public float[] Max;
        public float Radius;
        public int Type;
    }

    [Serializable]
    public class ULight
    {
        public bool Enabled;
        public float Radius;
        public float Intensity;
        public float Type;
        public float Angle;
        public float[] Color;
        public bool ShadowsEnabled;
    }

    [Serializable]
    public class UReflectionProbe
    {
        public bool Enabled;
        public bool IsBacked;
        public float Intensity;
        public float[] BoxSize;
        public float[] BoxMin;
        public float[] BoxMax;
        public int Resolution;
        public float[] ClipPlanes;
    }

    [Serializable]
    public class UGameObject
    {
        public string Id;
        public string Name;
        public bool IsStatic;
        public bool IsActive;
        public UTransform Transform;
        public UMeshRenderer Renderer;
        public UCollider Collider;
        public ULight Light;
        public UReflectionProbe ReflectionProbe;
    }
}
