using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;
    public int currentLevel = 0;

    private GameObject levelObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadLevel()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int lvl)
    {
        if (levelObject != null)
            Destroy(levelObject);

        levelObject = Instantiate(levels[lvl].LevelPrefab);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
