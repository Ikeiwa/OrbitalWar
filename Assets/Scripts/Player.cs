using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public Transform head;
    public Transform[] firePoints;
    public GameObject projectilePrefab;
    private GravityCharacterController controller;

    public float health = 100;
    public float maxHealth = 100;

    public Vector3 firePos;

    public void ApplyDamage(float damages)
    {

    }

    private void Awake()
    {
        controller = GetComponent<GravityCharacterController>();
    }

    private void FixedUpdate()
    {
        Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")), 1);
        controller.Move(inputDir);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Plane groundplane = new Plane(transform.up, head.position);
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        groundplane.Raycast(mouseRay, out float hitDist);
        firePos = mouseRay.GetPoint(hitDist);

        head.LookAt(firePos, transform.up);

        if (Input.GetMouseButtonDown(0))
        {
            foreach(Transform firePoint in firePoints)
            {
                GameObject projectile = Instantiate(projectilePrefab);
                projectile.transform.position = firePoint.position;
                projectile.transform.rotation = firePoint.rotation;
            }
            
        }
    }
}
