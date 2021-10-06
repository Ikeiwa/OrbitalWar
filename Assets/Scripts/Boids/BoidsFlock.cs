using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoidHelper
{
    const int numViewDirections = 50;
    public static readonly Vector3[] directions;

    static BoidHelper()
    {
        directions = new Vector3[numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }
    }
}


[System.Serializable]
public struct Boid
{
    public Vector3 position, velocity, avoidCollision, debug;

    public static int GetStride()
    {
        return sizeof(float) * 3 * 4;
    }

    public void Init(float minSpeed,float maxSpeed)
    {
        position = Random.insideUnitSphere*10;
        velocity = Random.onUnitSphere * Random.Range(minSpeed, maxSpeed);
    }

    bool WillCollide(BoidSettings settings)
    {
        if (Physics.SphereCast(position, settings.boundsRadius, velocity.normalized, out _, settings.collisionAvoidDst, settings.obstacleMask))
        {
            return true;
        }
        return false;
    }

    Vector3 ObstacleRays(BoidSettings settings)
    {
        Vector3[] rayDirections = BoidHelper.directions;

        Quaternion rotation = Quaternion.LookRotation(velocity);

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = rotation*rayDirections[i];
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }
        return velocity;
    }

    public void TestCollision(BoidSettings settings)
    {
        if (WillCollide(settings))
        {
            avoidCollision = ObstacleRays(settings);
        }
    }
}

[System.Serializable]
public class BoidSettings
{
    [Header("Settings")]
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    public float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;
}

public class BoidsFlock : MonoBehaviour
{
    const int threadSize = 1024;

    public GameObject boidPrefab;
    public ComputeShader shader;
    public int boidsCount = 50;
    private GameObject[] boidObjects;
    private Boid[] boids;

    public Transform target;
    public BoidSettings settings;


    // Start is called before the first frame update
    void Start()
    {
        boidObjects = new GameObject[boidsCount];
        boids = new Boid[boidsCount];

        for (int i = 0; i < boidsCount; i++)
        {
            GameObject boid = Instantiate(boidPrefab);
            boid.transform.parent = transform;
            boidObjects[i] = boid;
            boids[i].Init(settings.minSpeed, settings.maxSpeed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (boids != null)
        {

            for (int i = 0; i < boidsCount; i++)
            {
                boids[i].TestCollision(settings);
            }

            var boidBuffer = new ComputeBuffer(boidsCount, Boid.GetStride());
            boidBuffer.SetData(boids);

            shader.SetBuffer(0, "boids", boidBuffer);
            shader.SetInt("boidsCount", boids.Length);
            shader.SetFloat("deltaTime", Time.deltaTime);

            shader.SetFloat("minSpeed", settings.minSpeed);
            shader.SetFloat("maxSpeed", settings.maxSpeed);
            shader.SetFloat("perceptionRadius", settings.perceptionRadius);
            shader.SetFloat("avoidanceRadius", settings.avoidanceRadius);
            shader.SetFloat("maxSteerForce", settings.maxSteerForce);
            shader.SetFloat("alignWeight", settings.alignWeight);
            shader.SetFloat("cohesionWeight", settings.cohesionWeight);
            shader.SetFloat("seperateWeight", settings.seperateWeight);
            shader.SetFloat("targetWeight", settings.targetWeight);
            shader.SetFloat("boundsRadius", settings.boundsRadius);
            shader.SetFloat("avoidCollisionWeight", settings.avoidCollisionWeight);
            shader.SetFloat("collisionAvoidDst", settings.collisionAvoidDst);

            if(target != null)
                shader.SetVector("target", target.position);
            else
                shader.SetVector("target", Vector3.zero);

            int threadGroups = Mathf.CeilToInt(boidsCount / (float)threadSize);
            shader.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boids);
            boidBuffer.Release();

            for(int i = 0; i < boidsCount; i++)
            {
                boidObjects[i].transform.position = boids[i].position;
                boidObjects[i].transform.rotation = Quaternion.LookRotation(boids[i].velocity);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if(boids != null)
        {
            for (int i = 0; i < boids.Length; i++)
            {
                Gizmos.DrawSphere(boids[i].position, settings.boundsRadius);
                Gizmos.DrawWireSphere(boids[i].position, settings.avoidanceRadius);
                Gizmos.DrawLine(boids[i].position, boids[i].position + boids[i].velocity);
            }
        }
        
    }
}
