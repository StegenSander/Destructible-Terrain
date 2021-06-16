using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TerrainMap
{
    private World _ParentWorld;
    private World.TerrainInformation _TerrainInfo;
    private World.TerrainGenerationInformation _TerrainGenInfo;
    private float[,,] _TerrainData;

    public enum TerrainChange
    {
        Remove,
        Add,
    }

    #region Initialisation
    public TerrainMap(World world)
    {
        _ParentWorld = world;
        _TerrainInfo = _ParentWorld.TerrainInfo;
        _TerrainGenInfo = _ParentWorld.TerrainGenInfo;
        _TerrainData = new float[_TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks + 1
            , _TerrainInfo.ChunkHeight * _TerrainInfo.AmountOfChunks + 1
            , _TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks + 1];

        PopulateTerrain();
    }

    public void PopulateTerrain()
    {
        for (int x = 0; x < _TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks + 1; x++)
            for (int y = 0; y < _TerrainInfo.ChunkHeight * _TerrainInfo.AmountOfChunks + 1; y++)
                for (int z = 0; z < _TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks + 1; z++)
                {
                    if (_ParentWorld.GenerationFunction != null)
                        _TerrainData[x, y, z] = _ParentWorld.GenerationFunction(x, y, z);
                    else
                        _TerrainData[x, y, z] = GenerateTerrainHeight(x, y, z);
                }
    }

    private float GenerateTerrainHeight(int x, int y, int z) //This function should be replaceable to allow customisation of the terrain Generation
    {
        return y - (_TerrainGenInfo.TerrainHeightRange * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f) + _TerrainGenInfo.BaseTerrainHeight /*Base Terrain Height */);
    }
    #endregion

    #region UpdatingTerrain
    public void UpdateTerrain(Collider col,TerrainChange change)
    {
        Profiler.BeginSample("Removing Terrain");
        Bounds bounds = col.bounds;
        //Possible optimasation clip bounds to terrain space so we don't check unnecessary areas
        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;

        for (int x = Mathf.RoundToInt(boundsMin.x); x < boundsMax.x; x++)
            for (int y = Mathf.RoundToInt(boundsMin.y); y < boundsMax.y; y++)
                for (int z = Mathf.RoundToInt(boundsMin.z); z < boundsMax.z; z++)
                {
                    Vector3 worldPos = new Vector3(x, y, z);

                    if (col.ClosestPoint(worldPos) == worldPos)
                    {
                        if (TerrainChange.Remove == change)
                            SetTerrainValue(worldPos,1);
                        if (TerrainChange.Add == change)
                            SetTerrainValue(worldPos, -1);
                    }
        }
        Profiler.EndSample();
    }
    public void SetTerrainValue(Vector3 worldPos, int value)
    {
        Vector3 terrainPos = _ParentWorld.WorldToTerrainSpace(worldPos);
        SetTerrainValue(RoundToVector3Int(terrainPos), value);
    }
    public void SetTerrainValue(Vector3Int terrainPos, int value)
    {
        if (!IsPosInTerrain(terrainPos)) return;

        #region MarkUpdates
        //Regular one
        Chunk chunk = null;
        chunk = _ParentWorld.AccesChunk(_ParentWorld.TerrainToWorldSpace(terrainPos));
        if (chunk != null) chunk.NeedsUpdate = true;

        //Makes sure all chunks get updated if it's on an edge
        bool xEdge = (terrainPos.x % _TerrainInfo.ChunkWidth) == 0;
        bool zEdge = (terrainPos.z % _TerrainInfo.ChunkWidth) == 0;
        
        if (xEdge)
        {
            chunk = _ParentWorld.AccesChunk(_ParentWorld.TerrainToWorldSpace(terrainPos - new Vector3Int(1, 0, 0)));
            if (chunk != null) chunk.NeedsUpdate = true;
        }
        if (zEdge)
        {
            chunk = _ParentWorld.AccesChunk(_ParentWorld.TerrainToWorldSpace(terrainPos - new Vector3Int(0, 0, 1)));
            if (chunk != null) chunk.NeedsUpdate = true;
        }
        if (zEdge && xEdge)
        {

            chunk = _ParentWorld.AccesChunk(_ParentWorld.TerrainToWorldSpace(terrainPos - new Vector3Int(1, 0, 1)));
            if(chunk != null) chunk.NeedsUpdate = true;
        }
        #endregion

        _TerrainData[terrainPos.x, terrainPos.y, terrainPos.z] = value;
    }
    #endregion
    public bool IsPosInTerrain(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0
            && pos.x <= _TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks
            && pos.y <= _TerrainInfo.ChunkWidth
            && pos.z <= _TerrainInfo.ChunkWidth * _TerrainInfo.AmountOfChunks;
    }

    public float SampleTerrain(Vector3Int p)
    {
        return _TerrainData[p.x, p.y, p.z];
    }


    static public Vector3Int RoundToVector3Int(Vector3 vec)
    {
        return new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));
    }
}
