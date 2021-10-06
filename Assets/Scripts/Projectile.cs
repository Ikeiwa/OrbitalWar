using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask blockLayers;
    public LayerMask entityLayers;
    public GameObject explosionPrefab;

    public float damages = 10;
    public float speed = 3;
    public float lifetime = 3;

    private List<Vector3> trajectory;
    private float trajPosition = 0;

    private float step;
    private float subSteps = 5;

    public void CalculateTrajectory()
    {
        trajectory = new List<Vector3>();

        float distance = 0;
        int iterations = 1;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        trajectory.Add(position);

        float targetDistance = (speed * lifetime);

        while(distance < targetDistance && iterations < 100)
        {
            Vector3 lastPos = trajectory[iterations - 1];
            position += rotation * Vector3.forward * step;

            if (Physics.SphereCast(lastPos, 0.05f, position - lastPos, out RaycastHit hit, step, blockLayers))
            {
                trajectory.Add(hit.point);
                break;
            }

            if (Physics.Raycast(position,rotation*Vector3.down*4,out hit,4,blockLayers))
            {
                Vector3 normal =GravityCharacterController.GetSmoothedNormal(hit);

                Quaternion angle = Quaternion.FromToRotation(rotation*Vector3.up, normal);
                rotation = angle*rotation;
                position = hit.point + normal * 0.5f;
            }

            distance += (position - lastPos).magnitude;
            iterations++;
            trajectory.Add(position);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        step = speed / subSteps;
        CalculateTrajectory();
    }

    // Update is called once per frame
    void Update()
    {
        trajPosition += speed * subSteps * Time.deltaTime;
        if (trajPosition >= trajectory.Count - 1)
        {
            Explode();
        }
        else
        {
            int currentNode = Mathf.FloorToInt(trajPosition);
            int nextNode = currentNode+1;
            float pathPercent = trajPosition - currentNode;

            transform.position = Vector3.Lerp(trajectory[currentNode], trajectory[nextNode], pathPercent);
            transform.LookAt(trajectory[nextNode]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entityLayers.value == (entityLayers.value | 1 << other.gameObject.layer)){
            other.gameObject.GetComponent<IDamageable>()?.ApplyDamage(damages);
            Explode();
        }
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        CalculateTrajectory();

        if(trajectory != null && trajectory.Count>1)
        {
            for(int i = 0; i < trajectory.Count - 1; i++)
            {
                Gizmos.DrawWireSphere(trajectory[i], 0.1f);
                Gizmos.DrawLine(trajectory[i], trajectory[i + 1]);
            }
            Gizmos.DrawWireSphere(trajectory.Last(), 0.1f);
        }
    }
}
