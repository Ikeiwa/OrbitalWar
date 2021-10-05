using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    private Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ((RectTransform)transform).localPosition = ScreenToCanvasPosition(canvas,Input.mousePosition);
    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
            Cursor.visible = false;
    }

    public Vector3 ScreenToCanvasPosition(Canvas canvas, Vector3 screenPosition)
    {
        var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                           screenPosition.y / Screen.height,
                                           0);
        return ViewportToCanvasPosition(canvas,viewportPosition);
    }

    public Vector3 ViewportToCanvasPosition(Canvas canvas, Vector3 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector3.Scale(centerBasedViewPortPosition, scale);
    }
}
