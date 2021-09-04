using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
	public static Mesh GenerateTerrainMesh(float[] heightMap)
	{
		MeshData meshData = new MeshData(64, 64);
		int vertexIndex = 0;

		for (int x = 0; x < 64; x++)
		{
			for (int y = 0; y < 64; y++)
			{
				meshData.vertices[vertexIndex] = new Vector3(x, heightMap[vertexIndex], y);
				meshData.uvs[vertexIndex] = new Vector2(x, y);

				if (x < 63 && y < 63)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + 65, vertexIndex + 64);
					meshData.AddTriangle(vertexIndex + 65, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData.CreateMesh();
	}

	public static void GenerateProps(List<SaveFile.PropClass> propsInfo, PropDataObject[] propsData, GameObject parent)
    {
		for (int i = 0; i < parent.transform.childCount; i++)
			Destroy(parent.transform.GetChild(i));

		for (int i = 0; i < propsInfo.Count; i++)
        {
			Debug.Log(propsInfo[i].PropGroup + ", " + propsInfo[i].PropId);

			for (int j = 0; j < propsData.Length; j++)
            {
				if (propsInfo[i].PropGroup == propsData[j].Group && propsInfo[i].PropId == propsData[j].ID)
                {
					GameObject g = Instantiate(propsData[j].Prefabs[0], parent.transform);

					g.transform.localPosition = propsInfo[i].Pos;
					g.transform.eulerAngles = propsInfo[i].Euler;
					g.transform.localScale = propsInfo[i].Scale;
				}
            }
		}
    }
}

public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}
}