# Schedule_GA

To run the app go to Funkcje_GA => Funkcje_GA => Funkcje_GA.exe

About the project

The program was designed to provide a way of assinging functions for employees at hospital emergency department. The app allows for adding, editing and deleting information about current staff, manually creating a schedule or loading it from a text file and assigning functions. Core of an app consist of genetic optimization algorithm (GA) designed scecifically for the task of assigning functions. Depending of the number of days in a month and the number of shifts during the day there may be over 800 binary variables in an optimization problem. The standard algorithm was therefore modified to cope with the task of finding a solution in the shortest possible time.

What I learned while working on this project?

The most important realization was to discover that genetic algorithms work best when they are designed for specific task. For example, as happens in my case, two bits are used to code a specific information (00 means no function, 10 means triage and 01 means orderly nurse) the crossover operation must take that into consideration, as standard one point crossover or randomized crossover would lead to possible loss of data, therefore making search too random.

For additional information about the app and GA optimization visit the Wiki.
