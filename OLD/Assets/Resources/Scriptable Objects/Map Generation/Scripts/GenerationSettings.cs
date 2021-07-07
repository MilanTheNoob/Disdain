using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Generation Settings")]
public class GenerationSettings : ScriptableObject
{
	[Header("Biomes")]
	public List<BiomeClass> Biomes;
	[Header("Base Classes")]
	public List<HeightMapSettings> HeightMaps;
	public MeshSettings MeshSetting;

	public BiomeClass GetBiome(Vector2 coord, Noise noise)
    {
		float rainfall = noise.Generate(coord.x, coord.y);

		for (int i = 0; i < Biomes.Count; i++)
        {
			if (rainfall > Biomes[i].MinRainfall && rainfall <= Biomes[i].MaxRainFall)
            {
				Debug.Log(i);
				return Biomes[i];
            }
        }

		return null;
    }

	public float GetBiomeWeight(Vector2 chunk, Vector2 coord, BiomeClass biome)
    {
		return Mathf.Clamp(Mathf.Abs(biome.RainfallCenter - Mathf.PerlinNoise(chunk.x /** MeshSetting.meshScale*/ + coord.x, chunk.x /** MeshSetting.meshScale*/ + coord.y)), 0f, 1f);
	}
}

[System.Serializable]
public class HeightMapSettings
{
	public string Name;
	public NoiseSettings NoiseSetting;

	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public bool ChanceUse;
	[Range(0, 1)]
	public float UseChance;

	public float minHeight { get { return heightMultiplier * heightCurve.Evaluate(0); } }
	public float maxHeight { get { return heightMultiplier * heightCurve.Evaluate(1); } }

	[System.Serializable]
	public class NoiseSettings
	{
		public float scale = 50;

		public int octaves = 6;
		[Range(0, 1)]
		public float persistance = .6f;
		public float lacunarity = 2;

		public int seed;
		public Vector2 offset;

		public void ValidateValues()
		{
			scale = Mathf.Max(scale, 0.01f);
			octaves = Mathf.Max(octaves, 1);
			lacunarity = Mathf.Max(lacunarity, 1);
			persistance = Mathf.Clamp01(persistance);
		}
	}
}

[System.Serializable]
public class MeshSettings
{
	public float ySize = 75f;
	public float meshScale = 2.5f;
	public int ChunkSize = 64;

	public int numVertsPerLine { get { return ChunkSize + 5; } }
	public float meshWorldSize { get { return (numVertsPerLine - 3) * meshScale; } }
}

[System.Serializable]
public class PropTypeClass
{
	public string ID;
	public PropClass[] Props;
}

[System.Serializable]
public class BiomeClass
{
	public string BiomeName;

	[Header("Rainfall Values")]
	public float RainfallCenter;

	[Space]

	public float MinRainfall;
	public float MaxRainFall;

	[Header("Prop Settings")]
	public PropTypeClass[] PropTypes;

	[Header("Heightmaps")]
	public List<HeightMapSettings> HeightMaps;
}

public class BiomeData
{
	public BiomeClass Biome;
	public float Weight;
}
