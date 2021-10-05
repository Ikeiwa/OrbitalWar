using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LevelData", menuName = "OrbitalWar/Level", order = 1)]
public class LevelData : ScriptableObject
{
    public GameObject LevelPrefab;
    public Music music;
    public List<PathFinder.Node> graph;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelData))]
public class Example : Editor
{
    public override void OnInspectorGUI()
    {
        LevelData targetLevel = (LevelData)target;
        DrawDefaultInspector();

        if (targetLevel.graph == null)
            EditorGUILayout.LabelField("Please generate graph");
        else
            EditorGUILayout.LabelField("Graph nodes : " + targetLevel.graph.Count);

        
        if(GUILayout.Button("Compute Graph"))
        {
            GameObject levelObject = Instantiate(targetLevel.LevelPrefab);
            targetLevel.graph = PathFinder.GenerateGraph(levelObject.GetComponent<MeshFilter>().sharedMesh);
            DestroyImmediate(levelObject);

            EditorUtility.SetDirty(targetLevel);
        }
    }
}
#endif
