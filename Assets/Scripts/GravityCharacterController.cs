using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityCharacterController : MonoBehaviour
{
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    public float moveSpeed = 2f;

    new private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider;

    private bool onGround = false;
    private Vector3 inputDir;
    private Vector3 gravity = Vector3.up;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        ProcessInput();
    }

    private void ProcessInput()
    {
        Vector3 direction = transform.TransformDirection(inputDir);
        Vector3 move = direction * moveSpeed * Time.deltaTime;
        //rigidbody.MovePosition(transform.position + move);
        rigidbody.AddForce(move*10, ForceMode.Force);
        Debug.DrawRay(transform.position + transform.up * 0.25f, direction * 2,Color.green);
    }

    private void CheckGrounded()
    {
        onGround = false;
        float capsuleHeight = Mathf.Max(capsuleCollider.radius * 2f, capsuleCollider.height);
        Vector3 capsuleBottom = transform.TransformPoint(capsuleCollider.center - Vector3.up * capsuleHeight / 2f);
        float radius = transform.TransformVector(capsuleCollider.radius, 0f, 0f).magnitude;

        Ray ray = new Ray(capsuleBottom + transform.up * .01f, -transform.up*2);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, radius * 5f,1<<6 ,QueryTriggerInteraction.Ignore))
        {

            float normalAngle = Vector3.Angle(hit.normal, transform.up);
            if (normalAngle < slopeLimit)
            {
                float maxDist = radius / Mathf.Cos(Mathf.Deg2Rad * normalAngle) - radius + .02f;
                if (hit.distance < maxDist)
                    onGround = true;
            }
        }

        if (hit.collider != null)
        {
            Vector3 normal = GetSmoothedNormal(hit);

            Debug.DrawRay(transform.position, normal * 2);

            Vector3 projForward = Vector3.ProjectOnPlane(transform.forward, normal);
            Quaternion angle = Quaternion.FromToRotation(transform.up, normal);
            Quaternion q = angle*transform.rotation;
            //q = Quaternion.Lerp(transform.rotation, q, Time.fixedDeltaTime * 15);
            rigidbody.MoveRotation(q);

            rigidbody.velocity = angle*rigidbody.velocity;

            gravity = hit.normal;

            //Debug.DrawRay(transform.position, q * Vector3.up * 2, Color.red);
        }

        rigidbody.AddForce(-gravity * 9.81f, ForceMode.Acceleration);
    }

    public void Move(Vector3 direction)
    {
        inputDir = direction;
    }

    public static Vector3 GetSmoothedNormal(RaycastHit hit)
    {
        var meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null)
            return hit.normal;
        var mesh = meshCollider.sharedMesh;
        if (mesh.isReadable)
        {
            var p0 = mesh.triangles[hit.triangleIndex * 3 + 0];
            var p1 = mesh.triangles[hit.triangleIndex * 3 + 1];
            var p2 = mesh.triangles[hit.triangleIndex * 3 + 2];


            var N0 = mesh.normals[p0];
            var N1 = mesh.normals[p1];
            var N2 = mesh.normals[p2];
            var B = hit.barycentricCoordinate;
            var localNormal = (B[0] * N0 + B[1] * N1 + B[2] * N2).normalized;


            /*Debug.DrawRay(mesh.vertices[p0], mesh.normals[p0], Color.red);
            Debug.DrawRay(mesh.vertices[p1], mesh.normals[p1], Color.red);
            Debug.DrawRay(mesh.vertices[p2], mesh.normals[p2], Color.red);*/

            return meshCollider.transform.TransformDirection(localNormal);
        }
        return hit.normal;
    }
}
