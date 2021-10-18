using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MLAgent : MonoBehaviour
{
    public class NeuralConnection
    {
        public Neuron neuron;
        public float weight = 1;

        public NeuralConnection(Neuron neuron, float weight)
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
        }

        public void AddInput(Neuron neuron)
        {
            Inputs.Add(new NeuralConnection(neuron,1));
        }
    }

    public class NeuralNetwork
    {
        public readonly List<List<Neuron>> network;
        public readonly Dictionary<string, Neuron> inputs;
        public readonly Dictionary<string, Neuron> outputs;

        public NeuralNetwork(int hiddenLayers = 3, int neuronsPerLayer = 4)
        {
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
                        network[i][n].AddInput(network[i-1][p]);
                    }
                }
            }

            inputs = new Dictionary<string, Neuron>();
            outputs = new Dictionary<string, Neuron>();
        }

        public void AddInput(string name)
        {
            Neuron input = new Neuron();
            inputs.Add(name,input);

            foreach (var neuron in network[0])
            {
                neuron.AddInput(input);
            }
        }

        public void AddOutput(string name)
        {
            Neuron output = new Neuron();
            outputs.Add(name,output);

            foreach (var neuron in network.Last())
            {
                output.AddInput(neuron);
            }
        }

        public void Mutate(float percent)
        {

        }

        public NeuralNetwork MakeChild(NeuralNetwork other, float ratio = 0.5f)
        {
            return null;
        }
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
