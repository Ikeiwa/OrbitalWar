using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmooth : MonoBehaviour
{
    public Player target;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 8);
        transform.position = Vector3.Lerp(transform.position, Vector3.Lerp(target.transform.position, target.firePos, 0.25f), Time.deltaTime * 10);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, Time.deltaTime * 5);
    }
}
