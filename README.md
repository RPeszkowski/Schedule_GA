# Schedule_GA
[ENG]
To run the app go to Funkcje_GA => Funkcje_GA => Funkcje_GA.exe

About the project

<p align="justify">The program was designed to provide a way of assinging functions for employees at hospital emergency department. The app allows for adding, editing and deleting information about current staff, manually creating a schedule or loading it from a text file and assigning functions. Core of an app consist of genetic optimization algorithm (GA) designed scecifically for the task of assigning functions. Depending of the number of days in a month and the number of shifts during the day there might be up to 558 binary variables in an optimization problem. The standard algorithm could not offer more than giving a permissible solution, that did not follow requirements of accepptable solutions, therefore was modified to solve the problem in a much shorter time.</p>

What I learned while working on this project? 

<p align="justify">I think that the most important realization whas that genetic algorithms are to be designed speciffically for a given task and should consider data format and way of coding information into genes in order not to accidentally mess the data of the same gene from two parents and as a result of this loose information. I also learned a lot about writing clean and scalable code. As the application grew and the codebase became larger, I realized that some things could be improved. Splitting the code into modules made it easier to maintain and allowed me to implement future changes without rewriting everything from scratch.

For additional information about the app and GA optimization please consider visiting Wiki.</p>

[PL]

#### Najważniejsze fragmenty kodu

<p align="justify">Kod ten składa się z kilkunastu klas, z których najważniejsze to:</p>

* Employee - klasa przechowuje informacje na temat pracownika takie jak numer id, imię, czy jest stażystą, itp.
* EmployeeManagement - obiekt tej klasy zarządza systemem pracowników. Dodaje, usuwa i edytuje informacje o pracownikach. Pracownicy są przechowywani w postaci listy, jednak zastosowanie interfejsów pozwala na zmianę implementacji w przyszłosci.
* Optimization - jest to wydzielony obszar kodu odpowiedzialny za przeprowadzanie optymalizacji.
* ScheduleManagement - obiekt tej klasy przechowuje aktualny grafik. Grafik jest obecnie listą 62 (po 2 zmiany na dzień) elementów typu Shift, jednak zastosowanie interfejsu pozwala na zmiany w przyszłości.
* Shift - klasa przechowuje informacje na temat zmiany, tj. id zmiany, obecni pracownicy i pracownicy funkcyjni.

<p align="justify">
Projekt skłąda się z warstwy modelu (IOptimization, IEmployeeManagement, IScheduleManagement, Shift, Employee), warstwy prezentera (IViewFileService, IViewEmployee, IViewSchedule, IViewOptimization) oraz warstwy view (Form1 i Form2). Zaimplementowano testy jednostkowe dla klasy EmployeeManagement.</p>

