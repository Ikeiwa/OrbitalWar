using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MLAgent : MonoBehaviour
{
    public class Neuron
    {

    }

    public class NeuralNetwork
    {
        public readonly List<List<Neuron>> network;
        public readonly Dictionary<string, int> inputs;
        public readonly Dictionary<string, int> outputs;

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

            inputs = new Dictionary<string, int>();
            outputs = new Dictionary<string, int>();
        }

        public void AddInput(string name)
        {
            inputs.Add(name,network[0].Count);
            network[0].Add(new Neuron());
        }

        public void AddOutput(string name)
        {
            outputs.Add(name,network.Last().Count);
            network.Last().Add(new Neuron());
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
