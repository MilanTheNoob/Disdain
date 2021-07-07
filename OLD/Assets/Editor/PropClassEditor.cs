using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PropClass))]
public class PropClassEditor : Editor
{
    PropClass propClass;
    void OnEnable() { propClass = (PropClass)target; }

    public override void OnInspectorGUI()
    {
        propClass.ID = EditorGUILayout.IntField("Prop ID", propClass.ID);

        EditorGUILayout.Space();

        propClass.MinRot = EditorGUILayout.Vector3Field("Min Rot", propClass.MinRot);
        propClass.MaxRot = EditorGUILayout.Vector3Field("Max Rot", propClass.MaxRot);

        EditorGUILayout.Space();

        propClass.MinScale = EditorGUILayout.FloatField("Minimum Scale", propClass.MinScale);
        propClass.MaxScale = EditorGUILayout.FloatField("Maximum Scale", propClass.MaxScale);

        EditorGUILayout.Space();

        propClass.UseBounds = EditorGUILayout.Toggle("Use Bounds", propClass.UseBounds);
        if (propClass.UseBounds) propClass.BoundsSize = EditorGUILayout.FloatField("Bounds Size", propClass.BoundsSize);

        EditorGUILayout.Space();

        propClass.GenerateType = (GenerateTypeEnum)EditorGUILayout.EnumPopup("Generate Type", propClass.GenerateType);

        switch (propClass.GenerateType)
        {
            case GenerateTypeEnum.Random:
                propClass.PerChunkMin = EditorGUILayout.IntField("Min Props Per Chunk", propClass.PerChunkMin);
                propClass.PerChunkMax = EditorGUILayout.IntField("Max Props Per Chunk", propClass.PerChunkMax);
                break;

            case GenerateTypeEnum.RandomChance:
                propClass.Chance = EditorGUILayout.FloatField("Min Chance Per Chunk", propClass.Chance);
                break;

            case GenerateTypeEnum.Perlin:
                propClass.PerlinChance = EditorGUILayout.FloatField("Perlin Chance Per Chunk", propClass.PerlinChance);
                break;
        }

        EditorUtility.SetDirty(propClass);
    }
}
