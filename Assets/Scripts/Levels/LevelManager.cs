using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;
    public int currentLevel = 0;
    public GameObject playerPrefab;
    public GameObject bounds;

    public EnemySpawner enemySpawner;
    public Enemy[] enemies;

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

    private Bounds GetLevelBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        MeshRenderer[] levelRenderers = levelObject.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < levelRenderers.Length; i++)
        {
            bounds.Encapsulate(levelRenderers[i].bounds);
        }

        return bounds;
    }

    public void LoadLevel(int lvl)
    {
        if (!player)
            player = Instantiate(playerPrefab);

        if (levelObject != null)
        {
            DestroyImmediate(levelObject);
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                DestroyImmediate(enemy.gameObject);
        }
            

        levelObject = Instantiate(levels[lvl].LevelPrefab);
        pathFinder = GetComponent<PathFinder>();
        pathFinder.GenerateGraph(levelObject.GetComponent<MeshFilter>().mesh);
        player.transform.position = levelObject.transform.Find("PlayerStart").position;
        player.transform.rotation = levelObject.transform.Find("PlayerStart").rotation;

        for(int i=0; i < pathFinder.graph.Count / 32; i++)
        {
            PathFinder.Node node = pathFinder.graph[Random.Range(0, pathFinder.graph.Count - 1)];
            EnemySpawner enemy = Instantiate(enemySpawner);
            enemy.transform.position = node.position;
            enemy.transform.rotation = Quaternion.FromToRotation(enemy.transform.up, node.up) * enemy.transform.rotation;
            enemy.enemyPrefab = enemies[Random.Range(0, enemies.Length)];
        }

        Vector3 levelSize = GetLevelBounds().size;
        float largestSize = Mathf.Max(levelSize.x, levelSize.y, levelSize.z);

        bounds.transform.localScale = new Vector3(largestSize, largestSize, largestSize) * 1.5f;

        MusicManager.Instance.Play(levels[lvl].music);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            LoadLevel(0);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Keypad4))
            LoadLevel(3);
        if (Input.GetKeyDown(KeyCode.Keypad5))
            LoadLevel(4);
    }
}
