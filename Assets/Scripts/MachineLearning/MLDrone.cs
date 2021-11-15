using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLDrone : MLAgent
{
    public Vector3 velocity;

    public override void GenerateNetwork()
    {
        network = new NeuralNetwork(2, 5);
        //network = new NeuralNetwork(1, 1);
        network.AddInput("Dist forward");
        network.AddInput("Dist up");
        network.AddInput("Dist down");
        network.AddInput("Dist left");
        network.AddInput("Dist right");

        network.AddOutput("Fly up");
        //network.AddOutput("Fly forward");
        network.AddOutput("Turn");
    }

    public override void CalculateScore()
    {
        score = transform.localPosition.z;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private float RayDistance(Vector3 dir)
    {
        Debug.DrawRay(transform.position,dir.normalized*5);

        if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, 5, 1 << 0))
        {
            return hit.distance / 5;
        }

        return 1;
    }

    // Update is called once per frame
    public override void UpdateAgent()
    {
        if (!dead)
        {
            float distForward = RayDistance(transform.forward);
            float distUp = RayDistance(transform.forward + transform.up);
            float distDown = RayDistance(transform.forward - transform.up);
            float distLeft = RayDistance(transform.forward - transform.right);
            float distRight = RayDistance(transform.forward + transform.right);

            network.SetInput("Dist forward", distForward);
            network.SetInput("Dist up", distUp);
            network.SetInput("Dist down", distDown);
            network.SetInput("Dist left", distLeft);
            network.SetInput("Dist right", distRight);

            network.Update();

            float flyUp = Mathf.Clamp01(network.GetOutput("Fly up"))*2-1;
            //float flyForward = Mathf.Clamp01(network.GetOutput("Fly forward"))*2-1;
            float turn = Mathf.Clamp01(network.GetOutput("Turn"))*2-1;

            velocity *= 0.9f;
            velocity += transform.up * flyUp * 0.01f;
            //velocity += transform.forward * flyForward * 0.01f;
            velocity += transform.forward * 0.005f;
            velocity += transform.right * turn * 0.01f;
            velocity = Vector3.ClampMagnitude(velocity, 0.2f);
            //transform.Rotate(Vector3.up * turn);

            transform.position += velocity;

            if (Physics.OverlapSphereNonAlloc(transform.position, 0.09f, new Collider[1], 1 << 0) > 0)
            {
                dead = true;
            }
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        dead = true;
    }*/
}       
