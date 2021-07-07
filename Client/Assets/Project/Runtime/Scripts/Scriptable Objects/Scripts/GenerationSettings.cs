using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Generation Settings")]
public class GenerationSettings : ScriptableObject
{
	[Header("Prop Settings used for Single & Multi Player")]
	public PropSettingsStruct PropSettings;
	[Header("Mesh Settings used for Lobby & Login")]
	public MeshSettingsStruct MeshSetting;
}

[System.Serializable]
public class PropSettingsStruct
{
	public PropTypeClass[] PropTypes;
	public int PoolSizes = 500;

	[System.Serializable]
	public class PropTypeClass
    {
		public string ID;
		public PropClass[] Props;
    }

	[System.Serializable]
	public class PropClass
    {
		public int ID;
		public GameObject[] Prefabs;
    }
}

[System.Serializable]
public class MeshSettingsStruct
{
	public float ySize = 75f;
	public float meshScale = 2.5f;
	public bool useFlatShading;
	public int ChunkSize = 16;

	public int numVertsPerLine { get { return ChunkSize + 5; } }
	public float meshWorldSize { get { return (numVertsPerLine - 3) * meshScale; } }
}
