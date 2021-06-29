#ifndef VECTORS_H
#define VECTORS_H

struct Vector2
{
public:
	float x, y;

	Vector2() { x = 0; y = 0; }
	Vector2(float X, float Y)
	{
		x = X;
		y = Y;
	}
};

struct Vector3
{
public:
	float x, y, z;

	Vector3() { x = 0; y = 0; z = 0; }
	Vector3(float X, float Y, float Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};

struct Quaternion
{
public:
	float x, y, z, w;

	Quaternion() { x = 0; y = 0; z = 0; w = 0; }
	Quaternion(float X, float Y, float Z, float _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

#endif