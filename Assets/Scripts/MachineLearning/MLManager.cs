using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class MLManager : MonoBehaviour
{
    public Transform cam;
    public MLAgent agentPrefab;
    public Transform spawnPoint;
    public float updateRate = 60;
    public int nbAgents = 50;
    public int selectedTop = 10;
    public float mutationRate = 0.02f;
    public float maxGenDuration = 600;

    public List<MLAgent> agents;
    public int generation = 0;

    public int selectedAgent = 0;
    public bool showOnlySelected = false;

    // Start is called before the first frame update
    void Start()
    {
        generation = 0;
        for (int i = 0; i < nbAgents; i++)
        {
            MLAgent agent = Instantiate(agentPrefab, spawnPoint);
            agent.GenerateNetwork();
            agents.Add(agent);
        }

        StartCoroutine(UpdateAgents());
    }

    public void AdvanceGeneration()
    {
        agents = agents.OrderByDescending(a => a.score).ToList();

        for (int c = selectedTop; c < nbAgents; c++)
        {
            int father = Random.Range(0, selectedTop);
            int mother = Random.Range(0, selectedTop);

            agents[c].network = agents[mother].network.MakeChild(agents[father].network);
        }

        for (int i = 0; i < nbAgents; i++)
        {
            agents[i].transform.localPosition = Vector3.zero;
            agents[i].transform.localRotation = Quaternion.identity;
            agents[i].dead = false;
        }

        generation += 1;
        StartCoroutine(UpdateAgents());
    }

    IEnumerator UpdateAgents()
    {
        bool allDead = false;
        int genDuration = 0;

        while (!allDead && genDuration<maxGenDuration)
        {
            genDuration++;
            allDead = true;

            foreach (MLAgent agent in agents)
            {
                agent.UpdateAgent();
                agent.CalculateScore();
                allDead = allDead && agent.dead;
            }

            if (updateRate == 0)
                yield return new WaitForEndOfFrame();
            else
                yield return new WaitForSecondsRealtime(1f / updateRate);
        }

        AdvanceGeneration();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            showOnlySelected = !showOnlySelected;

            for (int i = 0; i < agents.Count; i++)
            {
                if (showOnlySelected)
                    agents[i].model.SetActive(selectedAgent == i);
                else
                    agents[i].model.SetActive(true);
            }
        }

        if(Input.GetKeyDown(KeyCode.RightArrow))
            ChangeSelectedAgent(selectedAgent+1);

        if(Input.GetKeyDown(KeyCode.LeftArrow))
            ChangeSelectedAgent(selectedAgent-1);

        if (Input.GetKeyDown(KeyCode.End))
        {
            foreach (MLAgent agent in agents)
                agent.dead = true;
        }

        cam.transform.position = agents[selectedAgent].transform.position;
    }

    private void ChangeSelectedAgent(int agent)
    {
        if (agent < 0)
            agent = nbAgents - agent;
        else
            agent = agent % nbAgents;

        if (showOnlySelected)
        {
            agents[selectedAgent].model.SetActive(false);
            agents[agent].model.SetActive(false);
        }

        selectedAgent = agent;
    }

    private void OnGUI()
    {
        agents[selectedAgent].Draw();
    }
}
