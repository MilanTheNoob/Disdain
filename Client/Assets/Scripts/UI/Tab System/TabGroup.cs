using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public GameObject[] objectsToSwap;

    public void SwitchTabs(int index)
    {
        AudioManager.PlayClick();
        for (int i = 0; i < objectsToSwap.Length; i++)
        {
            if (i == index && !objectsToSwap[i].activeSelf)
            {
                TweeningLibrary.FadeIn(objectsToSwap[i], 0.1f);
            }
            else if (i != index)
            {
                TweeningLibrary.FadeOut(objectsToSwap[i], 0.1f);
            }
        }
    }
}
