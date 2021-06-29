using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Image img;
    void Start() { img = GetComponent<Image>(); }
    public void OnPointerDown(PointerEventData eventData)
    {
        TweeningLibrary.LerpColor(img, new Color32(27, 27, 27, 255), new Color32(32, 32, 32, 255), 0.1f);
        CarManager.car.throttle = 1;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TweeningLibrary.LerpColor(img, new Color32(32, 32, 32, 255), new Color32(27, 27, 27, 255), 0.1f);
        CarManager.car.throttle = 0;
    }
}
