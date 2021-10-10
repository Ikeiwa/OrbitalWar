using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergyFollower : Enemy
{
    public GameObject explosionPrefab;
    public AudioClip deathClip;
    public float speed = 1;
    public float health = 20;

    List<PathFinder.Node> path;
    int currentNode = 1;

    Player player;

    public override void ApplyDamage(float damages)
    {
        health -= damages;
        if (health <= 0)
            Explode();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        if(path != null && path.Count>=2)
        {
            if (currentNode < path.Count)
            {
                PathFinder.Node target = path[currentNode];
                PathFinder.Node current = path[currentNode-1];

                float trueSpeed = speed * MusicManager.energy;

                if (target.type == 2 || current.type == 2)
                    trueSpeed *= 2;
                else if (target.type == 1 || current.type == 1)
                    trueSpeed /= 2;

                    transform.Translate((target.position - transform.position).normalized * trueSpeed * Time.deltaTime, Space.World);


                Quaternion endRot = Quaternion.LookRotation(target.position - transform.position, target.up);


                transform.rotation = Quaternion.Lerp(transform.rotation, endRot, Time.deltaTime* trueSpeed);

                if((target.position- transform.position).sqrMagnitude < 0.1f)
                    currentNode++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GetComponent<LineRenderer>().enabled = !GetComponent<LineRenderer>().enabled;
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            Vector3 playerPos = player.transform.position + player.transform.up * 0.5f;
            PathFinder.Node start = PathFinder.instance.GetClosestNode(transform.position);
            PathFinder.Node end = PathFinder.instance.GetClosestNode(playerPos);
            yield return PathFinder.instance.GetShortestPath(start, end,(newPath) => { path = newPath; } ,2);

            if(path != null)
            {
                if ((playerPos - path.Last().position).sqrMagnitude < 1)
                {
                    path.Add(new PathFinder.Node { position = playerPos, up = player.transform.up });
                }

                currentNode = 1;

                List<Vector3> positions = new List<Vector3>();
                foreach (PathFinder.Node node in path)
                    positions.Add(node.position);

                GetComponent<LineRenderer>().positionCount = positions.Count;
                GetComponent<LineRenderer>().SetPositions(positions.ToArray());
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.GetComponent<IDamageable>()?.ApplyDamage(5);
            Explode();
        }
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(deathClip, transform.position);
        Destroy(gameObject);
    }
}
