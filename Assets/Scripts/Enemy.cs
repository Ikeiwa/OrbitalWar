using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1;

    List<PathFinder.Node> path;
    int currentNode = 1;

    Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        if(path != null && path.Count>2)
        {
            if (currentNode < path.Count)
            {
                PathFinder.Node target = path[currentNode];
                PathFinder.Node current = path[currentNode-1];


                transform.Translate((target.position - transform.position).normalized * speed * Time.deltaTime, Space.World);


                Quaternion endRot = Quaternion.LookRotation(target.position - current.position, target.up);


                transform.rotation = Quaternion.Lerp(transform.rotation, endRot, Time.deltaTime* speed);

                if((target.position- transform.position).sqrMagnitude < 0.32f)
                    currentNode++;
            }
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            PathFinder.Node start = PathFinder.instance.GetClosestNode(transform.position);
            PathFinder.Node end = PathFinder.instance.GetClosestNode(player.transform.position+player.transform.up);
            yield return PathFinder.instance.GetShortestPath(start, end,(newPath) => { path = newPath; } ,1);
            currentNode = 1;

            List<Vector3> positions = new List<Vector3>();
            foreach (PathFinder.Node node in path)
                positions.Add(node.position);

            GetComponent<LineRenderer>().positionCount = positions.Count;
            GetComponent<LineRenderer>().SetPositions(positions.ToArray());

            yield return new WaitForSeconds(1);
        }
    }
}
