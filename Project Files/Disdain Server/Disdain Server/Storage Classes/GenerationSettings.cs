using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class GenerationSettings
{
	public List<BiomeClass> Biomes;

	public List<HeightMapSettings> HeightMaps;
	public MeshSettings MeshSetting;

	public BiomeClass GetBiome(Vector2 coord, Noise noise)
	{
		float rainfall = noise.Generate(coord.X, coord.Y);

		for (int i = 0; i < Biomes.Count; i++)
		{
			if (rainfall > Biomes[i].MinRainfall && rainfall <= Biomes[i].MaxRainFall)
			{
				Console.WriteLine(i);
				return Biomes[i];
			}
		}

		return null;
	}

	public float GetBiomeWeight(Vector2 chunk, Vector2 coord, BiomeClass biome, Noise noise)
	{
		MathF 
		return Math.Clamp(Math.Abs(biome.RainfallCenter - Mathf.PerlinNoise(chunk.X * MeshSetting.meshScale + coord.X, chunk.Y * MeshSetting.meshScale + coord.Y)), 0f, 1f);
	}
}

public class HeightMapSettings
{
	public string Name;
	public NoiseSettings NoiseSetting;

	public float heightMultiplier;

	public bool ChanceUse;
	public float UseChance;

	public class NoiseSettings
	{
		public float scale = 50;

		public int octaves = 6;
		public float persistance = .6f;
		public float lacunarity = 2;

		public int seed;
		public Vector2 offset;

		public void ValidateValues()
		{
			scale = Math.Max(scale, 0.01f);
			octaves = Math.Max(octaves, 1);
			lacunarity = Math.Max(lacunarity, 1);
			persistance = Math.Clamp01(persistance);
		}
	}
}

public class MeshSettings
{
	public float ySize = 75f;
	public float meshScale = 2.5f;
	public int ChunkSize = 64;

	public int numVertsPerLine { get { return ChunkSize + 5; } }
	public float meshWorldSize { get { return (numVertsPerLine - 3) * meshScale; } }
}

public class PropTypeClass
{
	public string ID;
	public PropClass[] Props;
}

public class BiomeClass
{
	public string BiomeName;
	public float RainfallCenter;

	public float MinRainfall;
	public float MaxRainFall;

	public PropTypeClass[] PropTypes;
	public List<HeightMapSettings> HeightMaps;
}

public class BiomeData
{
	public BiomeClass Biome;
	public float Weight;
}