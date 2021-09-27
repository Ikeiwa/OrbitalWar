using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                pos = new Vector3(Random.Range(-volume.x, volume.x), Random.Range(-volume.y, volume.y), Random.Range(-volume.z, volume.z)), 
                velocity = Random.insideUnitSphere*5 
            };
        }

        buffer.SetData(boidsData);
        shader.SetBuffer(kernelHandle, "boidBuffer", buffer);
        shader.SetVector("volume", volume);
        shader.SetInt("boidsCount", boidsCount);
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
                Gizmos.DrawWireSphere(boidsData[i].pos, 0.5f);
                Gizmos.DrawLine(boidsData[i].pos, boidsData[i].pos + boidsData[i].velocity);
            }
        }
        
    }
}
