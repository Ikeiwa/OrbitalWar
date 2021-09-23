using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmooth : MonoBehaviour
{
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        target = transform.parent;
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 8);
        transform.position = target.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * 5);
    }
}
