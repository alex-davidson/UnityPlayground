using System;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Random = UnityEngine.Random;

public class InitialSpawnBehaviour : MonoBehaviour {
    
    /// <summary>
    /// Prefab of which to spawn instances.
    /// </summary>
    public GameObject Prefab;
    /// <summary>
    /// Number of instances to spawn at game start.
    /// </summary>
    public int NumberOfSpawns;
    /// <summary>
    /// Spawning will occur in a rectangular area in the middle of the terrain.
    /// This value is the fraction of the total terrain width and length which
    /// will be spawned upon.
    /// </summary>
    public float AreaFraction;
    
	void Start()
    {
        if(AreaFraction <= 0) return;
        
        var terrain = Terrain.activeTerrain;
        var terrainData = terrain.terrainData;
        
        var spawnRegion = DetermineSpawnRegion(terrainData.size, Math.Min(AreaFraction, 1));
        var prefabRadius = Utils.MeasurePrefabRadius(Prefab);
        for(var i = 0; i < NumberOfSpawns; i++)
        {
            var position = GetRandomPosition(spawnRegion);
            var terrainRelativePosition = AdjustSpawnPositionForTerrainShape(terrainData, position, prefabRadius);
            Instantiate(Prefab, terrainRelativePosition + terrain.GetPosition(), Quaternion.identity);
        }
	}

    /// <summary>
    /// Given a terrain size, returns a rectangle in the XZ plane considered suitable for spawning.
    /// </summary>
    /// <param name="size">3D size of terrain volume. The height (Y axis) will be ignored.</param>
    /// <param name="clampedAreaFraction">0 &lt; value &lt;= 1</param>
    /// <returns></returns>
    private static Rect DetermineSpawnRegion(Vector3 size, float clampedAreaFraction)
    {
        var spawnOffset = size * (1 - clampedAreaFraction) / 2;
        var spawnSize = size * clampedAreaFraction;
        return new Rect(spawnOffset.x, spawnOffset.z, spawnSize.x, spawnSize.z);
    }

    /// <summary>
    /// </summary>
    /// <param name="terrainData"></param>
    /// <param name="position">Location in the XZ plane (not the XY plane!)</param>
    /// <param name="sphereRadius"></param>
    /// <returns></returns>
    private static Vector3 AdjustSpawnPositionForTerrainShape(TerrainData terrainData, Vector2 position, float sphereRadius)
    {
        var height = terrainData.GetInterpolatedHeight(position.x, position.y);
        var normal = terrainData.GetInterpolatedNormal(position.x, position.y);

        var offsetAlongNormal = normal * sphereRadius;
        var positionOnTerrain = new Vector3(position.x, height, position.y);
        return positionOnTerrain + offsetAlongNormal;
    }

    /// <summary>
    /// Given a rectangle, return a random location in it.
    /// </summary>
    /// <param name="region"></param>
    /// <returns></returns>
    private static Vector2 GetRandomPosition(Rect region)
    {
        var x = Random.Range(region.xMin, region.xMax);
        var y = Random.Range(region.yMin, region.yMax);
        return new Vector2(x, y);
    }
}
