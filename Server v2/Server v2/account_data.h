#ifndef ACCOUNT_DATA_H
#define ACCOUNT_DATA_H

#include <string>
#include <vector>

struct skin_data
{
	bool gender;
	int skin;

	int head;
	int hair;

	int hat;
	int shirt;
	int pants;
	int shoes;
	int accessory;
	
	float fatness;
	float muscles;
	float slimness;
	float thinness;
	float breasts;
};

struct account_data
{
	bool fully_initialized = false;

	std::string username;
	std::string password;
	std::string email;

	std::vector<std::string> ips;
	skin_data skin;
};

#endif // !ACCOUNT_DATA_H
