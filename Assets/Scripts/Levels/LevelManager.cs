using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;
    public int currentLevel = 0;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;


    private GameObject player;
    private GameObject levelObject;
    private PathFinder pathFinder;

    // Start is called before the first frame update
    void Start()
    {
        pathFinder = GetComponent<PathFinder>();
        LoadLevel();
    }

    [ContextMenu("Load Level")]
    public void LoadLevel()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int lvl)
    {
        if (!player)
            player = Instantiate(playerPrefab);

        if (levelObject != null)
            DestroyImmediate(levelObject);

        levelObject = Instantiate(levels[lvl].LevelPrefab);
        pathFinder = GetComponent<PathFinder>();
        pathFinder.GenerateGraph(levelObject.GetComponent<MeshFilter>().mesh);
        player.transform.position = levelObject.transform.Find("PlayerStart").position;
        player.transform.rotation = levelObject.transform.Find("PlayerStart").rotation;

        for(int i=0; i < pathFinder.graph.Count / 32; i++)
        {
            PathFinder.Node node = pathFinder.graph[Random.Range(0, pathFinder.graph.Count - 1)];
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.transform.position = node.position;
            enemy.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), node.up);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
