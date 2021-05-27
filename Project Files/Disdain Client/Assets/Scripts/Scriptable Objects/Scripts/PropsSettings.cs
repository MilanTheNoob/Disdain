﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Generation/New Prop Settings")]
public class PropsSettings : ScriptableObject
{
    public PropsGroupStruct[] PropGroups;
    public BiomesStruct[] Biomes;

    [Space]

    public StructureChunk[] Structures;

    [Space]

    public GameObject[] Cars;

    [System.Serializable]
    public class PropsGroupStruct
    {
        public string Name;
        public PropsStruct[] Props;
    }

    [System.Serializable]
    public class PropsStruct
    {
        public string PropName;

        [Space]

        public GameObject[] PrefabVariants;
        
        [Space]

        public float YOffset = -0.2f;
        public Vector3 RotationClamp = new Vector3(5, 180, 5);

        [Space]

        public int PoolSizes = 200;
    }

    [System.Serializable]
    public class BiomesStruct
    {
        public string Name;
        public BiomePropStruct[] props;
    }
}

[System.Serializable]
public class BiomePropStruct
{
    public string Name;
    public int Group;
    public int Prop;

    [Space]

    public int PerChunk;

    [Space]

    public float SizeMax = 2.3f;
    public float SizeMin = 0.8f;
}

[System.Serializable]
public class StructureChunk
{
    public GameObject Structure;
    public BiomePropStruct[] Props;
}
