using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeatCannon : Enemy
{
    public enum State
    {
        Idle,
        Chase,
        Fire
    }

    public GameObject explosionPrefab;
    public Projectile projectilePrefab;
    public Transform firePoint;
    public AudioClip deathClip;
    public float speed = 1;
    public float health = 20;
    public float awareDistance = 6;
    public float fireDistance = 4;
    public float lostDistance = 8;

    List<PathFinder.Node> path;
    int currentNode = 1;
    private State currentState = State.Idle;
    private float playerDistance = float.MaxValue;

    Player player;
    private LineRenderer lineRenderer;


    public override void ApplyDamage(float damages)
    {
        health -= damages;
        if (health <= 0)
            Explode();
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        player = FindObjectOfType<Player>();
        MusicManager.Instance.OnBeat.AddListener(OnBeat);
        StartCoroutine(UpdatePath());
    }

    private void OnBeat()
    {
        if (currentState == State.Fire)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerDistance);
        switch (currentState)
        {
            case State.Idle:
                if (playerDistance < awareDistance)
                    currentState = State.Chase;
                break;
            case State.Chase:
                if (path != null && path.Count >= 2)
                {
                    Move();
                }

                if (playerDistance < fireDistance)
                    currentState = State.Fire;
                else if (playerDistance > lostDistance)
                    currentState = State.Idle;

                break;
            case State.Fire:
                Vector3 lookDir = player.transform.position - transform.position;
                lookDir = Vector3.ProjectOnPlane(lookDir, transform.up);
                transform.rotation = Quaternion.LookRotation(lookDir, transform.up);

                if (playerDistance > lostDistance)
                    currentState = State.Idle;
                else if (playerDistance > fireDistance)
                    currentState = State.Chase;

                break;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            lineRenderer.enabled = !lineRenderer.enabled;
        }
    }

    private void Move()
    {
        if (currentNode >= path.Count) return;

        PathFinder.Node target = path[currentNode];
        PathFinder.Node current = path[currentNode - 1];

        float trueSpeed = speed * MusicManager.energy;

        if (target.type == 2 || current.type == 2)
            trueSpeed *= 2;
        else if (target.type == 1 || current.type == 1)
            trueSpeed /= 2;

        transform.Translate((target.position - transform.position).normalized * (trueSpeed * Time.deltaTime), Space.World);


        Quaternion endRot = Quaternion.LookRotation(target.position - transform.position, target.up);


        transform.rotation = Quaternion.Lerp(transform.rotation, endRot, Time.deltaTime * trueSpeed);

        if ((target.position - transform.position).sqrMagnitude < 0.1f)
            currentNode++;
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

                playerDistance = PathFinder.GetPathLengthSqrt(path);

                currentNode = 1;

                List<Vector3> positions = new List<Vector3>();
                foreach (PathFinder.Node node in path)
                    positions.Add(node.position);

                if (playerDistance < fireDistance)
                    lineRenderer.startColor = lineRenderer.endColor = Color.red;
                else if (playerDistance < awareDistance)
                    lineRenderer.startColor = lineRenderer.endColor = Color.yellow;
                else
                    lineRenderer.startColor = lineRenderer.endColor = Color.green;

                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPositions(positions.ToArray());
            }

            yield return new WaitForSeconds(1);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        other.GetComponent<IDamageable>()?.ApplyDamage(5);
        Explode();
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(deathClip, transform.position);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,awareDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,lostDistance);
    }
}
