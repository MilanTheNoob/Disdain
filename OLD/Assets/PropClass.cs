using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Prop Class")]
public class PropClass : ScriptableObject
{
	public int ID;

	[Space]

	public Vector3 MinRot;
	public Vector3 MaxRot;

	[Space]

	public float MinScale;
	public float MaxScale;

	[Space]

	public bool UseBounds;
	public float BoundsSize;

	[Space]

	public GenerateTypeEnum GenerateType = GenerateTypeEnum.Random;

	// GTE - Random
	public int PerChunkMin;
	public int PerChunkMax;

	// GTE - Random Chance
	[Range(0f, 1f)]
	public float Chance;

	// GTE - Perlin
	public float PerlinChance;
}

public enum GenerateTypeEnum
{
	Random,
	RandomChance,
	Perlin
}