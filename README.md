[PL]
# Aplikacja do tworzenia grafiku pracowników szpitala z wykorzystaniem algorytmu genetycznego
<i>C#, WinForms, VisualStudio.</i>
<p align="justify"> Najważniejszym zadaniem aplikacji jest umożliwienie automatycznego rozdzielania funkcji (sala, triaż) zgodnie z grafikiem pomiędzy pracowników szpitalnego oddziału ratunkowego. W celu automatyzacji zadania wykorzystano specjalnie zaimplementowany algorytm optymalizacji genetycznej (GA), którego zadaniem jest takie przydzielenie funkcji, aby na każdej zmianie znajdowała się jedna pielęgniarka/pielęgniarz salowy oraz aby dwoje pracowników zajmowało się triażem. Inne wymagania obejmują np. ograniczenia dotyczące stażystów, równomierne rozłożenie funkcji pomiędzy dyżury nocne i dzienne danego pracownika, uwzględnienie wymiaru etatu i zaległości, równomierne rozłożenie funkcji w ciągu miesiąca. Instrukcję korzystania z aplikacji oraz dokładny opis działania algorytmu genetycznego oraz stawianych przed nim wymagań można zobaczyć w zakładce wiki repozytorium.</p>

<p align="justify">Aby uruchomić aplikację pobierz najnowsze wydanie i uruchom plik Funkcje_GA.exe</p>
 
## Kod

<<<<<<< HEAD
<div align = center>
 <img src="https://github.com/user-attachments/assets/419952d2-d3f3-460d-82ff-158093b60690" alt="Main_UI" width="750">
 <p><em>Rys. 1: Interfejs główny aplikacji przed przydzieleniem funkcji.</em></p><br>

 <img src="https://github.com/user-attachments/assets/461f78e5-4229-45ee-8e5f-d2b42c35dbd2" alt="Main_UI_functions_assigned" width="750">
 <p><em>Rys. 2: Interfejs główny aplikacji po przydzieleniu funkcji. Kolorami oznaczono dyżury wybranego pracownika.</em></p>
</div>
=======
<p align="justify">Poniżej zamieszczono klasy wykorzystane w kodzie wraz z krótkim opisem ich odpowiedzialności.</p>
>>>>>>> parent of 214649f... Merge branch 'main' of https://github.com/RPeszkowski/Schedule_GA

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
*  ViewEmployee - klasa odpowiadająca za obsługę UI związaną z zarządzaniem pracownikami.
*  ViewFile - klasa pośrednicząca pomiędzy UI a systemem zapisu/odczytu z plików.
*  ViewOptimization - klasa pośrednicząca pomięzy UI a procesem optymalizacji.
*  ViewSchedule - klasa pośrednicząca pomiędzy UI a zarządzaniem grafikiem.

## Wykorzystane technologie

* C#, WinForms.
* Moq - do testów jednostkowych.
* Serilog - do logowania zdarzeń.
* xUnit - do testów jednostkowych.

<<<<<<< HEAD
<div align="center">

| **Typ warunku** | **Kod** | **Opis**                                                                                                                                                                      |
| --------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Twardy          | H1      | Zawsze powinna być przypisana jedna funkcja salowej i dwie funkcje triaż. Wszystkie pozostałe zmiany muszą być niefunkcjonalne.                                               |
| Twardy          | H2      | Jeden pracownik nie może być przypisany do dwóch funkcji jednocześnie.                                                                                                        |
| Twardy          | H3a     | Stażyści nie powinni być przypisywani do triażu na zmianach nocnych, chyba że harmonogram nie pozwala inaczej*.                                                               |
| Twardy          | H3b     | Stażyści z okresem zatrudnienia krótszym niż 3 miesiące nie powinni być przypisywani do triażu, ani na zmianach dziennych, ani nocnych.                                       |
| Twardy          | H4      | Nie więcej niż jeden stażysta powinien być przypisany do triażu na jednej zmianie, chyba że harmonogram nie pozwala inaczej.                                                 |
| Miękki          | S1      | Przypisanie równej liczby funkcji dziennych i nocnych dla każdego pracownika (jeśli liczba funkcji jest nieparzysta, liczba funkcji dziennych i nocnych może różnić się o 1). |
| Miękki          | S2      | Liczba funkcji powinna być proporcjonalna do ogólnej liczby zmian pracownika i uwzględniać zaległości.                                                                        |
| Miękki          | S3      | Funkcje powinny być rozmieszczone równomiernie.                                                                                                                               |

</div>

<p align="justify">Wymagano ścisłego spełnienia warunków H1 - H4 i S1. (Na tyle na ile pozwalał na to grafik. Zaistniała sytuacja, gdy na zmianie nocnej był tylko jeden pracownik, który w myśl zasad powinien pełnić triaż. W tym przypadku konieczne było przypisanie stażysty do nocnego triażu). Warunki S2 i S3 mogły być częściowo niespełnione. Wykonano testy algorytmu genetycznego na przykładowym grafiku (folder Tests). Wyniki wydajnościowe dla grafiku znajdującego się w pliku Grafik_test1.txt były nastepujące:</p>

<div align="center">

| **Metryka**     | **Wartość średnia** | **Najmniejsza wartość** | **Największa wartość** | **Odchylenie standardowe** |
| --------------- | ------------------- | ----------------------- | ---------------------- | -------------------------- |
| Liczba iteracji | 5461                | 2032                    | 11109                  | 2099                       |
| Czas wykonania  | 00:00:26            | 00:00:10                | 00:00:54               | 00:00:10                   |

</div>

#### Obserwacje i wnioski
=======
[ENG]<br>
# Funkcje_GA

<i>C#, WinForms, Visual Studio.</i>

<p align="justify"> The main purpose of the application is to automatically assign roles (orderly nurse, triage) according to the schedule among the staff of the hospital emergency department. To automate this task, a specially implemented genetic algorithm (GA) is used. Its goal is to assign roles so that each shift has one nurse/orderly staff and two employees assigned to triage. Other requirements include, for example, restrictions on interns, balanced distribution of roles between day and night shifts, consideration of employment fraction and leave, and distribution of roles throughout the month. Instructions on how to use the application and a detailed description of the genetic algorithm and its requirements can be found in the repository's wiki. </p> 

<p align="justify"> Application execution: Funkcje_GA => Funkcje_GA => Funkcje_GA.exe </p>
>>>>>>> parent of 214649f... Merge branch 'main' of https://github.com/RPeszkowski/Schedule_GA

## Code

<p align="justify"> Below are the classes used in the code along with a brief description of their responsibilities. </p>

#### Model / Logic

<<<<<<< HEAD
<div align="center">

| Nazwa klasy | Odpowiedzialność |
|-------------|-----------------|
| Constants | Przechowuje wartości stałe używane w całym projekcie. |
| CustomExceptions | Niestandardowe wyjątki stworzone na potrzeby projektu. |
| Employee | Klasa przechowuje informacje na temat pracownika takie jak numer id, imię, czy jest stażystą, itp. |
| EmployeeManagement | Obiekt tej klasy zarządza systemem pracowników. Dodaje, usuwa i edytuje informacje o pracownikach (wraz z klasą do testów jednostkowych EmployeeManagementTests). |
| EnumFunctionTypes | Enumerator przypisujący liczby możliwym funkcjom pracowników. |
| FileServiceTxt | Klasa zawiera FileServiceGrafik i FileServicePracownicy do obsługi odpowiednich plików. |
| ListBoxGrafik | Zmodyfikowane list boxy z dodatkowymi metodami. |
| Optimization | Obiekt tej klasy odpowiada za przeprowadzanie optymalizacji. |
| ScheduleManagement | Obiekt tej klasy przechowuje aktualny grafik (wraz z klasą do testów jednostkowych ScheduleManagementTests). |
| Shift | Klasa przechowuje informacje na temat zmiany, tj. id zmiany, obecni pracownicy i pracownicy funkcyjni. |

</div>

#### Warstwa prezentacji

<div align="center">
 
| Nazwa klasy | Odpowiedzialność |
|-------------|-----------------|
| Form1 | Główny formularz aplikacji zawierający dane pracowników oraz grafik. |
| Form2 | Formularz do dodawania, usuwania i edytowania pracowników. |
| PresenterSchedule | Klasa pośrednicząca pomiędzy UI a zarządzaniem grafikiem. |
| ViewEmployee | Klasa odpowiadająca za obsługę UI związaną z zarządzaniem pracownikami. |
| ViewFile | Klasa pośrednicząca pomiędzy UI a systemem zapisu/odczytu z plików. |
| ViewOptimization | Klasa pośrednicząca pomiędzy UI a procesem optymalizacji. |

</div>

## Wykorzystane technologie
=======
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
>>>>>>> parent of 214649f... Merge branch 'main' of https://github.com/RPeszkowski/Schedule_GA

* C#, WinForms.
* Moq – for unit testing.
* Serilog – for logging.
* xUnit – for unit testing.
