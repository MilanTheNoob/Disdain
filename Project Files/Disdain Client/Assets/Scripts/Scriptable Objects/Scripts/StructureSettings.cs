using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Generation/New Structure Settings")]
public class StructureSettings : ScriptableObject
{
    public List<StructureStruct> structureObjects = new List<StructureStruct>();

    public void AddObject(string objName, Mesh objMesh, Vector3 objPos, Quaternion objRot, Vector3 objScale, Material objMat)
    {
        StructureStruct structureStruct = new StructureStruct();

        structureStruct.objectName = objName;
        structureStruct.objectMesh = objMesh;
        structureStruct.objectPosition = objPos;
        structureStruct.objectRotation = objRot;
        structureStruct.objectScale = objScale;
        structureStruct.objectMaterial = objMat;

        structureObjects.Add(structureStruct);
    }

    [System.Serializable]
    public class StructureStruct
    {
        public string objectName;
        public Mesh objectMesh;
        public Vector3 objectPosition;
        public Quaternion objectRotation;
        public Vector3 objectScale;
        public Material objectMaterial;
    }
}
