using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    #region Variables
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

    public Bounds Boundaries
    {
        get {
            Vector3 worldSize = new Vector3(_TerrainInfo.AmountOfChunks * _TerrainInfo.ChunkWidth,_TerrainInfo.ChunkHeight, _TerrainInfo.AmountOfChunks * _TerrainInfo.ChunkWidth);
            Bounds bounds = new Bounds();
            bounds.SetMinMax(transform.position, transform.position + worldSize);
            return bounds;
        }
    }
    #endregion

    void Start()
    {
        _TerrainMap = new TerrainMap(this);

        for (int row = 0; row < _TerrainInfo.AmountOfChunks; row++)
            for (int column = 0; column < _TerrainInfo.AmountOfChunks; column++)
            {
                Vector3Int chunkPos = RowColumnToChunkPos(row, column);
                _ChunkMap.Add(chunkPos
                    , new Chunk(this, row, column, chunkPos));
            }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAllChunks();
    }

    public Vector3 WorldToTerrainSpace(Vector3 vec)
    {
        return vec - transform.position;
    }
    public Vector3 TerrainToWorldSpace(Vector3 vec)
    {
        return vec + transform.position;
    }

    public void UpdateAllChunks()
    {
        foreach (var PosChunk in _ChunkMap)
        {
            if (PosChunk.Value.NeedsUpdate)
                PosChunk.Value.CreateMesh();
        }
    }

    public Chunk AccesChunk(Vector3 worldPos)
    {
        if (Boundaries.Contains(worldPos))
        {
            int row = (int)(worldPos.x) / _TerrainInfo.ChunkWidth;
            int column = (int)(worldPos.z) / _TerrainInfo.ChunkWidth;
            Vector3Int chunkPos = RowColumnToChunkPos(row, column);
            if (_ChunkMap.ContainsKey(chunkPos))
                return _ChunkMap[chunkPos];
        }
        return null;
    }
    public Chunk AccesChunk(int row, int column)
    {
       Vector3Int chunkPos = RowColumnToChunkPos(row, column);
       if (_ChunkMap.ContainsKey(chunkPos))
            return _ChunkMap[chunkPos];

        return null;
    }

    Vector3Int RowColumnToChunkPos(int row, int column)
    {
       return TerrainMap.RoundToVector3Int(transform.position) 
            + new Vector3Int(row * _TerrainInfo.ChunkWidth, 0, column * _TerrainInfo.ChunkWidth);
    }
}
