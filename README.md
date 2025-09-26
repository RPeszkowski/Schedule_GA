[PL]
# Aplikacja do tworzenia grafiku pracowników szpitala z wykorzystaniem algorytmu genetycznego
<i>C#, WinForms, VisualStudio.</i>
<p align="justify"> Najważniejszym zadaniem aplikacji jest umożliwienie automatycznego rozdzielania funkcji (sala, triaż) zgodnie z grafikiem pomiędzy pracownikami Szpitalnego Oddziału Ratunkowego. W celu automatyzacji zadania wykorzystano specjalnie zaimplementowany algorytm optymalizacji genetycznej (GA), którego zadaniem jest przydzielenie funkcji zgodnie z przyjętymi założeniami. Instrukcję korzystania z aplikacji oraz dokładny opis działania algorytmu genetycznego i stawianych przed nim wymagań można zobaczyć w zakładce wiki repozytorium.</p>

<p align="justify">Aby uruchomić aplikację pobierz najnowsze wydanie i uruchom plik Funkcje_GA.exe</p>

<div align = center>
 <img src="https://github.com/user-attachments/assets/24bcf20e-4eca-4056-8e42-0a0609255c4f" alt="Getting_started_UI" width="600" style="border-radius: 100px;">
 <p><em>Rys. 1: Interfejs główny aplikacji.</em></p>
</div>

## Algorytm genetyczny

Pełniejszy opis w zakładce Wiki.

#### Wstęp
<p align="justify">Algorytm wybiera osoby tak, aby rozwiązanie było optymalne lub suboptymalne względem przyjętego kryterium. Zaczyna od stworzenia populacji początkowej, oblicza dla niej wartości funkcji celu, a następnie wybierając najlepsze osobniki tworzy populację potomną korzystając z mechanizmów krzyżowania i mutacji. Proces jest powtarzany dopóki nie zostanie osiągnięta wymagana jakość rozwiązania.</p>

#### Format danych
 
<p align="justify">Na każdej zmianie musi znaleźć się jedna salowa pielęgniarka/pielęgniarz oraz dwie osoby pracujące na triażu. Z przyczyn praktycznych, maksymalna liczba osób na zmianie to 8. Do zakodowania numeru pracownika potrzebne są więc 3 bity. W połączeniu z 3 funkcjami na każdej zmianie i 62 zmianami w miesiącu daje to 558 zmiennych binarnych.</p>

#### Wymagania

<p align="justify">Poniżej przytoczone są wymagania, jakie musi spełniać rozwiązanie problemu optymalizacji. Literką H oznaczono wymagania twarde (konieczne do spełnienia, chyba że nie pozwala na to grafik), a literką S miękkie (do spełnienia w miarę możliwości).</p>

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

<p align="justify">Przydzielanie funkcji trwa zaledwie kilkadziesiąt sekund, co stanowi duży zysk względem ręcznego przydzielania, które mogło trwać nawet 2 godziny. Ręczne przydzielanie pozwalało na spełnienie zwykle tylko warunków H1 - H4, a warunki S1 - S3 były często naruszane. Dodatkowo, konieczne były czasochłonne zmiany grafiku, gdy pracownicy zamieniali się dyżurami. Zaletą rozwiązania ręcznego jest natomiast to, że przydzielanie funkcji może uwzględnić personalne preferencje pracowników.</p>
 
## Kod

<p align="justify">Poniżej zamieszczono klasy wykorzystane w kodzie wraz z krótkim opisem ich odpowiedzialności.</p>

#### Model/logika

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

* C#, WinForms.
* Moq - do testów jednostkowych.
* Serilog - do logowania zdarzeń.
* xUnit - do testów jednostkowych.

## Informacje o wersji:
* .Net: 4.7.2
* MS Visual Studio: 17.14.13
* C#: 7.3
