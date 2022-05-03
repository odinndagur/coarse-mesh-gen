using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer), typeof(MeshCollider))]
public class MeshGenerator : MonoBehaviour
{

    float[] bigArr;
    int height = 2499;
    int width = 2500;
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 250;
    public int zSize = 250;
    public int xoff = 0;
    public int zoff = 0;
    // public int change_val = 250;

    // private float nextTime = 0.0f;
    // public float changeRate = 0.4f;
    // public bool alwaysRegenerate = false;
    [Range(0.0f,1.0f)]
    public float t = 0.0f;
    float lastT = 0.0f;
    ComputeBuffer heightsBuffer;
    ComputeBuffer positionsBuffer;
    ComputeBuffer positionsBuffer2;
    Vector3[] buff;
    Vector3[] buff2;
    [SerializeField]
    ComputeShader cs;
    static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
        heightsId = Shader.PropertyToID("_Heights"),
		tId = Shader.PropertyToID("_t");

    //  #if UNITY_EDITOR
    // instead of @script ExecuteInEditMode()
    [ContextMenu("Generate mesh")]
    void GenerateMesh() 
    {
        loadData();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }
    [ContextMenu("Load Data")]
    void loadData(){
        bigArr = new float[width*height];
        LoadTilemap("/Users/odinndagur/Code/Github/binary_writer_py/test_heights2.dat");
    }
    // #endif
    

    void OnEnable () {
		heightsBuffer = new ComputeBuffer(width*height,sizeof(float));
        loadData();
        heightsBuffer.SetData(bigArr);
        cs.SetBuffer(0,heightsId,heightsBuffer);
		positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        
	}

    void OnDisable () {
        heightsBuffer.Release();
        heightsBuffer = null;
		positionsBuffer.Release();
        positionsBuffer = null;
        positionsBuffer2.Release();
        positionsBuffer2 = null;
	}
    // Start is called before the first frame update
    void Start()
    {
        generateOtherBuffer(xoff,zoff);
        GenerateMesh();
        UpdateMesh();
    }

    void Update(){
        // t = ((float)Mathf.Sin(Time.deltaTime) + 1.0f);
        t = GameObject.Find("LerpController").GetComponent<LerpController>().t;
        // GenerateMesh();
        // UpdateMesh();
        if(t != lastT){
            updateOnGPU();
            Debug.Log("gpu");
            lastT = t;
        }
        // if (Time.time >= nextTime){
        //     nextTime = Time.time + changeRate;
        //     change_shape();
        // }
        // updateOnGPU();
    }

    [ContextMenu("Update on gpu")]
    void updateOnGPU(){
        if(heightsBuffer == null || positionsBuffer == null || positionsBuffer2 == null){
            heightsBuffer = new ComputeBuffer(width*height,sizeof(float));
            loadData();
            heightsBuffer.SetData(bigArr);
            cs.SetBuffer(0,heightsId,heightsBuffer);
            positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
            positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));

        }
        // Debug.Log(mesh.vertices[500]);
        int kernelHandle = cs.FindKernel("CSMain");
        cs.SetFloat("_t",t);
        positionsBuffer2.SetData(buff2);
        positionsBuffer.SetData(buff);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Heights", heightsBuffer);
        int groups = Mathf.CeilToInt(251 / 8f);
        cs.SetInt("_Groupsize",groups);
		cs.Dispatch(kernelHandle, groups * groups, 1, 1);
        Vector3[] data = new Vector3[756012 / 12];
        // Debug.Log(data[62000]);
        positionsBuffer.GetData(data);
        Debug.Log(data[62000]);



        // Vector3[] v = mesh.vertices;
        // Debug.Log(v[25]);
        // positionsBuffer.GetData (v);
        // Debug.Log(v[25]);


        mesh.vertices = data;
        mesh.RecalculateNormals ();
        mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Debug.Log(groups);
        // Debug.Log(mesh.vertices[500]);
    }

    void generateOtherBuffer(int xoff, int zoff){
        buff = new Vector3[(xSize + 1) * (zSize + 1)];
        buff2 = new Vector3[(xSize + 1) * (zSize + 1)];
        

        for (int i = 0, z =0; z<= zSize; z++)
        {
            for (int x = 0; x<=xSize; x++)
            {
                // float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                // float y = get_height_interpolated(xoff + x, zoff + z,t);
                float y = get_height(xoff + x, zoff + z);
                buff[i] = new Vector3(x, y, z);
                y = get_height(xoff + 250 + x, zoff + 250 + z);
                buff2[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }
    public void SetOffset(int xo, int zo){
        xoff = xo;
        zoff = zo;
        if(xoff > width - xSize){
            xoff = 0;
        }
        if(zoff > height - zSize){
            zoff = 0;
        }
        CreateShape();
        UpdateMesh();
    }

    // void change_shape(){
    //     xoff+=change_val;
    //     zoff+=change_val;

    //     if(xoff > width - xSize){
    //         xoff = 0;
    //     }
    //     if(zoff > height - zSize){
    //         zoff = 0;
    //     }
    //     CreateShape();
    //     UpdateMesh();
    // }

    float get_height(int x, int z){
        float height = bigArr[z * width + x];
        // Debug.Log(height);
        if(height < -3.0f){
            Debug.Log("<-3.0f");
            Debug.Log(height);
            return -100.0f;
        }
        else {
            return bigArr[z * width + x];
        }
        // return arr[x,y];
        // return x % 2 + z % 5;
        // return Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
    }

    float get_height_interpolated(int x, int z, float t){
        float height1 = get_height(x,z);
        float height2 = get_height(x+750,z+750);
        return Mathf.Lerp(height1,height2,t);
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z =0; z<= zSize; z++)
        {
            for (int x = 0; x<=xSize; x++)
            {
                // float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                // float y = get_height_interpolated(xoff + x, zoff + z,t);
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
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // optionally, add a mesh collider (As suggested by Franku Kek via Youtube comments).
        // To use this, your MeshGenerator GameObject needs to have a mesh collider
        // component added to it.  Then, just re-enable the code below.
        
        mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        
    }

    static double[] GetDoublesAlt(byte[] bytes)
    {
        var result = new double[bytes.Length / sizeof(double)];
        Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
        return result;
    }

    private void LoadTilemap(string aFileName)
    {
        try
        {
            using (var fileStream = System.IO.File.OpenRead(aFileName))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < width * height; i++){
                    bigArr[i] = reader.ReadSingle();
                }

                // var magic = reader.ReadString();
                // // check your file magic to identify your file, so you can be sure
                // // you access the right file
                // if (magic != "YourFileMagic")
                //     throw new System.Exception("Wrong file format");
                // // check your file version in order to be future proof
                // var version = reader.ReadInt32();
                // if (version != 1)
                //     throw new System.Exception("Not supported file version");
                // // read our own 
                // regionMapSize.x = reader.ReadInt32();
                // regionMapSize.y = reader.ReadInt32();
                // regionsize = reader.ReadInt32();
                // tilemapRegionValues = new float[regionMapSize.x, regionMapSize.y, regionsize, regionsize];
                // for (int regionX = 0; regionX < regionMapSize.x; regionX++)
                // {
                //     for (int regionY = 0; regionY < regionMapSize.y; regionY++)
                //     {
                //         for (int tilemapX = 0; tilemapX < regionsize; tilemapX++)
                //         {
                //             for (int tilemapY = 0; tilemapY < regionsize; tilemapY++)
                //             {
                //                 float value = reader.ReadSingle();
                //                 tilemapRegionValues[regionX, regionY, tilemapX, tilemapY] = value;
                //             }
                //         }
                //     }
                // }
            }
        }
        catch(System.Exception e)
        {
            // handle errors here.
        }
    }

    /* Optionally, draw spheres at each vertex
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i=0; i<vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
    */

}