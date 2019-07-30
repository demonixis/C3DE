using System;

namespace Demonixis.UnityJSONSceneExporter
{
    public enum ColliderType
    {
        Box = 0, Sphere, Capsule, Mesh
    }

    [Serializable]
    public struct UMeshRenderer
    {
        public bool Enabled;
        public string Name;
        public UMeshFilter MeshFilter;
        public UMaterial[] Materials;
    }

    [Serializable]
    public struct UMeshFilter
    {
        public float[] Positions;
        public float[] Normals;
        public float[] UVs;
        public int[][] Indices;
        public int[][] Triangles;
        public uint[] VertexStart;
        public uint[] IndexStart;
        public int SubMeshCount;
        public int MeshFormat;
    }

    [Serializable]
    public struct UMaterial
    {
        public float[] Scale;
        public float[] Offset;
        public string MainTexture;
        public string ShaderName;
    }

    [Serializable]
    public struct UCollider
    {
        public bool Enabled;
        public float[] Min;
        public float[] Max;
        public float Radius;
        public int Type;
    }

    [Serializable]
    public struct ULight
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
    public struct UReflectionProbe
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
    public struct UGameObject
    {
        public int ID;
        public string Name;
        public int Parent;
        public bool IsStatic;
        public bool IsActive;
        public float[] LocalPosition;
        public float[] LocalRotation;
        public float[] LocalScale;
        public UMeshRenderer Renderer;
        public UCollider Collider;
        public ULight Light;
        public UReflectionProbe ReflectionProbe;
    }
}
