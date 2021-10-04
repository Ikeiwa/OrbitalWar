using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ((RectTransform)transform).anchoredPosition = Input.mousePosition;
    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
            Cursor.visible = false;
    }
}
