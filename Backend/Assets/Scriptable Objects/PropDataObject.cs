using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Create a new PropDataObject")]
public class PropDataObject : ScriptableObject
{
    public byte Group;
    public byte ID;

    [Space]

    public GameObject[] Prefabs;

    [Space]

    public string PropName;
    public string PropDescription;

    [Space]

    public Sprite PropImg;
}