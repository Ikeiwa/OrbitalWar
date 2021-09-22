using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;
    public int currentLevel = 0;
    public GameObject playerPrefab;


    private GameObject player;
    private GameObject levelObject;

    // Start is called before the first frame update
    void Start()
    {
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
        GetComponent<PathFinder>().GenerateGraph(levelObject.GetComponent<MeshFilter>().mesh);
        player.transform.position = levelObject.transform.Find("PlayerStart").position;
        player.transform.rotation = levelObject.transform.Find("PlayerStart").rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
