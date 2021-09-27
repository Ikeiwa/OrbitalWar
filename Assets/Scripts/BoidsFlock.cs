using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Boid
{
    public Vector3 pos, velocity;
}

public class BoidsFlock : MonoBehaviour
{
    public ComputeShader shader;
    public int boidsCount = 50;
    public Boid[] boidsData;
    public Vector3 volume;

    private int strideSize;
    private int kernelHandle;
    private ComputeBuffer buffer;
    private Texture3D sceneTex;

    // Start is called before the first frame update
    void Start()
    {
        boidsData = new Boid[boidsCount];
        kernelHandle = shader.FindKernel("CSMain");

        strideSize = sizeof(float) * 3 + sizeof(float) * 3;
        buffer = new ComputeBuffer(boidsCount, strideSize);


        for (int i = 0; i < boidsCount; i++)
        {
            boidsData[i] = new Boid { 
                pos = new Vector3(Random.Range(0, volume.x), Random.Range(0, volume.y), Random.Range(0, volume.z)), 
                velocity = Random.insideUnitSphere*5 
            };
        }

        buffer.SetData(boidsData);
        shader.SetBuffer(kernelHandle, "boidBuffer", buffer);
        shader.SetVector("volume", volume);
        shader.SetFloat("boidsCount", boidsCount);

        UpdateScene();

        transform.position = volume / 2;
        transform.localScale = volume;
    }

    public void UpdateScene()
    {
        int size = 32;
        TextureFormat format = TextureFormat.Alpha8;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        sceneTex = new Texture3D(size, size, size, format, false);
        sceneTex.wrapMode = wrapMode;

        Color[] colors = new Color[size * size * size];
        Vector3 cellSize = volume / (float)size;

        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    pos.Scale(cellSize);
                    bool hit = Physics.CheckSphere(pos, cellSize.x);
                    colors[x + yOffset + zOffset] = hit ? Color.white : new Color(0,0,0,0);
                }
            }
        }

        sceneTex.SetPixels(colors);
        sceneTex.Apply();

        shader.SetTexture(kernelHandle, "_Scene", sceneTex);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", sceneTex);
    }

    private void OnDestroy()
    {
        buffer.Release();
    }

    // Update is called once per frame
    void Update()
    {
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.Dispatch(kernelHandle, boidsCount, 1, 1);

        buffer.GetData(boidsData);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if(boidsData != null)
        {
            for (int i = 0; i < boidsData.Length; i++)
            {
                Gizmos.DrawSphere(boidsData[i].pos, 0.25f);
                Gizmos.DrawLine(boidsData[i].pos, boidsData[i].pos + boidsData[i].velocity);
            }
        }
        
    }
}
