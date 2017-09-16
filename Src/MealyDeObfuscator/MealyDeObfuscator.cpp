#include "stdafx.h"

void deobfuscate(char* text, int length, int key, char automata[][2], char output[][2])
{
	int v = 0, state = 0, i = 0;
	for (i = 0; i < length; i++)
	{
		v = key & 1;
		key >>= 1;
		text[i] = output[state][v];
		state = automata[state][v];
	}
	text[i] = '\0';
}

int main()
{
	char text[20];
	int key = 135265;

	char automata[][2] =
	{ { 6, 1 },{ 2, 5 },{ 3, 9 },{ 4, 7 },{ 0, 11 },{ 4, 7 },
	{ 2, 9 },{ 8, 9 },{ 9, 8 },{ 10, 7 },{ 4, 5 },{ 12, 1 },
	{ 12, 12 } };

	char output[][2] =
	{ { 'e', 's' },{ 'u', 'e' },{ 'p', 'u' },{ 'e', 'a' },
	{ 'r', 'r' },{ 'c', 'p' },{ 't', 's' },{ 's', 't' },
	{ 's', 'e' },{ 'w', 's' },{ 'o', 's' },{ 'd', 't' },
	{ 'p', 'u' } };

	deobfuscate(text, sizeof(text)-1, key, automata, output);

	printf("Output: %s", text);
	return 0;
}