using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Scriptable Objects/Map Generation/New Structures Settings")]
public class StructuresSettings : ScriptableObject
{
    [Header("Structure Settings")]
    public int pool;
    public int perChunk;

    [Header("Structures")]
    public StructuresStruct[] StandardBuildings;

    [System.Serializable]
    public class StructuresStruct
    {
        public string name; 
        public GameObject structure;
        public float yOffset;
    }
}
