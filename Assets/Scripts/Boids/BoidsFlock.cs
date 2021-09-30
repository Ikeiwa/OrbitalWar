using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoidData
{
    public Vector3 pos, direction, flockDirection, flockCenter, avoidDirection;
    public int nearBoids;

    public static int GetStride()
    {
        return sizeof(float) * 3 * 5 + sizeof(int);
    }
}

public class BoidsFlock : MonoBehaviour
{
    const int threadSize = 1024;

    public Boid boidPrefab;
    public BoidSettings settings;
    public ComputeShader shader;
    public int boidsCount = 50;
    public Boid[] boids;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < boidsCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 5;
            Boid boid = Instantiate(boidPrefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;
            boid.transform.parent = transform;
        }

        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids)
        {
            b.Init(settings, null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (boids != null)
        {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++)
            {
                boidData[i].pos = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.GetStride());
            boidBuffer.SetData(boidData);

            shader.SetBuffer(0, "boids", boidBuffer);
            shader.SetInt("numBoids", boids.Length);
            shader.SetFloat("viewRadius", settings.perceptionRadius);
            shader.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadSize);
            shader.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Length; i++)
            {
                boids[i].avgFlockHeading = boidData[i].flockDirection;
                boids[i].centreOfFlockmates = boidData[i].flockCenter;
                boids[i].avgAvoidanceHeading = boidData[i].avoidDirection;
                boids[i].numPerceivedFlockmates = boidData[i].nearBoids;

                boids[i].UpdateBoid();
            }

            boidBuffer.Release();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if(boids != null)
        {
            for (int i = 0; i < boids.Length; i++)
            {
                Gizmos.DrawSphere(boids[i].position, 0.25f);
                Gizmos.DrawLine(boids[i].position, boids[i].position + boids[i].forward);
            }
        }
        
    }
}
