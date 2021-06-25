using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainModifier
{
    public enum TerrainChange
    {
        Remove,
        Add,
    }

    static public void ModifyTerrain(World world, Collider col, TerrainChange change)
    {
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
                        float valueToChange = 1;
                        if (TerrainChange.Add == change)
                            valueToChange = -1;

                        Chunk c = world.AccesChunk(worldPos);
                        c.Terrain.SetTerrainValue(worldPos, valueToChange);

                        bool xBorder = x % world.TerrainInfo.ChunkWidth == 0;
                        bool zBorder = z % world.TerrainInfo.ChunkWidth == 0;
                        if (xBorder)
                        {
                            c = world.AccesChunk(worldPos + new Vector3(-0.001f,0,0));
                            c.Terrain.SetTerrainValue(worldPos, valueToChange);

                        }
                        if (zBorder)
                        {
                            c = world.AccesChunk(worldPos + new Vector3(0, 0, -0.001f));
                            c.Terrain.SetTerrainValue(worldPos, valueToChange);
                        }
                        if (xBorder && zBorder)
                        {
                            c = world.AccesChunk(worldPos + new Vector3(-0.001f, 0, -0.001f));
                            c.Terrain.SetTerrainValue(worldPos, valueToChange);
                        }
                    }
                }
    }
}
