using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa zajmuje się przeprowadzaniem optymalizacji genetycznej.
    internal class Optimization : IOptimization
    {
        //Obiekty tej klasy są wykorzystywane w optymalizacji genetycznej.
        private class Osobnik
        {
            public bool[] genom;            //Rozwiązanie związane z danym osobnikiem.
            public decimal wartosc;         //Wartość funkcji celu dla danego rozwiązania.

            //Konstruktor.
            public Osobnik(bool[] genom, decimal wartosc)
            {
                this.genom = genom;
                this.wartosc = wartosc;
            }
        }

        //Obiekt tej klasy jest wykorzystywany do porównywania wartości f. celu dwóch osobników w problemie optymalizacji.
        private class OsobnikComparer : IComparer
        {
            //Porównujemy wartości f. celu dla pary osobników.
            public int Compare(object x, object y)
            {
                return (new CaseInsensitiveComparer()).Compare(((Osobnik)x).wartosc, ((Osobnik)y).wartosc);
            }
        }

        private readonly IEmployeeManagement _employeeManager;                    //Instancja menadżera pracowników.
        private readonly IScheduleManagement _scheduleManager;                         //Instancja menadżera grafiku.

        private int[] grafikDyzurow;                                         //Grafik.
        private double[] wymiarEtatu;                                        //Wymiar etatu pracowników.
        private double[] zaleglosci;                                         //Zaległości pracowników.
        private int[] liczbaDyzurow;                                         //Liczba dyżurów na danej zmianie.
        private int[] nieTriazDzien;                                         //Pracownicy, którzy nie powinni być na dziennym triażu.
        private int[] nieTriazNoc;                                           //Pracownicy, którzy nie powinni być na nocnym triażu.
        private double[] oczekiwanaLiczbaFunkcji;                            //Oczekiwana liczba funkcji dla każdego pracownika.
        private decimal stopienZdegenerowania;                               //Określa, czy jest możliwe ułożenie grafiku bez sytuacji zakazanych.

        //Konstruktor.
        public Optimization(IEmployeeManagement employeeManager, IScheduleManagement scheduleManager)
        {
            this._employeeManager = employeeManager;
            this._scheduleManager = scheduleManager;
        }

        //Informacja o przebiegu optymalizacji.
        public event Action<string> ProgressUpdated;

        //Event do powiadamiania o ostrzeżeniach.
        public event Action<string> WarningRaised;

        //Obliczamy funkcję celu dla danego rozwiązania.
        private decimal FunkcjaCelu(bool[] funkcje)
        {
            decimal W = 0.0m;                                               //Funkcja celu.
            decimal a = 0.0m;                                               //Funkcja kary.
            int[] liczbaStazystowNaTriazu = new int[2 * LICZBA_DNI];        //Liczba stażystów na triażu danego dnia.      
            int[][] liczbaFunkcji = new int[LICZBA_ZMIAN][];                //Liczba funkcji na dziennej zmianie (0) i nocnej zmianie (1)
            bool[] numerOsoby = new bool[MAX_LICZBA_BITOW];                 //Indeks osoby na zmianie zakodowany binarnie.
            int nrOsobySala;                                                //Indeks na zmianie osoby przypisanej do sali.
            int nrOsobyTriaz1;                                              //Indeks na zmianie pierwszej osoby przypisanej do triażu.
            int nrOsobyTriaz2;                                              //Indeks na zmianie drugiej osoby przypisanej do triażu.
            int liczbaKonsekwentnychBezFunkcji;                             //Liczba następujących po sobie bezpośrednio dyżurów, w trakcie których osoba nie pełni funkcji.
            bool[][] dyzuryRozstaw = new bool[MAX_LICZBA_OSOB][];           //Każdemu pracownikowi przypisujemy tablicę o długości równej liczbie zmian. Jeśli dana zmiana jest funkcyjna dajemy true, jeśli nie to false.       
            int[] nrDyzuru = new int[MAX_LICZBA_OSOB];                      //Zawiera aktualne indeksy do tablicy dyzuryRozstaw. Jeśli w danym dniu osoba o indeksie i ma dyżur, to na koniec pętli for, w której odbywa się dekodowanie rozwiązania
                                                                            //nrDyzuru[i] zostanie inkrementowane o 1. Przykład: na zmianie nr. 0 (dzień) osoba o indeksie 10 ma swój pierwszy dyżur (nrDyzuru[10] = 0) i jest to sala.
                                                                            //dyzuryRozstaw[10][0] zostanie przypisane true, a na koniec iteracji dekodującej rozwiązanie dla pierwszej zmiany (czyli nrZmiany = 0) nrDyzuru[10] zostanie inkrementowane.
                                                                            //Jeśli ta sama osoba ma następny dyżur na zmianie nocnej 3 dnia miesiąca (czyli nrZmiany = 33) i jest to zmiana bez funkcji, to dyzuryRozstaw[10][1] pozostanie false,
                                                                            //a na koniec iteracji dla 33 zmiany nrDyzuru[10] zmieni się z 1 na 2.
            int tempIndex;                                                  //Zmienna pomocnicza oznaczająca indeks w tablicy grafikDyzurow.
            int tempNrOsoba;                                                //Numer osoby znajdujący się w grafikDyzurow[tempIndex].

            //Zerujemy zmienne lokalne przechowujące informacje o rozwiązaniu.
            for (int nrZmiany = 0; nrZmiany < MAX_LICZBA_OSOB; nrZmiany++)
            {
                dyzuryRozstaw[nrZmiany] = new bool[Convert.ToInt32(wymiarEtatu[nrZmiany])];
                nrDyzuru[nrZmiany] = 0;
            }

            //Inicjalizujemy liczbę funkcji.
            for (int i = 0; i < LICZBA_ZMIAN; i++)
                liczbaFunkcji[i] = new int[MAX_LICZBA_OSOB];

            //Ciąg dalszy inicjalizowania zmiennych lokalnych.
            for (int nrOsoby = 0; nrOsoby < 2 * LICZBA_DNI; nrOsoby++)
                liczbaStazystowNaTriazu[nrOsoby] = 0;

            //Wyznaczamy parametry dla danych indeksów i zmiany.
            void WyznaczParametry(int[] indexes, int nrZmiany, int dzien_0_noc_1)
            {
                for (int nrIndeksu = 0; nrIndeksu < indexes.Length; nrIndeksu++)
                {
                    tempIndex = nrZmiany * MAX_LICZBA_DYZUROW + indexes[nrIndeksu];        //Indeks.
                    tempNrOsoba = grafikDyzurow[tempIndex];                         //Osoba.

                    //Sprawdzamy, czy nie zakodowaliśmy indeksu, który nie oznacza żadnej osoby (mozliwe tylko, gdy liczba osób na zmianie < MAX_LICZBA_DYZUROW).
                    if (tempNrOsoba == 0)
                        a += 1000000.0m;

                    //Jeśli zakodowaliśmy osobę to zwiększamy liczbę sal tej osoby i dyżur funkcyjny w dyzuryRozstaw.
                    else
                    {
                        liczbaFunkcji[dzien_0_noc_1][tempNrOsoba - 1]++;
                        dyzuryRozstaw[tempNrOsoba - 1][nrDyzuru[tempNrOsoba - 1]] = true;
                    }
                }
            }

            //Funkcja do dekodowania rozwiązania i wyznaczania niektórych kar.
            void DekodujRozwiazanie(int nrZmiany, int dzien_0_noc_1)
            {
                //Zmiana dzienna.
                //Zerujemy indeksy.
                nrOsobySala = 0;
                nrOsobyTriaz1 = 0;
                nrOsobyTriaz2 = 0;

                //Sprawdzamy, czy zmiana nie jest pusta.
                if (liczbaDyzurow[nrZmiany] > 0)
                {
                    //Dekodujemy indeks osoby pełniącej funkcję salowej/salowego (indeks to liczba od 0 do MAX_LICZBA_DYZURÓW - 1).
                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobySala = (nrOsobySala * 2) + (funkcje[3 * nrZmiany * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    //Dekodujemy indeks pierwszej osoby pełniącej funkcję triaż (indeks to liczba od 0 do MAX_LICZBA_DYZURÓW - 1).
                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz1 = (nrOsobyTriaz1 * 2) + (funkcje[3 * nrZmiany * MAX_LICZBA_BITOW + MAX_LICZBA_BITOW + j] ? 1 : 0);

                    //Dekodujemy indeks drugiej osoby pełniącej funkcję triaż (indeks to liczba od 0 do MAX_LICZBA_DYZURÓW - 1).
                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz2 = (nrOsobyTriaz2 * 2) + (funkcje[3 * nrZmiany * MAX_LICZBA_BITOW + 2 * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    //Sprawdzamy, czy jedna osoba nie ma przypisanych dwóch funkcji.
                    if (nrOsobySala == nrOsobyTriaz1)
                        a += 1000000.0m;

                    if (nrOsobySala == nrOsobyTriaz2)
                        a += 1000000.0m;

                    if (nrOsobyTriaz1 == nrOsobyTriaz2)
                        a += 1000000.0m;

                    //Wyznaczamy parametry dla wybranych osób.
                    WyznaczParametry(new int[] { nrOsobySala, nrOsobyTriaz1, nrOsobyTriaz2}, nrZmiany, dzien_0_noc_1);

                    //Sprawdzamy, czy zmiana jest dzienna.
                    if (dzien_0_noc_1 == 0)
                    {

                        //Sprawdzamy, czy przypisaliśmy do triażu stażystę, który nie powinien być przypisany. Jeśli tak, to nakładamy karę.
                        for (int j = 0; j < nieTriazDzien.Length; j++)
                        {
                            if (grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazDzien[j] || grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazDzien[j])
                                a += 100.0m;
                        }

                        //Sprawdzamy liczbę stażystów na triażu. Zakładamy, że wszystkie osoby, które nie powinny pełnić triażu za dnia, nie powinno go pełnić również w nocy,
                        //czyli zbiór osób nie mogących być na dziennym triażu zawiera się w zbiorze osób nie mogących być na nocnym triażu.
                        for (int j = 0; j < nieTriazNoc.Length; j++)
                        {
                            if (grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                                liczbaStazystowNaTriazu[nrZmiany]++;
                        }
                    }

                    //Zmiana nocna.
                    else
                    {
                        //Jeśli na triaży jest stażysta, to dodajemy karę i inkrementujemy liczbęstażystów na triażu.
                        for (int j = 0; j < nieTriazNoc.Length; j++)
                        {
                            if (grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || grafikDyzurow[nrZmiany * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                            {
                                liczbaStazystowNaTriazu[nrZmiany]++;
                                a += 100.0m;
                            }
                        }
                    }

                    //Zwiększamy nrDyzuru dla wszystkich pracowników obecnych na zmianie.
                    for (int j = nrZmiany * MAX_LICZBA_DYZUROW; j < (nrZmiany + 1) * MAX_LICZBA_DYZUROW; j++)
                    {
                        if (grafikDyzurow[j] != 0)
                            nrDyzuru[grafikDyzurow[j] - 1]++;
                    }
                }
            }

            //Dekodujemy rozwiązanie dla kolejnych dni.
            for (int nrZmiany = 0; nrZmiany < LICZBA_DNI; nrZmiany++)
            {
                //Dekodujemy rozwiązanie dla dziennej zmiany.
                DekodujRozwiazanie(nrZmiany, 0);

                //Dekodujemy rozwiązanie dla nocnej zmiany.
                DekodujRozwiazanie(nrZmiany + LICZBA_DNI, 1);
            }

            //Sprawdzamy liczbę stażystów na danej zmianie. Jeśli jest więcej niż 1 to dopisujemy karę.
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                if (liczbaStazystowNaTriazu[i] >= 2)
                    a += 10000.0m;
            }

            //Wyliczamy funkcję celu. Dotyczy warunków podziału proporcjonalnego do wymiaru etatu i zaległości, równego podziału pomiędzy zmiany nocne i dzienne
            //oraz jednorodnego rozdzielenia funkcji na przestrzeni miesiąca.
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                //Sprawdzamy, czy różnica między przypisanymi funkcjami, a parametrem oczekiwana liczba funkcji jest większa lub równa 2.
                if (Math.Abs(liczbaFunkcji[0][i] + liczbaFunkcji[1][i] - oczekiwanaLiczbaFunkcji[i]) >= 2)
                    W += 0.01m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaFunkcji[0][i] + liczbaFunkcji[1][i] - oczekiwanaLiczbaFunkcji[i])));

                //Sprawdzamy, czy różnica między przypisanymi funkcjami, a parametrem oczekiwana liczba funkcji jest większa lub równa 1 i mniejsza od 2.
                else if (Math.Abs(liczbaFunkcji[0][i] + liczbaFunkcji[1][i] - oczekiwanaLiczbaFunkcji[i]) >= 1)
                    W += 0.0000001m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaFunkcji[0][i] + liczbaFunkcji[1][i] - oczekiwanaLiczbaFunkcji[i])));

                //Sprawdzamy czy dysproporcja pomiędzy funkcjami nocnymi i dziennymi jest większa od 2.
                if (Math.Abs(liczbaFunkcji[0][i] - liczbaFunkcji[1][i]) > 2)
                    W += 1.0m * Convert.ToDecimal(Math.Abs(liczbaFunkcji[0][i] - liczbaFunkcji[1][i]));
                
                //Sprawdzamy czy dysproporcja pomiędzy funkcjami nocnymi i dziennymi jest dokładnie równa 2.
                else if (Math.Abs(liczbaFunkcji[0][i] - liczbaFunkcji[1][i]) == 2)
                    W += 0.0002m;

                liczbaKonsekwentnychBezFunkcji = 0; //Zerujemy konsekwentną liczbę dyżurów bez funkcji.

                //Sprawdzamy warunek jednorodnego rozłożenia dyżurów funkcyjnych.
                for (int j = 0; j < dyzuryRozstaw[i].Length; j++)
                {
                    //Jeśli dyżur jest bezfunkcyjny to inkrementujemy.
                    if (!dyzuryRozstaw[i][j])
                        liczbaKonsekwentnychBezFunkcji++;

                    //Jeśli dyzur jest funkcyjny i były minimum 4 kolejne dyżury bez funkcji to naliczamy karę.
                    else if (dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji > 3)
                    {
                        W += 0.00000001m * Convert.ToDecimal(liczbaKonsekwentnychBezFunkcji);
                        liczbaKonsekwentnychBezFunkcji = 0;
                    }

                    //Jeśli dyzur jest funcyjny i były maksimum 3 kolejne dyżury bez funkcji to nie naliczamy kary.
                    else if (dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji <= 3)
                        liczbaKonsekwentnychBezFunkcji = 0;

                    //Jeśli to ostatni dyżur danej osoby i były minimum 4 dyżury bez funkcji to naliczamy karę.
                    if (j == dyzuryRozstaw[i].Length - 1 && !dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji > 3)
                        W += 0.00000001m * Convert.ToDecimal(liczbaKonsekwentnychBezFunkcji);
                }
            }

            //Dodajemy funkcję kary do funkcji celu i zwracamy funkcję celu.
            W += a;
            return W;
        }

        //Liczba dyzurów na danej zmianie.
        private int[] LiczbaDyzurowNaZmianie()
        {
            int[] liczbaDyzurow = new int[2 * LICZBA_DNI];          //Liczba dyzurów dla danej zmiany.

            //Dla każdej zmiany liczymy dyżury.
            for (int i = 0; i < LICZBA_DNI; i++)
            {
                liczbaDyzurow[i] = 0;

                //Jeżeli grafik nie zawiera zera to inkrementujemy liczbę dyżurów na danej zmianie.
                for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                {
                    //Zmiana dzienna.
                    if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + j] != 0)
                        liczbaDyzurow[i]++;

                    //Zmiana nocna.
                    if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + j + LICZBA_DNI * MAX_LICZBA_DYZUROW] != 0)
                        liczbaDyzurow[i + LICZBA_DNI]++;
                }
            }

            return liczbaDyzurow;
        }

        //Generuje tablicę z numerami osób, które nie powinny mieć triażu za dnia.
        private int[] ListaNieTiazDzien()
        {
            //Pobieramy listę osób, które nie powinny być przypisane do triażu w ciągu dnia i zamieniamy na tablicę int.
            Employee[] employeesNieTriazDzien = _employeeManager.GetAllActive().Where(emp => emp.CzyTriazDzien == false).ToArray();
            int[] nieTriazDzien = new int[employeesNieTriazDzien.Count()];          //Liczba osób, które nie powinny mieć triażu w ciągu dnia.
            for (int i = 0; i < employeesNieTriazDzien.Count(); i++)
                nieTriazDzien[i] = employeesNieTriazDzien[i].Numer;

            //Zwracamy tablicę int.
            return nieTriazDzien;
        }

        //Generuje tablicę z numerami osób, które nie powinny mieć triażu w nocy.
        private int[] ListaNieTiazNoc()
        {
            //Pobieramy listę osób, które nie powinny być przypisane do triażu w ciągu nocy i zamieniamy na tablicę int.
            Employee[] employeesNieTriazNoc = _employeeManager.GetAllActive().Where(emp => emp.CzyTriazNoc == false).ToArray();
            int[] nieTriazNoc = new int[employeesNieTriazNoc.Count()];          //Liczba osób, które nie powinny mieć triażu w ciągu nocy.
            for (int i = 0; i < employeesNieTriazNoc.Count(); i++)
                nieTriazNoc[i] = employeesNieTriazNoc[i].Numer;

            //Zwracamy tablicę int.
            return nieTriazNoc;
        }

        //Oblcizamy oczekiwaną liczbę dyzurów funkcyjnych dla każdego pracownika.
        private double[] OczekiwanaLiczbaFunkcji()
        {
            const double MAX_LICZBA_FUNKCJI = 8.5;                      //Maksymalna oczekiwana liczba funkcji.
            const double MIN_LICZBA_FUNKCJI = 2.5;                      //Minimalna oczekiwana liczba funkcji.
            double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB]; //Tablica z oczekiwanymi wartościami dla wszystkich pracowników.
            int liczbaDniRoboczych = 0;                                     //Liczba dni, w których są dyżury.
            double sumaEtatow = 0.0;                                        //Suma etatów wszytskich pracowników.

            //Sprawdzamy, które dni są robocze.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                if (_scheduleManager.GetShiftById(shiftId).GetEmployees().Count() > 0)
                    liczbaDniRoboczych++;
            }

            //Liczymy sumę etatów.
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                sumaEtatow += Convert.ToInt32(wymiarEtatu[i]);

            //Obliczamy oczekiwaną liczbę funkcji dla każdgo pracownika.
            //Liczba wszystkich funkcji na wszystkich zmianach: 3 * liczbaDniRoboczych (po 3 funkcje na zmianie).
            //Liczba danego pracownika funkcji jest proporcjonalna do wymiaru etatu (3 * liczbaDniRoboczych * wymiarEtatu[i] / sumaEtatow).
            //Odejmujemy/dodajemy funkcje w zalezności od zaległości. Na koniec sprawdzamy, czy liczba mieści się w zakresie.
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                if (wymiarEtatu[i] > 0.000001)
                {
                    oczekiwanaLiczbaFunkcji[i] = Math.Min(Math.Max(((3 * liczbaDniRoboczych * wymiarEtatu[i] / sumaEtatow) - zaleglosci[i]), 0), MAX_LICZBA_FUNKCJI);
                    oczekiwanaLiczbaFunkcji[i] = Math.Max(oczekiwanaLiczbaFunkcji[i], MIN_LICZBA_FUNKCJI);
                }
            }
            return oczekiwanaLiczbaFunkcji;
        }

        //Algorytm optymalizacji genetycznej.
        public bool[] OptymalizacjaGA(int liczbaZmiennych, int liczbaOsobnikow, decimal tol, decimal tolX, int maxKonsekwentnychIteracji, int maxIteracji)
        {
            if (liczbaZmiennych < 1)
                throw new OptimizationInvalidDataException("Liczba zmiennych musi być dodatnia.");

            //Jeśli wybrano liczbę osobników mniejszą niż 10 to przypisz 10.
            if (liczbaOsobnikow < 10)
                liczbaOsobnikow = 10;

            //Jeśli tolerancja jest ujemna to ustawiamy 0
            if (tol < 0m)
            {
                tol = 0m;
                WarningRaised?.Invoke($"Podano ujemną wartość tol. Zmieniono na 0.");
            }

            //Jeśli tolerancjaX jest ujemna to ustawiamy 0
            if (tolX < 0m)
            {
                tolX = 0m;
                WarningRaised?.Invoke($"Podano ujemną wartość tolX. Zmieniono na 0.");
            }

            //Jeśli maxIteracji jest ujemne.
            if (maxIteracji < 0)
            {
                maxIteracji = 100;
                WarningRaised?.Invoke($"Podano ujemną wartość maxIteracji. Zmieniono na 100.");
            }

            //Jeśli maxKonsekwentnychIteracji jest ujemne.
            if (maxKonsekwentnychIteracji < 0)
            {
                maxKonsekwentnychIteracji = Math.Min(10, maxIteracji);
                WarningRaised?.Invoke($"Podano ujemną wartość maxKonsekwentnychIteracji. Zmieniono na {maxKonsekwentnychIteracji}.");
            }

            //Jeśli maxIteracji < maxKonsekwentnychIteracji.
            if (maxIteracji < maxKonsekwentnychIteracji)
            {
                WarningRaised?.Invoke($"Wartość maxIteracji ({maxIteracji}) jest mniejsza niż maxKonsekwentnychIteracji ({maxKonsekwentnychIteracji}).");
            }

            Random rnd = new Random();                                  //Instancja do losowania liczb.
            OsobnikComparer OsComp = new OsobnikComparer();             //Instancja do porównywania funkcji celu dwóch osobników.
            Osobnik[] osobniki = new Osobnik[liczbaOsobnikow];          //Aktualna iteracja osobników.
            Osobnik[] osobnikiTemp = new Osobnik[liczbaOsobnikow];      //Bufor trzymający osobniki z poprzedniej iteracji.

            const double SZANSA_MUTACJA = 0.004;                        //Szansa mutacji pojedynczego genu.
            const double SZANSA_KRZYZOWANIE = 0.5;                      //Szansa, że pomiędzy wylosowaną parą dojdzie do krzyżowania
            const double FRACTION_OF_ELITES = 0.01;                     //Odsetek osobników elitarnych.
            const double FRACTION_OF_REPRODUCING = 0.25;                //Odsetek osobników, które mogą uczestniczyć w krzyżowaniu/kopiowaniu.

            int nrKonsekwentnejIteracji = 1;                            //Nr aktualnej iteracji.
            int nrIteracji = 1;                                         //Określa od ilu iteracji nie nastapiła poprawa f. celu o conajmniej tolX.
            int liczbaWywolanFunkcjiCelu = 0;                           //Zlicza ile razy wywołano funkcję celu.
            decimal prevCel = 0;                                        //Najlepsza wartość funkcji celu w poprzedniej iteracji.
            decimal cel = 0;                                            //Najlepsza wartość funkcji celu w obecnej iteracji.
            double temp;                                                //Zmienna losowa z zakresu <0, 1) wykorzystywana w kilku miejsach programu. 
            double czyKrzyzowanie;                                      //Zmienna uzywana do określenia, czy dojdzie do krzyżowania pomiędzy dwoma wylosowanymi osobnikami.
            int nrPrzodka1 = 0;                                         //Nr 1 osobnika, który uczestniczy w krzyżowaniu.
            int nrPrzodka2 = 0;                                         //Nr 2 osobnika, który uczestniczy w krzyżowaniu.
            int liczbaReprodukujacych = Convert.ToInt32(Math.Floor(FRACTION_OF_REPRODUCING * Convert.ToDouble(liczbaOsobnikow)));                       //Liczba reprodukujących osobników.
            int liczbaElitarnych = Convert.ToInt32(Math.Max(Convert.ToInt32(Math.Floor(FRACTION_OF_ELITES * Convert.ToDouble(liczbaOsobnikow))), 1));   //Liczba elitarnych osobników.
            string raport;                                                              //Informacja o przebiegu procesu optymalizacji.
            double sumaSzans = 0.0;                                                     //1 + 2 + 3 + ... + liczbaReprodukujących.
            double[] szansa = new double[Convert.ToInt32(liczbaReprodukujacych)];       //Szansa danego osobnika na reprodukcję.

            //Obliczamy sumę szans.
            for (int i = 0; i < liczbaReprodukujacych; i++)
                sumaSzans += (i + 1);

            //Obliczamy szance kolejnych osobników. nty Osobnik zostaje wybrany, gdy liczba losowa jest mniejsza od ntej szansy, ale większa od (n-1)tej szansy.
            szansa[0] = liczbaReprodukujacych / sumaSzans;
            for (int i = 1; i < liczbaReprodukujacych; i++)
                szansa[i] = szansa[i - 1] + (liczbaReprodukujacych - i) / sumaSzans;

            //Generujemy losowe genomy początkowe.
            for (int j = 0; j < liczbaOsobnikow; j++)
            {
                bool[] bools = new bool[liczbaZmiennych];               //Genom osobnika.
                bool[] bools2 = new bool[liczbaZmiennych];              //Genom kopii osobnika.

                //Losujemy poszczególne geny.
                for (int i = 0; i < liczbaZmiennych; i++)
                {
                    temp = rnd.NextDouble();
                    if (temp < 0.5)
                    {
                        bools[i] = false;
                        bools2[i] = false;
                    }

                    else
                    {
                        bools[i] = true;
                        bools2[i] = true;
                    }
                }

                //Przypisujemy genomy początkowe do osobników i ich kopii.
                osobniki[j] = new Osobnik(bools, 0.0m);
                osobnikiTemp[j] = new Osobnik(bools2, 0.0m);
            }

            //Obliczamy f. celu dla populacji początkowej.
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                osobniki[i].wartosc = FunkcjaCelu(osobniki[i].genom);
                liczbaWywolanFunkcjiCelu++;
            }

            //Sortujemy osobniki malejąco i wybieramy najlepszą  wartość f. celu.
            Array.Sort(osobniki, OsComp);
            cel = osobniki[0].wartosc;

            //Wyznaczamy kolejne poppulacja dopóki:
            //1. Nie zostanie osiągnięta maksymalna liczba iteracji.
            //2. Nie zostanie osiągnięta maksymalna liczba konsekwentych iteracji bez poprawy f. celu.
            //3. Rozwiązanie nie będzie sięróżnić od celu o mniej niż tol + stopienZdegenerowania.
            while (nrIteracji <= maxIteracji && nrKonsekwentnejIteracji <= maxKonsekwentnychIteracji && (cel > stopienZdegenerowania + tol))
            {
                //Inkrementujemy numery iteracji i kopiujemy genomy populacji.
                nrIteracji++;
                nrKonsekwentnejIteracji++;
                for (int i = 0; i < liczbaOsobnikow; i++)
                    osobnikiTemp[i].genom = osobniki[i].genom;

                //Tworzenie nowej populacji przez krzyżowanie. Osobniki elitarne przechodzą do nowej populacji bez uszczerbku.
                for (int i = liczbaOsobnikow - 1; i >= liczbaElitarnych; i--)
                {
                    //Losujemy pierwszego przodka. Wybieramy największy indeks j taki, że szansa[j] <= temp.
                    temp = rnd.NextDouble();
                    for (int j = (liczbaReprodukujacych - 1); j > 0; j--)
                    {
                        if (szansa[j] <= temp)
                        {
                            nrPrzodka1 = j;
                            break;
                        }
                    }

                    //Losujemy drugiego przodka. Wybieramy największy indeks j taki, że szansa[j] <= temp.
                    temp = rnd.NextDouble();
                    for (int j = (liczbaReprodukujacych - 1); j > 0; j--)
                    {
                        if (szansa[j] <= temp)
                        {
                            nrPrzodka2 = j;
                            break;
                        }
                    }

                    //Sprawdzamy czy dojdzie do krzyżowania, czy do kopiowania.
                    czyKrzyzowanie = rnd.NextDouble();

                    if (czyKrzyzowanie < SZANSA_KRZYZOWANIE)
                    {
                        //Dla każdego genu złozonego z MAX_LICZBA_BITOW bitów wybieramy jednego z przodków, od którego gen będzie kopiowany.
                        for (int k = 0; k < liczbaZmiennych / MAX_LICZBA_BITOW; k++)
                        {
                            //Kopiowanie od pierwszego przodka.
                            temp = rnd.NextDouble();
                            if (temp < 0.5)
                            {
                                for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                {
                                    osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka1].genom[k * MAX_LICZBA_BITOW + m];
                                }
                            }

                            //Kopiowanie od drugiego przodka.
                            else
                            {
                                for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                {
                                    osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka2].genom[k * MAX_LICZBA_BITOW + m];
                                }
                            }
                        }
                    }

                    //Kopiowanie. Z prawdopodobieństwem 50% kopiujemy jednego z przodków, a z prawdopodobieństwem 50% drugiego z przodkuów.
                    else if (czyKrzyzowanie >= SZANSA_KRZYZOWANIE)
                    {
                        temp = rnd.NextDouble();
                        for (int k = 0; k < liczbaZmiennych / MAX_LICZBA_BITOW; k++)
                        {
                            //Kopiowanie pierwszego przodka.
                            if (temp < 0.5)
                            {
                                for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                    osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka1].genom[k * MAX_LICZBA_BITOW + m];
                            }

                            //Kopiowanie drugiego przodka.
                            else
                            {
                                for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                    osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka2].genom[k * MAX_LICZBA_BITOW + m];
                            }
                        }
                    }
                }

                // Mutacje wszystkich nieelitarnych osobników.
                for (int i = liczbaElitarnych; i < liczbaOsobnikow; i++)
                {
                    //Dla każdego genu, jeżeli wartość zmiennej jest temp mniejsza od szansy na mutację to zamień bit na przeciwny.
                    for (int j = 0; j < liczbaZmiennych; j++)
                    {
                        temp = rnd.NextDouble();
                        if (temp <= SZANSA_MUTACJA)
                        {
                            osobniki[i].genom[j] = !osobniki[i].genom[j];
                        }
                    }
                }

                //Obliczanie funkcji celu dla wszystkich osobników, z wyjątkiem elitarnych
                //(których nie trzeba obliczać, bo zostały z poprzedniej iteracji).
                Parallel.For(liczbaElitarnych, liczbaOsobnikow, i =>
                {
                    osobniki[i].wartosc = FunkcjaCelu(osobniki[i].genom);
                    liczbaWywolanFunkcjiCelu++;
                });

                //Sortowanie od najmniejsszej do największej wartości funkcji celu. Przypisanie nowej wartości do aktualnej najlepszej wartości
                //f. celu i najlepszej wartości f. celu w poprzedniej iteracji. Porównanie tych wartości i jesli aktualna jest mniejsza wyzerowanie
                //licznika kolejnych iteracji bez poprawy.
                Array.Sort(osobniki, OsComp);
                prevCel = cel;
                cel = osobniki[0].wartosc;
                if (Math.Abs(prevCel - cel) > tolX)
                    nrKonsekwentnejIteracji = 0;

                //Odświeżanie etykiety labelRaport co 100 iteracji.
                if (nrIteracji % 100 == 0)
                {
                    raport = "Wartość f. celu: " + cel.ToString()
                           + " Nr. Iteracji: " + nrIteracji.ToString()
                           + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString()
                           + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString()
                           + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                    ProgressUpdated?.Invoke(raport);
                }
            }

            //Odświeżanie etykiety labelRaport po zakończeniu optymalizacji.
            //Wyświetla infomrację, jeśli nie udało się osiągnąć celu optymalizacji.
            if (cel > stopienZdegenerowania + tol)
            {
                raport = "Wartość f. celu: " + cel.ToString()
                           + " Nr. Iteracji: " + nrIteracji.ToString()
                           + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString()
                           + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString()
                           + " Cel: " + (stopienZdegenerowania + tol).ToString() + "."
                           + " Cel nie został osiągnięty. Rozważ ponowne rozdzielenie funkcji.";
                ProgressUpdated?.Invoke(raport);
            }

            //Wyświetla informację, gdy cel został osiągnięty.
            else
            {
                raport = "Wartość f. celu: " + cel.ToString()
                           + " Nr. Iteracji: " + nrIteracji.ToString()
                           + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString()
                           + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString()
                           + " Cel: " + (stopienZdegenerowania + tol).ToString() + "."
                           + " Ukończono.";
                ProgressUpdated?.Invoke(raport);
            }

            //Funkcja zwraca genom najlepszego osobnika.
            return osobniki[0].genom;
        }

        //Przygotowanie danych do optymalizacji.
        public void Prepare()
        {
            //Sprawdzamy, czy liczba pracowników na każdej zmianie wynosi 0 lub od 3 do MAX_LICZBA_DYZUROW.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                if (_scheduleManager.GetShiftById(shiftId).GetEmployees().Count() == 1 || _scheduleManager.GetShiftById(shiftId).GetEmployees().Count() == 2)
                    throw new OptimizationInvalidScheduleException($"Za mało pracowników na zmianie: {shiftId}.");

                if (_scheduleManager.GetShiftById(shiftId).GetEmployees().Count() > MAX_LICZBA_DYZUROW)
                    throw new OptimizationInvalidScheduleException($"Za dużo pracowników na zmianie: {shiftId}.");
            }

            //Obliczamy zaległości i wymiar etatu.
            wymiarEtatu = new double[MAX_LICZBA_OSOB];                  //Wymiar etatu.
            zaleglosci = new double[MAX_LICZBA_OSOB];                   //Zaległości.
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                //Dla każdego pracownika w systemie pobieramy wymiar etatu i zaległości.
                if (_employeeManager.GetEmployeeById(i + 1) != null)
                {
                    wymiarEtatu[i] = _employeeManager.GetEmployeeById(i + 1).WymiarEtatu;
                    zaleglosci[i] = _employeeManager.GetEmployeeById(i + 1).Zaleglosci;
                }

                //Jeśli pracownika nie ma to wpisujemy 0.
                else
                {
                    wymiarEtatu[i] = 0.0;
                    zaleglosci[i] = 0;
                }
            }

            //Przygotowujemy resztę danych.
            grafikDyzurow = UtworzGrafik();                                      //Przygotowujemy grafik.
            nieTriazDzien = ListaNieTiazDzien();                                //Przygotowujemy listę osób, które nie powinny być na triażu w ciau dnia.
            nieTriazNoc = ListaNieTiazNoc();                                    //Przygotowujemy listę osób, które nie powinny być na triażu w ciau nocy.
            liczbaDyzurow = LiczbaDyzurowNaZmianie();                                    //Przygotowujemy liczbę dyżurów każdego dnia.
            oczekiwanaLiczbaFunkcji = OczekiwanaLiczbaFunkcji();                //Przygotowujemy liczbę funkcji każdego pracownika.
            stopienZdegenerowania = StopienZdegenerowania();                    //Wyznaczamy stopień zdegenerowania.
        }

        //Określa poniżej jakiej wartości funkcji celu nie można zejść ze względu na grafik.
        private decimal StopienZdegenerowania()
        {
            decimal stopienZdegenerowania = 0.0m;                       //Stopień zdegenerowania grafiku.
            int liczbaStazystowDzien;                                   //Liczba stażystów, którzy nie mogą pełnić triażu za dnia.
            int liczbaStazystowNoc;                                     //Liczba stażystów, którzy nie mogą pełnić triażu w nocy.

            //Dla każdej zmiany sprawdzamy, czy grafik uniemożliwia uzyskanie rozwiązania dopuszczalnego.
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                //Sprawdzamy, czy na zmianie są pracownicy.
                if (liczbaDyzurow[i] > 0)
                {
                    //Zerujemy liczby stażystów.
                    liczbaStazystowDzien = 0;
                    liczbaStazystowNoc = 0;

                    //Sprawdzamy ilu stażystów danego typu jest na danej zmianie
                    for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                    {
                        if (nieTriazDzien.Contains(grafikDyzurow[j + i * MAX_LICZBA_DYZUROW]))
                            liczbaStazystowDzien++;

                        if (nieTriazNoc.Contains(grafikDyzurow[j + i * MAX_LICZBA_DYZUROW]))
                            liczbaStazystowNoc++;
                    }

                    //Jeśli na danej zmianie są tylko stażyści to naliczamy dużą karę.
                    //Zakładamy, że każda osoba nie mogąca pełnić triażu za dnia nie może go też pełnić w nocy.
                    if (liczbaDyzurow[i] == liczbaStazystowNoc)
                        stopienZdegenerowania += 10000.0m;

                    //Jeżeli zmiana jest dzienna i mamy tylko stażystów, którrzy nie mogą pełnić triażu na dziennej zmianie to naliczamy dwie mniejsze kary.
                    if ((liczbaDyzurow[i] == liczbaStazystowDzien) && i < LICZBA_DNI)
                        stopienZdegenerowania += +200.0m;

                    //Jeśli zmiana jest dzienna i jet tylko jedna osoba, która może pełnić triaż to naliczamy mniejszą karę.
                    else if ((liczbaDyzurow[i] - liczbaStazystowDzien == 1) && i < LICZBA_DNI)
                        stopienZdegenerowania += +100.0m;

                    //Jeżeli zmiana jest nocna i mamy tylko stażystów, którrzy nie mogą pełnić triażu na nocnej zmianie to naliczamy dwie mniejsze kary.
                    if ((liczbaDyzurow[i] == liczbaStazystowNoc) && i >= LICZBA_DNI)
                        stopienZdegenerowania += +200.0m;

                    //Jeśli zmiana jest nocna i jet tylko jedna osoba, która może pełnić triaż to naliczamy mniejszą karę.
                    else if ((liczbaDyzurow[i] - liczbaStazystowNoc == 1) && i >= LICZBA_DNI)
                        stopienZdegenerowania += +100.0m;
                }
            }

            //Zwracamy stopień zdegenerowania.
            return stopienZdegenerowania;
        }

        //Tworzymy tablicę z grafikiem dyzurów i usuwamy przypisane funkcje, jeśli jakieś były przed startem optymalizacji.
        private int[] UtworzGrafik()
        {
            int nrZmiany;                                                           //Numer zmiany.
            int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];      //Grafik. Tablica zawiera numery pracowników. Pierwsze MAX_LICZBA_DYZUROW pól
                                                                                    //zawiera numery pracowników na zmianie dziennej 1 dnia, kolejne na zmianie dziennej 2 dnia itd.
                                                                                    //Jeśli liczba pracowników jest mniejsza niż maksymalna, to pozostałe pola są zerowane.
            
            //Dla każdego zmiany dyżuru pobieramy numer i wpisujemy we właściwe miejsce tablicy.
            for (int nrDyzuru = 0; nrDyzuru < 2 * LICZBA_DNI * MAX_LICZBA_DYZUROW; nrDyzuru++)
            {
                nrZmiany = Convert.ToInt32(Math.Floor(Convert.ToDouble(nrDyzuru) / MAX_LICZBA_DYZUROW));        //Numer zmiany. Od 0 do LICZBA_ZMIAN * LICZBA_DNI - 1.
                
                //Pobieramy numery pracowników z danej zmiany.
                if (_scheduleManager.GetShiftById(nrZmiany).GetEmployees().Count() > nrDyzuru % MAX_LICZBA_DYZUROW)
                {
                    //Czyścimy funkcje pracowników na danej zmianie, jeśli rozpoczęto optymalizację bez usunięcia funkcji.
                    _scheduleManager.AssignFunctionToEmployee(nrZmiany, _scheduleManager.GetShiftById(nrZmiany).GetEmployees().ToList()[nrDyzuru % MAX_LICZBA_DYZUROW].Numer, FunctionTypes.Bez_Funkcji);
                    
                    //Wpisujemy numer pracownika do tablicy.
                    dyzuryGrafik[nrDyzuru] = _scheduleManager.GetShiftById(nrZmiany).GetEmployees().ToList()[nrDyzuru % MAX_LICZBA_DYZUROW].Numer;
                }

                //Jeśli nie ma już pracowników na zmianie to wpisujemy 0.
                else
                    dyzuryGrafik[nrDyzuru] = 0;
            }

            //Zwracamy grafik.
            return dyzuryGrafik;
        }
    }
}
