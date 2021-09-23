using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform head;
    private GravityCharacterController controller;

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
        Vector3 mousePos = mouseRay.GetPoint(hitDist);

        head.LookAt(mousePos, transform.up);
    }
}
