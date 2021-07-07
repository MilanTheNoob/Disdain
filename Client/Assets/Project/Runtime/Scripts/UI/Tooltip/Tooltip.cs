using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Tooltip : MonoBehaviour
{
    public Text Header;
    public Text Content;

    [Space]

    public LayoutElement Layout;
    public int WrapLimit;

    [Space]

    public RectTransform RectTransform;

    void Update()
    {
        int headerLength = Header.text.Length;
        int contentLength = Content.text.Length;

        Layout.enabled = (headerLength > WrapLimit || contentLength > WrapLimit) ? true : false;

        Vector2 pos = Input.mousePosition;
        /*
        float pivotX = pos.x / Screen.width;
        float pivotY = pos.y / Screen.height;

        RectTransform.pivot = new Vector2(pivotX, pivotY);
        */
        transform.position = pos;
    }    
}
