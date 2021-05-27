using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Generation/New Height Map Data")]
public class HeightMapSettings : ScriptableObject
{
    public NoiseSettings noiseSettings;

    [Header("Elevation Settings")]
    public float heightMultiplier;
    public AnimationCurve heightCurve;


    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }
}
