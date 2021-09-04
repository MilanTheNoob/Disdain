#ifndef vectorS_H
#define vectorS_H

struct vector2
{
	float x, y;

	vector2() { x = 0; y = 0; }
	vector2(float X, float Y)
	{
		x = X;
		y = Y;
	}
};
struct vector3
{
	float x, y, z;

	vector3() { x = 0; y = 0; z = 0; }
	vector3(float X, float Y, float Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};
struct quaternion
{
	float x, y, z, w;

	quaternion() { x = 0; y = 0; z = 0; w = 0; }
	quaternion(float X, float Y, float Z, float _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

struct vector2_8
{
	uint8_t x;
	uint8_t y;

	vector2_8() { x = 0; y = 0; }
	vector2_8(uint8_t X, uint8_t Y)
	{
		x = X;
		y = Y;
	}
};
struct vector3_8
{
	uint8_t x;
	uint8_t y;
	uint8_t z;

	vector3_8() { x = 0; y = 0; z = 0; }
	vector3_8(uint8_t X, uint8_t Y, uint8_t Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};
struct quaternion_8
{
	uint8_t x, y, z, w;

	quaternion_8() { x = 0; y = 0; z = 0; w = 0; }
	quaternion_8(uint8_t X, uint8_t Y, uint8_t Z, uint8_t _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

struct vector2_16
{
	uint16_t x, y;

	vector2_16() { x = 0; y = 0; }
	vector2_16(uint16_t X, uint16_t Y)
	{
		x = X;
		y = Y;
	}
};
struct vector3_16
{
	uint16_t x, y, z;

	vector3_16() { x = 0; y = 0; z = 0; }
	vector3_16(uint16_t X, uint16_t Y, uint16_t Z)
	{
		x = X;
		y = Y;
		z = Z;
	}
};
struct quaternion_16
{
	uint16_t x, y, z, w;

	quaternion_16() { x = 0; y = 0; z = 0; w = 0; }
	quaternion_16(uint16_t X, uint16_t Y, uint16_t Z, uint16_t _w)
	{
		x = X;
		y = Y;
		z = Z;
		w = _w;
	}
};

#endif