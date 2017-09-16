# Mealy String Obfuscator
This project contains code and a test program used to create a Mealy automata giving as input a string. 
The aim of the project is to provide a simple method to generate this kind of automata and to use it in order to obfuscate sensitive strings.

## Download
 - [Source code][1]
 - [Download binary][2]
 
## Usage
The _MealyAutomata.fs_ file is the class in charge for the creation of the Mealy automata. The project _MealyDeObfuscator_ is 
a simple C application that use the output generated in order to deobfuscate a given string.

Once downloaded the binary you can use the program by invoking:

	StringObfuscator.exe supersecretpassword
	
The result of this operation will be:

	
	      -=[ Mealy String Obfuscator ]=-
	Copyright (c) 2017 Antonio Parata - [@s4tan][3]
	
	Input text: supersecretpassword
	Input: 1,0,0,0,0,1,1,0,0,0,0,0,1,0,0,0,0,1,0, Int: 135265
	Output: {{'e', 's'}, {'u', 'e'}, {'p', 'u'}, {'e', 'a'}, {'r', 'r'}, {'c', 'p'}, {'t', 's'}, {'s', 't'}, {'s', 'e'}, {'w', 's'}, {'o', 's'}, {'d', 't'}, {'p', 'u'}}
	Automata: {{6, 1}, {2, 5}, {3, 9}, {4, 7}, {0, 11}, {4, 7}, {2, 9}, {8, 9}, {9, 8}, {10, 7}, {4, 5}, {12, 1}, {12, 12}}
	
Now you can use the above output to deobfuscate the string. Below a simple C program that do this:

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
		{{6, 1}, {2, 5}, {3, 9}, {4, 7}, {0, 11}, {4, 7}, 
		{2, 9}, {8, 9}, {9, 8}, {10, 7}, {4, 5}, {12, 1}, 
		{12, 12}};
	
		char output[][2] =
		{{'e', 's'}, {'u', 'e'}, {'p', 'u'}, {'e', 'a'}, 
		{'r', 'r'}, {'c', 'p'}, {'t', 's'}, {'s', 't'}, 
		{'s', 'e'}, {'w', 's'}, {'o', 's'}, {'d', 't'}, 
		{'p', 'u'}};
	
		deobfuscate(text, sizeof(text)-1, key, automata, output);
	
		printf("Output: %s", text);
		return 0;
	}
	
## Compile
In order to compile the source code you should have installed Visual Studio >= 14 (2015 version). Once downloaded the source code just run the _build.bat_ script file.
You will find the compiled binaries in the _build_ folder.

## Test library
Once that you have compiled the code you should check that everything is ok. The _MealyObfuscatorTest_ just try to obfuscate and deobfuscate 1000 strings. Run it to be sure that 
the generated automata is correct.

## License information
Copyright (C) 2014-2017 Antonio Parata

  [1]: https://github.com/enkomio/MealyObfuscator/tree/master/Src
  [2]: https://github.com/enkomio/MealyObfuscator/releases/latest
  [3]: https://twitter.com/s4tan