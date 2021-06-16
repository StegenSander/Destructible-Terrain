using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Chunk
{
    #region Variables
    World _ParentWorld;
    World.TerrainInformation _TerrainInfo;
    
    private GameObject _ChunkObject;
    public GameObject ChunkObject
    {
        get { return _ChunkObject; }
    }
    MeshFilter _MeshFilter;
    MeshCollider _MeshCollider;

    //Vertex Buffer
    private List<Vector3> _VertexBuffer = new List<Vector3>();
    public List<Vector3> VertexBuffer
    {
        get { return _VertexBuffer; }
    }

    //IndexBuffer
    private List<int> _IndexBuffer = new List<int>();
    public List<int> IndexBuffer
    {
        get { return _IndexBuffer; }
    }

    private int _Row;
    private int _Column;

    public bool NeedsUpdate { get; set; } = true;
    #endregion

    public Chunk(World world,int row, int column, Vector3 position)
    {
        _Row = row;
        _Column = column;

        _ParentWorld = world;
        _TerrainInfo = _ParentWorld.TerrainInfo;

        _ChunkObject = new GameObject();
        _ChunkObject.transform.parent = world.transform;
        _ChunkObject.transform.position = position;
        _MeshFilter = _ChunkObject.AddComponent<MeshFilter>();
        _ChunkObject.AddComponent<MeshRenderer>().material = _TerrainInfo.Mat;
        _MeshCollider =  _ChunkObject.AddComponent<MeshCollider>();
    }

    public void CreateMesh()
    {
        Profiler.BeginSample("Clearing Data");
        NeedsUpdate = false;
        //Clear previously used data
        _VertexBuffer.Clear();
        _IndexBuffer.Clear();
        Profiler.EndSample();

        Profiler.BeginSample("Marching cubes");
        for (int x = 0; x < _TerrainInfo.ChunkWidth; x++)
            for (int y = 0; y < _TerrainInfo.ChunkHeight; y++)
                for (int z = 0; z < _TerrainInfo.ChunkWidth; z++)
                {
                    MarchCube( x, y, z);
                }
        Profiler.EndSample();

        Profiler.BeginSample("Updating mesh");
        UpdateMesh();
        Profiler.EndSample();
    }

    private void MarchCube(int x,int y, int z)
    {
        MarchCube(new Vector3Int(x, y, z));
    }
    private void MarchCube(Vector3Int pos)
    {
        //Get Cube index in the triangle Table
        //loop over all corners of the cube
        int triangleIdx = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int sampPos = 
                new Vector3Int(_Row * _TerrainInfo.ChunkWidth, 0, _Column * _TerrainInfo.ChunkWidth) + pos + MarchingCubeData.CornerTable[i];
            if (_ParentWorld.Terrain.SampleTerrain(sampPos) > 0/*Surface level*/)
                triangleIdx |= 1 << i; //Set the correct bit flag to 1, these bit value match the Triangle Table in Marching Cube Data
        }


        //Debug.Log($"Position:{pos} triangleIndex:{triangleIdx}");
        int idx = 0;
        for (int t = 0; t < 5; t++) //max 5 triangles per cube
            for (int p = 0; p < 3; p++) //3 points per triangle
            {
                //Get the edge out of the triangle table
                int edgeIdx = MarchingCubeData.TriangleTable[triangleIdx, idx];
                if (edgeIdx == -1) //-1 -> end of this triangeTable triangle
                    return;
                //Get the 2 vertices of the edge
                Vector3 vert1 = pos + MarchingCubeData.CornerTable[MarchingCubeData.EdgeTable[edgeIdx, 0]];
                Vector3 vert2 = pos + MarchingCubeData.CornerTable[MarchingCubeData.EdgeTable[edgeIdx, 1]];

                Vector3 vertPos;

                vertPos = (vert1 + vert2) / 2f;

                //_IndexBuffer.Add(AddToVertexBuffer(vertPos));
                _VertexBuffer.Add(vertPos);
                _IndexBuffer.Add(_VertexBuffer.Count - 1);

                idx++;
            }
    }

    //Add to vertex buffer returns the index;
    private int AddToVertexBuffer(Vector3 v)
    {
        int index = _VertexBuffer.IndexOf(v);
        if (index != -1)
            return index;

        _VertexBuffer.Add(v); //add vertex to list
        return _VertexBuffer.Count - 1; //return new idx
    }

    private void UpdateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = _VertexBuffer.ToArray();
        mesh.triangles = _IndexBuffer.ToArray();
        mesh.RecalculateNormals();

        _MeshFilter.mesh = mesh;
        _MeshCollider.sharedMesh = mesh;
    }
}
