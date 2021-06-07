using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Serializable]
    public struct TerrainGenerationInformation //Reconsider this since we want generation function to be easy configurable
    {
        [Tooltip("Height the terrain would have if the range is set to 0")]
        public int BaseTerrainHeight;
        [Tooltip("Distance between the lowest and the heighest point of the terrain")]
        public int TerrainHeightRange;
    }

    [Serializable]
    public struct TerrainInformation
    {
        public int ChunkWidth;
        public int ChunkHeight;
        public int AmountOfChunks;
        public Material Mat;
    }


    [SerializeField] private TerrainInformation _TerrainInfo;
    public TerrainInformation TerrainInfo
    {
        get { return _TerrainInfo; }
    }
    [SerializeField] private TerrainGenerationInformation _TerrainGenInfo;
    public TerrainGenerationInformation TerrainGenInfo
    {
        get { return _TerrainGenInfo; }
    }

    public Func<int, int, int, float> GenerationFunction { get; set; } = null;

    TerrainMap _TerrainMap;
    public TerrainMap Terrain
    {
        get { return _TerrainMap; }
    }

    private Dictionary<Vector3Int, Chunk> _ChunkMap = new Dictionary<Vector3Int, Chunk>();
    public Dictionary<Vector3Int, Chunk> ChunkMap
    {
        get { return _ChunkMap; }
    }
    // Start is called before the first frame update
    void Start()
    {
        _TerrainMap = new TerrainMap(this);

        for (int row = 0; row < _TerrainInfo.AmountOfChunks; row++)
            for (int column = 0; column < _TerrainInfo.AmountOfChunks; column++)
            {
                Vector3Int chunkPos = TerrainMap.RoundToVector3Int(transform.position) + new Vector3Int(row * _TerrainInfo.ChunkWidth, 0, column * _TerrainInfo.ChunkWidth);
                _ChunkMap.Add(chunkPos
                    , new Chunk(this, row, column, chunkPos));

                _ChunkMap[chunkPos].CreateMesh();
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
