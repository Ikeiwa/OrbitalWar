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
        bias = Random.value;
    }

    public void AddInput(Neuron neuron)
    {
        float weight = Random.Range(-1f,1f);
        Inputs.Add(new NeuralConnection(neuron, weight));
    }

    public void Update()
    {
        float result = 0;
        foreach (var input in Inputs)
        {
            result += input.neuron.value * input.weight;
        }

        result *= bias;

        value = (1f / (1f + Mathf.Exp(-result*5)))*2-1;
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
                input.weight = Random.Range(-1f, 1f);
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
            for (int n = 0; n < neuronsPerLayer; n++)
            {
                network[i].Add(new Neuron());
                if (i > 0)
                {
                    for (int p = 1; p < network[i].Count; p++)
                    {
                        network[i][n].AddInput(network[i-1][p]);
                    }
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
            for (int n = 0; n < layer.Count; n++)
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
        int i = 0;
        foreach (var input in inputs.Values)
        {
            GUI.Label(new Rect(10, i*40+10, 50, 50),input.value.ToString("0.00"));
            i++;
        }

        for (int l=0;l< network.Count;l++)
        {
            for (int n = 0; n < network[l].Count; n++)
            {
                
                GUI.Label(new Rect((l+1)*40+10, n * 40 + 10, 50, 50), network[l][n].value.ToString("0.00"));
            }
        }

        i = 0;
        foreach (var output in outputs.Values)
        {
            GUI.Label(new Rect((network.Count+1)*40+10,i * 40 + 10, 50, 50), output.value.ToString("0.00"));
            i++;
        }

        
    }
}

public class MLAgent : MonoBehaviour
{
    public GameObject model;
    public NeuralNetwork network;
    public float score = 0;
    public bool dead = false;

    public virtual void GenerateNetwork()
    {
        network = new NeuralNetwork();
    }

    public virtual void CalculateScore(){

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void UpdateAgent()
    {
        
    }

    public void Draw()
    {
        GUI.color = Color.red;
        network.Draw();

        GUI.color = Color.cyan;
        GUI.Label(new Rect(Screen.width-50, 10, 50, 50), score.ToString("0.00"));
    }
}
