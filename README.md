[PL]
# Funkcje_GA
<i>C#, WinForms, VisualStudio.</i>
<p align="justify"> Najważniejszym zadaniem aplikacji jest umożliwienie automatycznego rozdzielania funkcji (sala, triaż) zgodnie z grafikiem pomiędzy pracowników szpitalnego oddziału ratunkowego. W celu automatyzacji zadania wykorzystano specjalnie zaimplementowany algorytm optymalizacji genetycznej (GA), którego zadaniem jest takie przydzielenie funkcji, aby na każdej zmianie znajdowała się jedna pielęgniarka/pielęgniarz salowy oraz aby dwoje pracowników zajmowało się triażem. Inne wymagania obejmują np. ograniczenia dotyczące stażystów, równomierne rozłożenie funkcji pomiędzy dyżury nocne i dzienne danego pracownika, uwzględnienie wymiaru etatu i zaległości. Instrukcję korzystania z aplikacji oraz dokładny opis działania algorytmu genetycznego oraz stawianych przed nim wymagań można zobaczyć w zakładce wiki repozytorium.</p>

<p align="justify">Uruchomienie aplikacji: Funkcje_GA => Funkcje_GA => Funkcje_GA.exe</p>
 
## Kod

<p align="justify">Poniżej zamieszczono klasy wykorzystane w kodzie wraz z krótkim opisem ich odpowiedzialności.</p>

#### Model/logika

* Constants - przechowuje wartości stałe używane w całym projekcie.
* CustomExceptions - niestandardowe wyjątki stworzone na potrzeby projektu.
* Employee - klasa przechowuje informacje na temat pracownika takie jak numer id, imię, czy jest stażystą, itp.
* EmployeeManagement (wraz z klasą do testów jednostkowych z sufiksem EmployeeManagementTests) - obiekt tej klasy zarządza systemem pracowników. Dodaje, usuwa i edytuje informacje o pracownikach.
* FileServiceTxt - klasa zaawiera FileServiceGrafik i FileServicePracownicy do obsługi odpowiednich plików.
* FunctionTypes - enumerator przypisujący liczby możliwym funkcjom pracowników.
* ListBoxGrafik - zmodyfikowane listboxy z dodatkowymi metodami.
* Optimization - obiekt tej klasy odpowiada za przeprowadzanie optymalizacji.
* ScheduleManagement - (wraz z klasą do testów jednostkowych z sufiksem ScheduleManagementTests) obiekt tej klasy przechowuje aktualny grafik. Grafik jest obecnie listą 62 (po 2 zmiany na dzień) elementów typu Shift, jednak zastosowanie interfejsu pozwala na zmiany w przyszłości.
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

# Schedule_GA
[ENG]
To run the app go to Funkcje_GA => Funkcje_GA => Funkcje_GA.exe
