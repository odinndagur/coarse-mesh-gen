using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    ComputeBuffer outputPositions;
    ComputeBuffer fftBuffer;
    Vector3[] buff;
    Vector3[] buff2;
    public Material mat;
    [SerializeField]
    ComputeShader cs;
    static readonly int tId = Shader.PropertyToID("_t");

    public AudioPlayer audioPlayer;

    // public enum type {sine,slide};
    public bool sine = false;
    public float audioLerpFactor = 0.0f;
    public float audioMultiplier = 1.0f;

    // public Shader shader;
    // Material material;




    [ContextMenu("Start")]
    void Start(){
        // material = new Material(shader);
        loadMeshAndGenerateBufferArrays();
        positionsBuffer.SetData(buff);
        positionsBuffer2.SetData(buff2);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
        cs.SetBuffer(0, "_OutputPositions", outputPositions);
        // material.SetBuffer(0,"meshVertices", outputPositions);


        GenerateMesh();
        UpdateMesh();
        updateOnGPU();
    }
    void Update(){
        if(t != lastT || audioPlayer.GetComponent<AudioSource>().isPlaying){
            updateOnGPU();
            // Debug.Log("gpu");
            lastT = t;
        }
        if(sine){
        t = (Mathf.Sin(Time.time * morphSpeed) + 1)/2;
        }
        // updateOnGPU();
    }


    [ContextMenu("Generate mesh")]
    void GenerateMesh() 
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }


    void OnEnable () {
        // loadData();
		positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        outputPositions = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        fftBuffer = new ComputeBuffer(256, sizeof(float));

	}

    void OnDisable () {
		positionsBuffer.Release();
        positionsBuffer = null;
        positionsBuffer2.Release();
        positionsBuffer2 = null;
        outputPositions.Release();
        outputPositions = null;
        fftBuffer.Release();
        fftBuffer = null;
	}
    
    [ContextMenu("Buffer Setup")]
    void bufferSetup(){
                    // heightsBuffer = new ComputeBuffer(width*height,sizeof(float));
            // loadData();
            // heightsBuffer.SetData(bigArr);
            // cs.SetBuffer(0,heightsId,heightsBuffer);
        positionsBuffer = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2 = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        outputPositions = new ComputeBuffer((xSize+1)*(zSize+1), 3*sizeof(float));
        positionsBuffer2.SetData(buff2);
        positionsBuffer.SetData(buff);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
        cs.SetBuffer(0, "_OutputPositions", outputPositions);
    }
    [ContextMenu("Update on gpu")]
    void updateOnGPU(){
        // if(positionsBuffer == null || positionsBuffer2 == null){
        //     bufferSetup();
        // }
        // positionsBuffer2.SetData(buff2);
        // positionsBuffer.SetData(buff);
        cs.SetBuffer(0, "_Positions", positionsBuffer);
        cs.SetBuffer(0, "_Positions2", positionsBuffer2);
        cs.SetBuffer(0, "_OutputPositions", outputPositions);
        // material.SetBuffer (0, "meshVertices", outputPositions);

        fftBuffer.SetData(audioPlayer.spectrum);
        cs.SetBuffer(0, "_fftBuffer", fftBuffer);

        cs.SetFloat("_audioLerpFactor",audioLerpFactor);
        cs.SetFloat("_audioMultiplier",audioMultiplier);

        int groups = Mathf.CeilToInt(zSize / 8f);
        cs.SetInt("_Groupsize",groups);
        int kernelHandle = cs.FindKernel("CSMain");
        cs.SetFloat("_t",t);
		cs.Dispatch(kernelHandle, groups * groups, 1, 1);
        Vector3[] data = new Vector3[(xSize+1) * (zSize+1)];
        outputPositions.GetData(data);
        // Debug.Log(data[62000]);
        mesh.vertices = data;
        mesh.RecalculateNormals ();
    }

    float[] loadTerrainArray(string filepath, int arraySize){
        float[] temp = new float[arraySize];
        string fpath = Path.Combine(Application.streamingAssetsPath, filepath);
        try
        {
            using (var fileStream = System.IO.File.OpenRead(fpath))
            using (var reader = new System.IO.BinaryReader(fileStream))
            {
                for(int i = 0; i < arraySize; i++){
                    temp[i] = reader.ReadSingle();
                }
                return temp;
            }
        }
        catch(System.Exception e){ // handle errors here.
            Debug.Log(e);
        }
        return new float[1];
    }
    
    [ContextMenu("Load custom mesh data and generate buffers")]
    void loadMeshAndGenerateBufferArrays(){
        int buffSize = (xSize + 1) * (zSize + 1);
        buff = new Vector3[buffSize];
        buff2 = new Vector3[buffSize];

        float[] temp = loadTerrainArray("xsize_zsize_251_step_20_offset_12000.dat",buffSize);
        float[] temp2 = loadTerrainArray("xsize_zsize_251_step_20_offset_18000.dat",buffSize);

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
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for(int i = 0; i < vertices.Length; i++){
            vertices[i] = buff[i];
        }

        // for (int i = 0, z =0; z<= zSize; z++)
        // {
        //     for (int x = 0; x<=xSize; x++)
        //     {
        //         // float y = get_height(xoff + x, zoff + z);
        //         // float y = get_height(x, z);
        //         float y = buff[i];
        //         vertices[i] = new Vector3(x, y, z);
        //         i++;
        //     }
        // }

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