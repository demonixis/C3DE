using C3DE;
using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class GameObjectUtils
{
    public static GameObject Create(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.Transform.parent = parent;
        go.Transform.localPosition = Vector3.Zero;
        go.Transform.localRotation = Quaternion.Identity;
        return go;
    }

    public static Light CreateSunLight(Vector3 position, Vector3 orientation)
    {
        var light = new GameObject("Directional Light");

        var lightComponent = light.AddComponent<Light>();
        lightComponent.Type = LightType.Directional;

        light.Transform.position = position;
        light.Transform.LocalRotation = orientation;

        return lightComponent;
    }


    public static GameObject CreateTerrain(float[,] heightPercents, float maxHeight, float heightSampleDistance, TerrainLayer[] splatPrototypes, float[,,] alphaMap, Vector3 position)
    {
        // Create the TerrainData.
        var terrainData = new TerrainData();
        terrainData.heightmapResolution = heightPercents.GetLength(0);

        var terrainWidth = (terrainData.heightmapResolution - 1) * heightSampleDistance;
        terrainData.size = new Vector3(terrainWidth, maxHeight > 0 ? maxHeight : 1, terrainWidth);
        terrainData.SetHeights(0, 0, heightPercents);

        if ((splatPrototypes != null) && (alphaMap != null))
        {
            terrainData.alphamapResolution = alphaMap.GetLength(0);
            terrainData.terrainLayers = splatPrototypes;
            terrainData.SetAlphamaps(0, 0, alphaMap);
        }


        // Create the terrain game object.
        var terrain = GameObjectFactory.CreateTerrain();
        terrain.Transform.localPosition = position;

        var size = terrainData.size;
        size.X = 2;
        size.Z = 2;

        var mesh = terrain.Geometry;
        mesh.Size = size;
        mesh.OriginAtCenter = false;
        mesh.ReverseHeightmap = true;
        mesh.HeightmapSize = terrainData.heightmapResolution;
        mesh.Data = terrainData.heightmapTexture;
        mesh.Build();

        terrain.SetWeightData(0.1f, 0.7f, 0.9f, 1f);
        var weightMap = terrain.GenerateWeightMap();

        /*var material = new StandardTerrainMaterial();
        material.WeightMap = weightMap;

        if (terrainData.terrainLayers.Length > 0)
            material.SandMap = terrainData.terrainLayers[0].diffuseTexture;
        if (terrainData.terrainLayers.Length > 1)
            material.MainTexture = terrainData.terrainLayers[1].diffuseTexture;
        if (terrainData.terrainLayers.Length > 2)
            material.RockMap = terrainData.terrainLayers[2].diffuseTexture;
        if (terrainData.terrainLayers.Length > 3)
            material.SnowMap = terrainData.terrainLayers[3].diffuseTexture;*/

        var material = new StandardMaterial();
        material.MainTexture = terrainData.terrainLayers[0].diffuseTexture;
        material.Tiling = new Vector2(6);

        terrain.Renderer.Material = material;

        return terrain.GameObject;
    }

    /// <summary>
    /// Calculate the AABB of an object and it's descendants.
    /// </summary>
    public static Rectangle CalcVisualBoundsRecursive(GameObject gameObject)
    {
        return Rectangle.Empty;
    }

    /// <summary>
    /// Finds a descendant game object by name.
    /// </summary>
    public static GameObject FindChildRecursively(GameObject parent, string name)
    {
        return null;
    }

    /// <summary>
    /// Finds a descendant game object with a name containing nameSubstring.
    /// </summary>
    public static GameObject FindChildWithNameSubstringRecursively(GameObject parent, string nameSubstring)
    {
        return null;
    }

    /// <summary>
    /// Find an ancestor object, or the object itself, with a tag.
    /// </summary>
    public static GameObject FindObjectWithTagUpHeirarchy(GameObject gameObject, string tag)
    {
        return null;
    }

    public static GameObject FindTopLevelObject(GameObject baseObject)
    {
        return null;
    }

    /// <summary>
    /// Set the layer of an object and all of it's descendants.
    /// </summary>
    public static void SetLayerRecursively(GameObject gameObject, int layer)
    {
    }

    /// <summary>
    /// Adds mesh colliders to every descandant object with a mesh filter but no mesh collider, including the object itself.
    /// </summary>
    public static void AddMissingMeshCollidersRecursively(GameObject gameObject, bool isStatic = true)
    {
    }
}