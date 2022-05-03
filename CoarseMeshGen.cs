using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer), typeof(MeshCollider))]
public class CoarseMeshGen : MonoBehaviour
{

    float[] bigArr;
    int height = 2499;
    int width = 2500;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    public int xSize = 250;
    public int zSize = 250;
    public float morphSpeed = 1.0f;
    public int xoff = 0;
    public int zoff = 0;

    [Range(0.0f,1.0f)]
    public float t = 0.0f;
    float lastT = -0.0f;
    ComputeBuffer positionsBuffer;
    ComputeBuffer positionsBuffer2;
    Vector3[] buff;
    Vector3[] buff2;
    public Material mat;
    [SerializeField]
    ComputeShader cs;
    static readonly int tId = Shader.PropertyToID("_t");

    [ContextMenu("Start")]
    void Start(){
        // loadData();
        // generateBuffersLarge();
        loadMeshAndGenerateBuffers();
        GenerateMesh();
        UpdateMesh();
        updateOnGPU();
    }
    void Update(){
        // t = GameObject.Find("LerpController").GetComponent<LerpController>().t;
        // GenerateMesh();
        // UpdateMesh();
        if(t != lastT){
            updateOnGPU();
            Debug.Log("gpu");
            lastT = t;
        }
                t = (Mathf.Sin(Time.time * morphSpeed) + 1)/2;

    }



    //  #if UNITY_EDITOR
    // instead of @script ExecuteInEditMode()
    [ContextMenu("Generate mesh")]
    void GenerateMesh() 
    {
        // loadData();
        // loadDataWithSkips();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }
    [ContextMenu("Load Data")]
    void loadDataWorks(){
        bigArr = new float[width*height];
        try
        {
            using (var fileStream = System.IO.File.OpenRead("/Users/odinndagur/Code/Github/binary_writer_py/test_heights2.dat"))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < width * height; i++){
                    bigArr[i] = reader.ReadSingle();
                }
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }
    }

    void loadData(){
        width = 25000;
        height = 24999;
        bigArr = new float[width*height];
        try
        {
            using (var fileStream = System.IO.File.OpenRead("/Users/odinndagur/Code/Github/binary_writer_py/test_heights34.dat"))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < width * height; i++){
                    bigArr[i] = reader.ReadSingle();
                }
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }
    }
    [ContextMenu("Load data with skips")]
    void loadDataWithSkips(){
        int data_width = 25000;
        int data_height = 24999;
        width = 6000;
        height = 6000;
        bigArr = new float[width*height];
        try
        {
            using (var fileStream = System.IO.File.OpenRead("/Users/odinndagur/Code/Github/binary_writer_py/test_heights57.dat"))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {

                //testing the skips
                for(int j = 0; j < zoff; j++){
                    for(int jj = 0; jj < data_width; jj++){
                        reader.ReadSingle(); //throw away entire lines until we get to correct zoff line
                    }
                    for(int jj = 0; jj < xoff; jj++){
                        reader.ReadSingle(); //throw away x values until we get to correct one
                    }
                }
                for(int y = zoff, i = 0; y < height + zoff; y++){
                    for(int x = xoff; x < width + xoff; x++){
                        bigArr[i++] = reader.ReadSingle();
                    }
                    for(int j = xoff; j < data_width - width; j++){
                        reader.ReadSingle(); //throw away rest of the line and the beginning of next line
                    }
                }


                // for(int i = 0; i < width * height; i++){
                //     bigArr[i] = reader.ReadSingle();
                // }
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }
    }

    // #endif
    void OnEnable () {
        // loadData();
		positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2.SetData(buff2);
        positionsBuffer.SetData(buff);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
	}

    void OnDisable () {
		positionsBuffer.Release();
        positionsBuffer = null;
        positionsBuffer2.Release();
        positionsBuffer2 = null;
	}
    
    [ContextMenu("Buffer Setup")]
    void bufferSetup(){
                    // heightsBuffer = new ComputeBuffer(width*height,sizeof(float));
            // loadData();
            // heightsBuffer.SetData(bigArr);
            // cs.SetBuffer(0,heightsId,heightsBuffer);
        positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2.SetData(buff2);
        positionsBuffer.SetData(buff);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
    }
    [ContextMenu("Update on gpu")]
    void updateOnGPU(){
        if(positionsBuffer == null || positionsBuffer2 == null){
            bufferSetup();
        }
        int groups = Mathf.CeilToInt(zSize / 8f);
        cs.SetInt("_Groupsize",groups);
        int kernelHandle = cs.FindKernel("CSMain");
        cs.SetFloat("_t",t);
		cs.Dispatch(kernelHandle, groups * groups, 1, 1);
        Vector3[] data = new Vector3[(xSize+1) * (zSize+1)];
        positionsBuffer.GetData(data);
        Debug.Log(data[62000]);
        mesh.vertices = data;
        mesh.RecalculateNormals ();
    }

    [ContextMenu("Generate buffers")]
    void generateBuffers(){
        int buffSize = (xSize + 1) * (zSize + 1);
        buff = new Vector3[buffSize];
        buff2 = new Vector3[buffSize];
        
        for (int i = 0, z =0; z<= zSize; z++)
        {
            for (int x = 0; x<=xSize; x++)
            {
                float y = get_height(xoff + x, zoff + z);
                buff[i] = new Vector3(x, y, z);
                y = get_height(xoff + xSize + x, zoff + zSize + z);
                buff2[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }
    [ContextMenu("Generate buffers large")]
    void generateBuffersLarge(){
        int buffSize = (xSize + 1) * (zSize + 1);
        buff = new Vector3[buffSize];
        buff2 = new Vector3[buffSize];

        int step = 20;
        
        for (int i = 0, z =0; z<= zSize * step; z+= step)
        {
            for (int x = 0; x<=xSize * step; x+= step)
            {
                float y = get_height(xoff + x, zoff + z);
                buff[i] = new Vector3(x, y, z);
                y = get_height(xoff + xSize + x, zoff + zSize + z);
                buff2[i] = new Vector3(x, y, z);
                i++;
            }
        }

        // for (int i = 0, z = 0; z<= zSize * step; z+= step) {
        //     for (int x = 0; x<=xSize * step; x+= step) {
        //         float y = get_height(x, z);
        //         buff[i] = new Vector3(x, y, z);
        //         y = get_height(xSize + x, zSize + z);
        //         buff2[i] = new Vector3(x, y, z);
        //         i++;
        //     }
        // }
    }

    [ContextMenu("Load custom mesh data and generate buffers")]
    void loadMeshAndGenerateBuffers(){
        int buffSize = (xSize + 1) * (zSize + 1);
        buff = new Vector3[buffSize];
        buff2 = new Vector3[buffSize];

        float[] temp = new float[buffSize];
        float[] temp2 = new float[buffSize];

        try
        {
            using (var fileStream = System.IO.File.OpenRead("/Users/odinndagur/Code/Github/binary_writer_py/meshdata/xsize_zsize_251_step_20_offset_12000.dat"))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < width * height; i++){
                    temp[i] = reader.ReadSingle();
                }
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }

        
        try
        {
            using (var fileStream = System.IO.File.OpenRead("/Users/odinndagur/Code/Github/binary_writer_py/meshdata/xsize_zsize_251_step_20_offset_18000.dat"))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < width * height; i++){
                    temp2[i] = reader.ReadSingle();
                }
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }

        int step = 20;
        
        for (int i = 0, z = 0; z<= zSize * step; z+= step)
        {
            for (int x = 0; x<=xSize * step; x+= step)
            {
                // float y = get_height(xoff + x, zoff + z);
                buff[i] = new Vector3(x, temp[i], z);
                // y = get_height(xoff + xSize + x, zoff + zSize + z);
                buff2[i] = new Vector3(x, temp2[i], z);
                i++;
            }
        }






        // for (int i = 0, z = 0; z<= zSize * step; z+= step) {
        //     for (int x = 0; x<=xSize * step; x+= step) {
        //         float y = get_height(x, z);
        //         buff[i] = new Vector3(x, y, z);
        //         y = get_height(xSize + x, zSize + z);
        //         buff2[i] = new Vector3(x, y, z);
        //         i++;
        //     }
        // }
    }
    float get_height(int x, int z){
        float height = bigArr[z * width + x];
        if(height < -100.0f){
            return -100.0f;
        }
        else {
            return height;
        }
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z =0; z<= zSize; z++)
        {
            for (int x = 0; x<=xSize; x++)
            {
                // float y = get_height(xoff + x, zoff + z);
                // float y = get_height(x, z);
                float y = 0.0f;
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
}