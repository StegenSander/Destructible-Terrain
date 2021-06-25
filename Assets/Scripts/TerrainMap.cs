using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TerrainMap
{
    private Chunk _ParentChunk;
    private World.TerrainInformation _TerrainInfo;
    private World.TerrainGenerationInformation _TerrainGenInfo;
    public float[,,] _TerrainData;
    private Vector3 _PositionOffset;


    #region Initialisation
    public TerrainMap(Chunk chunk, Vector3 position)
    {
        _PositionOffset = position;
        _ParentChunk = chunk;
        _TerrainInfo = chunk.GetWorld.TerrainInfo;
        _TerrainGenInfo = chunk.GetWorld.TerrainGenInfo;
        _TerrainData = new float[_TerrainInfo.ChunkWidth + 1
            , _TerrainInfo.ChunkHeight + 1
            , _TerrainInfo.ChunkWidth + 1];

        PopulateTerrain();
        Debug.Log($"Terrain chunk size:{_TerrainInfo.ChunkWidth},{_TerrainInfo.ChunkHeight},{_TerrainInfo.ChunkWidth}");
    }

    public void PopulateTerrain()
    {
        for (int x = 0; x < _TerrainInfo.ChunkWidth + 1; x++)
            for (int y = 0; y < _TerrainInfo.ChunkHeight  + 1; y++)
                for (int z = 0; z < _TerrainInfo.ChunkWidth + 1; z++)
                {
                    if (_ParentChunk.GetWorld.GenerationFunction != null)
                        _TerrainData[x, y, z] 
                            = _ParentChunk.GetWorld.GenerationFunction(
                                _PositionOffset.x + x, _PositionOffset.y + y, _PositionOffset.z + z);
                    else
                        _TerrainData[x, y, z] = GenerateTerrainHeight(
                            _PositionOffset.x + x, _PositionOffset.y + y, _PositionOffset.z + z);
                }
    }

    private float GenerateTerrainHeight(float x, float y, float z) //This function should be replaceable to allow customisation of the terrain Generation
    {
        return y - (_TerrainGenInfo.TerrainHeightRange * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f) + _TerrainGenInfo.BaseTerrainHeight /*Base Terrain Height */);
    }
    #endregion

   
    public void SetTerrainValue(Vector3 worldPos, float value)
    {
        Vector3 terrainPos = _ParentChunk.GetWorld.WorldToTerrainSpace(worldPos) - _PositionOffset;
        SetTerrainValue(RoundToVector3Int(terrainPos), value);
    }
    public void SetTerrainValue(Vector3Int terrainPos, float value)
    {
        if (!IsPosInTerrain(terrainPos)) return;

        _ParentChunk.NeedsUpdate = true;

        _TerrainData[terrainPos.x, terrainPos.y, terrainPos.z] = value;
    }
    public bool IsPosInTerrain(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0
            && pos.x <= _TerrainInfo.ChunkWidth
            && pos.y <= _TerrainInfo.ChunkHeight
            && pos.z <= _TerrainInfo.ChunkWidth;
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
