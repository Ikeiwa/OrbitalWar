using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ((RectTransform)transform).anchoredPosition = new Vector2(Mathf.Clamp(time-MusicManager.beatTime,0,5)*50, 0);
        if (MusicManager.beatTime > time)
            Destroy(gameObject);
    }
}
