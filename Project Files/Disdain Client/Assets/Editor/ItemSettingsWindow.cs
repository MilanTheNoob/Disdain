using System.Collections;
using UnityEditor;
using UnityEngine;

public class ItemSettingsWindow : EditorWindow
{
    string Name;
    string parentName;
    GameObject g;

    [MenuItem("Editor Windows/Standard")]
    public static void ShowWindow()
    {
        GetWindow<ItemSettingsWindow>("Item Setting");
    }

    void OnGUI()
    {
        Name = EditorGUILayout.TextField("Name", Name);
        parentName = EditorGUILayout.TextField("Category", parentName);

        GUILayout.Label("\n");

        g = (GameObject)EditorGUILayout.ObjectField(g, typeof(GameObject), false);

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

            ItemSettings i = CreateInstance<ItemSettings>();

            i.name = name;
            i.icon = Resources.Load<Sprite>("Items/" + parentName + "/" + Name + "/" + projName);
            i.gameObject = Resources.Load<GameObject>("Items/" + parentName + "/" + Name + "/" + projName);

            GameObject newG = Resources.Load<GameObject>("Items/" + parentName + "/" + Name + "/" + projName);
            Interaction_InteractableItem newII = newG.AddComponent<Interaction_InteractableItem>();
            newII.itemSettings = i;

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
