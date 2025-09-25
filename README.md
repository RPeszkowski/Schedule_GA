[PL]
# Funkcje_GA
<i>C#, WinForms, VisualStudio.</i>
<p align="justify"> Najważniejszym zadaniem aplikacji jest umożliwienie automatycznego rozdzielania funkcji (sala, triaż) zgodnie z grafikiem pomiędzy pracowników szpitalnego oddziału ratunkowego. W celu automatyzacji zadania wykorzystano specjalnie zaimplementowany algorytm optymalizacji genetycznej (GA), którego zadaniem jest takie przydzielenie funkcji, aby na każdej zmianie znajdowała się jedna pielęgniarka/pielęgniarz salowy oraz aby dwoje pracowników zajmowało się triażem. Inne wymagania obejmują np. ograniczenia dotyczące stażystów, równomierne rozłożenie funkcji pomiędzy dyżury nocne i dzienne danego pracownika, uwzględnienie wymiaru etatu i zaległości, równomierne rozłożenie funkcji w ciągu miesiąca. Instrukcję korzystania z aplikacji oraz dokładny opis działania algorytmu genetycznego oraz stawianych przed nim wymagań można zobaczyć w zakładce wiki repozytorium.</p>

<p align="justify">Aby uruchomić aplikację pobierz najnowsze wydanie i uruchom plik Funkcje_GA.exe</p>
 
## Kod

<p align="justify">Poniżej zamieszczono klasy wykorzystane w kodzie wraz z krótkim opisem ich odpowiedzialności.</p>

#### Model/logika

* Constants - przechowuje wartości stałe używane w całym projekcie.
* CustomExceptions - niestandardowe wyjątki stworzone na potrzeby projektu.
* Employee - klasa przechowuje informacje na temat pracownika takie jak numer id, imię, czy jest stażystą, itp.
* EmployeeManagement (wraz z klasą do testów jednostkowych z sufiksem EmployeeManagementTests) - obiekt tej klasy zarządza systemem pracowników. Dodaje, usuwa i edytuje informacje o pracownikach.
* EnumFunctionTypes - enumerator przypisujący liczby możliwym funkcjom pracowników.
* FileServiceTxt - klasa zawiera FileServiceGrafik i FileServicePracownicy do obsługi odpowiednich plików.
* ListBoxGrafik - zmodyfikowane list boxy z dodatkowymi metodami.
* Optimization - obiekt tej klasy odpowiada za przeprowadzanie optymalizacji.
* ScheduleManagement - (wraz z klasą do testów jednostkowych z sufiksem ScheduleManagementTests) obiekt tej klasy przechowuje aktualny grafik.
* Shift - klasa przechowuje informacje na temat zmiany, tj. id zmiany, obecni pracownicy i pracownicy funkcyjni.

#### Warstwa prezentacji

*  Form1 - główny formularz aplikacji zawierający dane pracowników oraz grafik.
*  Form2 - formularz dodawania/usuwania/edytowania pracowników.
*  PresenterSchedule - klasa pośrednicząca pomiędzy UI a zarządzaniem grafikiem.
*  ViewEmployee - klasa odpowiadająca za obsługę UI związaną z zarządzaniem pracownikami.
*  ViewFile - klasa pośrednicząca pomiędzy UI a systemem zapisu/odczytu z plików.
*  ViewOptimization - klasa pośrednicząca pomięzy UI a procesem optymalizacji.

## Wykorzystane technologie

* C#, WinForms.
* Moq - do testów jednostkowych.
* Serilog - do logowania zdarzeń.
* xUnit - do testów jednostkowych.

[ENG]<br>
# Funkcje_GA

<i>C#, WinForms, Visual Studio.</i>

<p align="justify"> The main purpose of the application is to automatically assign roles (orderly nurse, triage) according to the schedule among the staff of the hospital emergency department. To automate this task, a specially implemented genetic algorithm (GA) is used. Its goal is to assign roles so that each shift has one nurse/orderly staff and two employees assigned to triage. Other requirements include, for example, restrictions on interns, balanced distribution of roles between day and night shifts, consideration of employment fraction and leave, and distribution of roles throughout the month. Instructions on how to use the application and a detailed description of the genetic algorithm and its requirements can be found in the repository's wiki. </p> 

<p align="justify"> Application execution: Funkcje_GA => Funkcje_GA => Funkcje_GA.exe </p>

## Code

<p align="justify"> Below are the classes used in the code along with a brief description of their responsibilities. </p>

#### Model / Logic

* Constants – stores constant values used throughout the project.
* CustomExceptions – custom exceptions created for the project.
* Employee – class storing information about an employee such as ID, name, whether they are an intern, etc.
* EmployeeManagement (together with EmployeeManagementTests) – manages the employee system, including adding, removing, and editing employee information.
* EnumFunctionTypes – enumerator assigning numbers to possible employee roles.
* FileServiceTxt – contains FileServiceGrafik and FileServicePracownicy for handling respective files
* ListBoxGrafik – customized list boxes with additional methods.
* Optimization – responsible for performing the optimization process.
* ScheduleManagement (together with ScheduleManagementTests) – stores the current schedule.
* Shift – class storing information about a shift, including shift ID, present employees, and assigned roles.

#### Presentation Layer

* Form1 – main application form containing employee data and schedule.
* Form2 – form for adding/removing/editing employees.
* ViewEmployee – handles the UI related to employee management.
* ViewFile – intermediates between the UI and file reading/writing.
* ViewOptimization – intermediates between the UI and the optimization process.
* ViewSchedule – intermediates between the UI and schedule management.

## Technologies Used

* C#, WinForms.
* Moq – for unit testing.
* Serilog – for logging.
* xUnit – for unit testing.
