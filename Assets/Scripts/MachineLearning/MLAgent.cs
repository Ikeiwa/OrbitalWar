using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class NeuralConnection
{
    public Neuron neuron;
    public float weight = 0.5f;

    public NeuralConnection(Neuron neuron, float weight = 0.5f)
    {
        this.neuron = neuron;
        this.weight = weight;
    }
}

public class Neuron
{
    public List<NeuralConnection> Inputs;
    public float bias = 1;
    public float value = 0;

    public Neuron()
    {
        Inputs = new List<NeuralConnection>();
        bias = Random.Range(0, 1);
    }

    public void AddInput(Neuron neuron)
    {
        Inputs.Add(new NeuralConnection(neuron, Random.Range(0,1)));
    }

    public void Update()
    {
        float result = 0;
        foreach (var input in Inputs)
        {
            result += input.neuron.value * input.weight;
        }

        result *= bias;

        value = 1 / (1 + Mathf.Exp(-result));
    }

    public void Copy(Neuron other)
    {
        for (int i = 0; i < Inputs.Count && i < other.Inputs.Count; i++)
        {
            Inputs[i].weight = other.Inputs[i].weight;
        }

        bias = other.bias;
    }

    public void Mutate(float percent)
    {
        foreach (var input in Inputs)
        {
            if (Random.value < percent)
            {
                input.weight = Random.Range(-1, 1);
            }
        }

        if (Random.value < percent)
        {
            bias += Random.Range(-0.05f, 0.05f);
        }
    }
}

public class NeuralNetwork
{
    public List<List<Neuron>> network;
    public Dictionary<string, Neuron> inputs;
    public Dictionary<string, Neuron> outputs;

    public readonly int hiddenLayers = 3;
    public readonly int neuronsPerLayer = 4;

    public NeuralNetwork(int hiddenLayers = 3, int neuronsPerLayer = 4)
    {
        this.hiddenLayers = hiddenLayers;
        this.neuronsPerLayer = neuronsPerLayer;

        network = new List<List<Neuron>>();
        for (int i = 0; i < hiddenLayers; i++)
        {
            network.Add(new List<Neuron>());
            for (int n = 1; n < neuronsPerLayer; n++)
            {
                network[i].Add(new Neuron());
            }
        }

        for (int i = 1; i < hiddenLayers; i++)
        {
            network.Add(new List<Neuron>());
            for (int n = 1; n < network[i].Count; n++)
            {
                for (int p = 1; p < network[i].Count; p++)
                {
                    network[i][n].AddInput(network[i - 1][p]);
                }
            }
        }

        inputs = new Dictionary<string, Neuron>();
        outputs = new Dictionary<string, Neuron>();
    }

    public void AddInput(string name)
    {
        Neuron input = new Neuron();
        inputs.Add(name, input);

        foreach (var neuron in network[0])
        {
            neuron.AddInput(input);
        }
    }

    public void AddOutput(string name)
    {
        Neuron output = new Neuron();
        outputs.Add(name, output);

        foreach (var neuron in network.Last())
        {
            output.AddInput(neuron);
        }
    }

    public void SetInput(string name, float value)
    {
        if(inputs.ContainsKey(name))
            inputs[name].value = value;
    }

    public float GetOutput(string name)
    {
        if (outputs.ContainsKey(name))
            return outputs[name].value;
        return 0;
    }

    public void Update()
    {
        foreach (var layer in network)
        {
            for (int n = 0; n < network.Count; n++)
            {
                layer[n].Update();
            }
        }

        foreach (var output in outputs.Values)
        {
            output.Update();
        }
    }

    public void Mutate(float percent)
    {
        foreach (var layer in network)
        {
            for (int n = 0; n < network.Count; n++)
            {
                layer[n].Mutate(percent);
            }
        }
    }

    public NeuralNetwork MakeChild(NeuralNetwork other, float ratio = 0.5f)
    {
        NeuralNetwork child = new NeuralNetwork(hiddenLayers, neuronsPerLayer);
        foreach (var input in inputs.Keys) { child.AddInput(input); }
        foreach (var output in outputs.Keys) { child.AddOutput(output); }

        for (int l = 0; l < child.network.Count; l++)
        {
            for (int n = 0; n < child.network.Count; n++)
            {
                bool useThis = Random.value < ratio;

                child.network[l][n].Copy(useThis ? network[l][n] : other.network[l][n]);
            }
        }

        return child;
    }

    public void Draw()
    {

    }
}

public class MLAgent : MonoBehaviour
{
    public NeuralNetwork network;
    public float score = 0;
    public bool dead = false;

    public virtual void GenerateNetwork()
    {
        network = new NeuralNetwork();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
