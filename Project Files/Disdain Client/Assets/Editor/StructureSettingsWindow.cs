using System.Collections;
using UnityEditor;
using UnityEngine;

public class StructureSettingsWindow : EditorWindow
{
    string Name;
    string parentName;
    GameObject g;
    int structureType;

    [MenuItem("Editor Windows/Structure Settings")]
    public static void ShowWindow()
    {
        GetWindow<StructureSettingsWindow>("Structure Settings");
    }

    void OnGUI()
    {
        Name = EditorGUILayout.TextField("Name", Name);
        parentName = EditorGUILayout.TextField("Category", parentName);

        GUILayout.Label("\n");

        g = (GameObject)EditorGUILayout.ObjectField(g, typeof(GameObject), false);

        GUILayout.Label("\n");
        GUILayout.Label("1 - Foundation\n2 - Wall\n3 - Furniture\n4 - Storage");
        structureType = EditorGUILayout.IntField("", structureType);

        if (GUILayout.Button("Create"))
        {
            string projName = Name.Replace(" ", "-").ToLower();
            AssetDatabase.CreateFolder("Assets/Resources/Items/" + parentName, Name);

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(g), "Assets/Resources/Items/" + parentName + "/" + Name + "/" + projName + ".prefab");
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(g));

            AssetDatabase.CopyAsset("Assets/Resources/100px Square.png", "Assets/Resources/Items/" + parentName + "/" + Name + "/" + projName + ".png");

            g = null;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            StructureItemSettings i = CreateInstance<StructureItemSettings>();

            i.name = name;
            i.icon = Resources.Load<Sprite>("Items/" + parentName + "/" + Name + "/" + projName);
            i.gameObject = Resources.Load<GameObject>("Items/" + parentName + "/" + Name + "/" + projName);

            GameObject newG = Resources.Load<GameObject>("Items/" + parentName + "/" + Name + "/" + projName);
            BuildPreview newBP = newG.AddComponent<BuildPreview>();

            if (structureType == 1) { newBP.structureType = BuildPreview.StructureType.Foundation; }
            else if (structureType == 2) { newBP.structureType = BuildPreview.StructureType.Wall; }
            else if (structureType == 3) { newBP.structureType = BuildPreview.StructureType.Furniture; }
            else if (structureType == 4) { newBP.structureType = BuildPreview.StructureType.Storage; }

            if (newG.GetComponent<MeshCollider>() == null && newG.GetComponent<MeshFilter>() == null)
            {
                MeshCollider c = newG.AddComponent<MeshCollider>();
                c.sharedMesh = newG.GetComponent<MeshFilter>().sharedMesh;
            }

            AssetDatabase.CreateAsset(i, "Assets/Resources/Interactables/" + projName + ".asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
