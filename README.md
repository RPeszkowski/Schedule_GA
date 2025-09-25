[PL]
# Funkcje_GA
<i>C#, WinForms, VisualStudio.</i>
<p align="justify"> Najważniejszym zadaniem aplikacji jest umożliwienie automatycznego rozdzielania funkcji (sala, triaż) zgodnie z grafikiem pomiędzy pracowników szpitalnego oddziału ratunkowego. W celu automatyzacji zadania wykorzystano specjalnie zaimplementowany algorytm optymalizacji genetycznej (GA), którego zadaniem jest takie przydzielenie funkcji, aby na każdej zmianie znajdowała się jedna pielęgniarka/pielęgniarz salowy oraz aby dwoje pracowników zajmowało się triażem. Inne wymagania obejmują np. ograniczenia dotyczące stażystów, równomierne rozłożenie funkcji pomiędzy dyżury nocne i dzienne danego pracownika, uwzględnienie wymiaru etatu i zaległości, równomierne rozłożenie funkcji w ciągu miesiąca. Instrukcję korzystania z aplikacji oraz dokładny opis działania algorytmu genetycznego oraz stawianych przed nim wymagań można zobaczyć w zakładce wiki repozytorium.</p>

<p align="justify">Aby uruchomić aplikację pobierz najnowsze wydanie i uruchom plik Funkcje_GA.exe</p>

## Algorytm genetyczny

Pełniejszy opis w zakładce Wiki.

Algorytm genetyczny zajmuje się przypisywanie funkcji pracownikom. Na każdej zmianie musi znaleźć się jedna salowa pielęgniarka/pielęgniarz oraz dwie osoby pracujące na triażu. Z przyczyn praktycznych maksymalna liczba osób na zmianie to 8. Do zakodowania numeru pracownika potrzebne są więc 3 bity. W połączeniu z 3 funkcjami na każdej zmianie i 62 zmianami w miesiącu daje to 556 zmiennych binarnych. Poniżej przytoczone są wymagania, jakie musi spełniać rozwiązanie problemu optymalizacji. Literka H oznaczono wymagania twarde (konieczne do spełnienia, chyba, że nie pozwala na to grafik), a literką S miękkie (do spełnienia w miarę możliwości).
 
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
