using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public int xChunkCount = 3;
    public int zChunkCount = 3;
    private static float[] heights;
    private int totalwidth = 2500;
    private int totalheight = 2499;
    Mesh[] mesh;
    Vector3[] vertices;
    int[] triangles;

    public int xSize = 250;
    public int zSize = 250;
    // public int xoff = 0;
    // public int zoff = 0;
    // public int change_val = 250;

    // private float nextTime = 0.0f;
    // public float changeRate = 0.4f;
    // public bool alwaysRegenerate = false;

    void Awake(){
        // loadData();
    }

    // Start is called before the first frame update
    void Start(){
        for(int z = 0; z < zChunkCount; z++){
            for(int x = 0; x < xChunkCount; x++){
                CreateShape(x * xSize,z*zSize);
                UpdateMesh(z*xChunkCount + x);
            }
        }
        // CreateShape();
    }

    // Update is called once per frame
    void Update(){
        
    }

    void loadData(){
        heights = new float[totalwidth*totalheight];
        loadHeights("/Users/odinndagur/Code/Github/binary_writer_py/test_heights2.dat");
    }

    float get_height(int x, int z){
        float height = heights[z * totalwidth + x];
        // Debug.Log(height);
        if(height < -3.0f){
            Debug.Log("<-3.0f");
            Debug.Log(height);
            return -100.0f;
        }
        else {
            return heights[z * totalwidth + x];
        }
        // return arr[x,y];
        // return x % 2 + z % 5;
        // return Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
    }

    void CreateShape(int xoff, int zoff)
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z =0; z<= zSize; z++)
        {
            for (int x = 0; x<=xSize; x++)
            {
                // float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                float y = get_height(xoff + x, zoff + z);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

    }

    void UpdateMesh(int index)
    {
        mesh[index].Clear();
        mesh[index].vertices = vertices;
        mesh[index].triangles = triangles;
        mesh[index].RecalculateNormals();
        // optionally, add a mesh collider (As suggested by Franku Kek via Youtube comments).
        // To use this, your MeshGenerator GameObject needs to have a mesh collider
        // component added to it.  Then, just re-enable the code below.
        
        // mesh[index].RecalculateBounds();
        // MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        // meshCollider.sharedMesh = mesh[index];
        
    }

    private void loadHeights(string aFileName)
    {
        try
        {
            using (var fileStream = System.IO.File.OpenRead(aFileName))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < totalwidth * totalheight; i++){
                    heights[i] = reader.ReadSingle();
                }
            }
        }
        catch(System.Exception e)
        {
            // handle errors here.
            Debug.Log(e);
        }
    }
}
