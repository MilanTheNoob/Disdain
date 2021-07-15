#ifndef VECTORS_H
#define VECTORS_H

struct Vector2
{
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

struct Vector2_8
{
	uint8_t x;
	uint8_t y;

	Vector2_8() { x = 0; y = 0; }
	Vector2_8(uint8_t X, uint8_t Y)
	{
		x = X;
		y = Y;
	}
};
struct Vector3_8
{
	uint8_t x;
	uint8_t y;
	uint8_t z;

	Vector3_8() { x = 0; y = 0; z = 0; }
	Vector3_8(uint8_t X, uint8_t Y, uint8_t Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};
struct Quaternion_8
{
	uint8_t x, y, z, w;

	Quaternion_8() { x = 0; y = 0; z = 0; w = 0; }
	Quaternion_8(uint8_t X, uint8_t Y, uint8_t Z, uint8_t _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

struct Vector2_16
{
	uint16_t x, y;

	Vector2_16() { x = 0; y = 0; }
	Vector2_16(uint16_t X, uint16_t Y)
	{
		x = X;
		y = Y;
	}
};
struct Vector3_16
{
	uint16_t x, y, z;

	Vector3_16() { x = 0; y = 0; z = 0; }
	Vector3_16(uint16_t X, uint16_t Y, uint16_t Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};
struct Quaternion_16
{
	uint16_t x, y, z, w;

	Quaternion_16() { x = 0; y = 0; z = 0; w = 0; }
	Quaternion_16(uint16_t X, uint16_t Y, uint16_t Z, uint16_t _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

#endif