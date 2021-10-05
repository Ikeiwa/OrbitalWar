using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "OrbitalWar/Level", order = 1)]
public class LevelData : ScriptableObject
{
    public GameObject LevelPrefab;
    public Music music;
}
