using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMap
{
    World _ParentWorld;
    World.TerrainInformation _TerrainInfo;
    World.TerrainGenerationInformation _TerrainGenInfo;
    private float[,,] _TerrainData;

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

    public float SampleTerrain(Vector3Int p)
    {
        return _TerrainData[p.x, p.y, p.z];
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

    static public Vector3Int RoundToVector3Int(Vector3 vec)
    {
        return new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));
    }
}
