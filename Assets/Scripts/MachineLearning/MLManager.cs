using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLManager : MonoBehaviour
{
    public MLAgent agentPrefab;
    public Transform spawnPoint;
    public int nbAgents = 50;
    public int selectedTop = 10;
    public float mutationRate = 0.02f;

    public List<MLAgent> agents;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
