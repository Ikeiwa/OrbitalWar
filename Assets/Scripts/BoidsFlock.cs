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
    public Vector3Int resolution = new Vector3Int(64, 64, 64);

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

        Vector3 halfVolume = volume / 2;

        for (int i = 0; i < boidsCount; i++)
        {
            boidsData[i] = new Boid { 
                pos = new Vector3(Random.Range(-halfVolume.x, halfVolume.x), Random.Range(-halfVolume.y, halfVolume.y), Random.Range(-halfVolume.z, halfVolume.z)), 
                velocity = Random.insideUnitSphere*5 
            };
        }

        buffer.SetData(boidsData);
        shader.SetBuffer(kernelHandle, "boidBuffer", buffer);
        shader.SetFloat("boidsCount", boidsCount);

        UpdateScene(volume);
    }

    public void UpdateScene(Vector3 size)
    {
        volume = size;

        TextureFormat format = TextureFormat.ARGB32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        sceneTex = new Texture3D(resolution.x, resolution.y, resolution.z, format, false);
        sceneTex.wrapMode = wrapMode;

        Color[] colors = new Color[resolution.x* resolution.y* resolution.z];
        Vector3 cellSize = new Vector3(volume.x / resolution.x, volume.y / resolution.y, volume.z / resolution.z);
        

        for (int z = 0; z < resolution.z; z++)
        {
            int zOffset = z * resolution.x * resolution.y;
            for (int y = 0; y < resolution.y; y++)
            {
                int yOffset = y * resolution.x;
                for (int x = 0; x < resolution.x; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    pos.Scale(cellSize);
                    pos -= volume / 2;

                    //Collider[] hits = Physics.CheckSphere(pos, cellSize.x, 1 << 6);

                    if (Physics.CheckSphere(pos, cellSize.x, 1 << 6))
                    {
                        /*if(hits[0].GetType() == typeof(MeshCollider))
                        {
                            MeshCollider collider = (MeshCollider)hits[0];
                            float minDist = float.MaxValue;
                            Vector3 normal = Vector3.up;

                            for(int i=0;i< collider.sharedMesh.vertexCount; i++)
                            {
                                float dist = (collider.sharedMesh.vertices[i] - pos).sqrMagnitude;
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    normal = collider.sharedMesh.normals[i];
                                }
                            }

                            colors[x + yOffset + zOffset] = new Color(normal.x, normal.y, normal.z, 1);
                        }
                        else
                        {
                            Vector3 dir = (hits[0].ClosestPoint(pos) - pos).normalized;

                            if (Physics.Raycast(pos, dir, out RaycastHit hit, float.PositiveInfinity, 1 << 6))
                            {
                                colors[x + yOffset + zOffset] = new Color(hit.normal.x, hit.normal.y, hit.normal.z, 1);
                            }
                            else
                            {
                                colors[x + yOffset + zOffset] = new Color(-dir.x, -dir.y, -dir.z, 1);
                            }
                        }*/


                        colors[x + yOffset + zOffset] = Color.white; 
                    }
                    else
                    {
                        colors[x + yOffset + zOffset] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }

        sceneTex.SetPixels(colors);
        sceneTex.Apply();

        shader.SetVector("volume", volume);
        shader.SetVector("volumeRes", new Vector3(volume.x / resolution.x, volume.y / resolution.y, volume.z / resolution.z));
        shader.SetTexture(kernelHandle, "_Scene", sceneTex);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", sceneTex);
        transform.localScale = volume;
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

        Gizmos.DrawWireCube(transform.position, volume);
        
    }
}
