using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLDrone : MLAgent
{
    private Vector3 velocity;

    public override void GenerateNetwork()
    {
        network = new NeuralNetwork(5, 8);
        network.AddInput("Dist forward");
        network.AddInput("Dist up");
        network.AddInput("Dist down");
        network.AddInput("Dist left");
        network.AddInput("Dist right");

        network.AddOutput("Fly up");
        network.AddOutput("Fly forward");
        network.AddOutput("Turn");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private float RayDistance(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, 5, 1 << 0))
        {
            return hit.distance / 5;
        }

        return 1;
    }

    // Update is called once per frame
    void Update()
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

            float flyUp = Mathf.Clamp01(network.GetOutput("Fly up"));
            float flyForward = Mathf.Clamp01(network.GetOutput("Fly forward"));
            float turn = Mathf.Clamp01(network.GetOutput("Turn"));




            transform.position += velocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        dead = true;
    }
}       
