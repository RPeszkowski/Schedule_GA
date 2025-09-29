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

        private decimal FunkcjaCelu(bool[] funkcje)
        {
            decimal W = 0.0m;
            decimal a = 0.0m;
            int[] liczbaStazystowNaTriazu = new int[2 * LICZBA_DNI];
            int[] liczbaSalOsobaDzien = new int[MAX_LICZBA_OSOB];
            int[] liczbaSalOsobaNoc = new int[MAX_LICZBA_OSOB];
            int[] liczbaTriazyOsobaDzien = new int[MAX_LICZBA_OSOB];
            int[] liczbaTriazyOsobaNoc = new int[MAX_LICZBA_OSOB];
            bool[] numerOsoby = new bool[MAX_LICZBA_BITOW];
            int nrOsobySala;
            int nrOsobyTriaz1;
            int nrOsobyTriaz2;
            bool[][] dyzuryRozstaw = new bool[MAX_LICZBA_OSOB][];
            int[] nrDyzuru = new int[MAX_LICZBA_OSOB];
            int liczbaKonsekwentnychBezFunkcji = 0;

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                dyzuryRozstaw[i] = new bool[Convert.ToInt32(wymiarEtatu[i])];
                nrDyzuru[i] = 0;

                liczbaSalOsobaDzien[i] = 0;
                liczbaSalOsobaNoc[i] = 0;
                liczbaTriazyOsobaDzien[i] = 0;
                liczbaTriazyOsobaNoc[i] = 0;
            }

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                liczbaStazystowNaTriazu[i] = 0;
            }

            for (int i = 0; i < LICZBA_DNI; i++)
            {
                nrOsobySala = 0;
                nrOsobyTriaz1 = 0;
                nrOsobyTriaz2 = 0;
                // Dzien
                if (liczbaDyzurow[i] > 0)
                {
                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobySala = (nrOsobySala * 2) + (funkcje[3 * i * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz1 = (nrOsobyTriaz1 * 2) + (funkcje[3 * i * MAX_LICZBA_BITOW + MAX_LICZBA_BITOW + j] ? 1 : 0);

                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz2 = (nrOsobyTriaz2 * 2) + (funkcje[3 * i * MAX_LICZBA_BITOW + 2 * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    if (nrOsobySala == nrOsobyTriaz1)
                        a += 1000000.0m;

                    if (nrOsobySala == nrOsobyTriaz2)
                        a += 1000000.0m;

                    if (nrOsobyTriaz1 == nrOsobyTriaz2)
                        a += 1000000.0m;

                    if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobySala] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaSalOsobaDzien[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1][nrDyzuru[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]] = true;
                    }

                    if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaTriazyOsobaDzien[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1][nrDyzuru[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]] = true;
                    }

                    if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaTriazyOsobaDzien[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1][nrDyzuru[grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]] = true;
                    }

                    for (int j = 0; j < nieTriazDzien.Length; j++)
                    {
                        if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazDzien[j] || grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazDzien[j])
                            a += 100.0m;
                    }

                    for (int j = 0; j < nieTriazNoc.Length; j++)
                    {
                        if (grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || grafikDyzurow[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                            liczbaStazystowNaTriazu[i]++;
                    }

                    for (int j = i * MAX_LICZBA_DYZUROW; j < (i + 1) * MAX_LICZBA_DYZUROW; j++)
                    {
                        if (grafikDyzurow[j] != 0)
                            nrDyzuru[grafikDyzurow[j] - 1]++;
                    }
                }

                nrOsobySala = 0;
                nrOsobyTriaz1 = 0;
                nrOsobyTriaz2 = 0;
                //Noc
                if (liczbaDyzurow[i + LICZBA_DNI] > 0)
                {
                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobySala = (nrOsobySala * 2) + (funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz1 = (nrOsobyTriaz1 * 2) + (funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + MAX_LICZBA_BITOW + j] ? 1 : 0);

                    for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                        nrOsobyTriaz2 = (nrOsobyTriaz2 * 2) + (funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + 2 * MAX_LICZBA_BITOW + j] ? 1 : 0);

                    if (nrOsobySala == nrOsobyTriaz1)
                        a += 1000000.0m;

                    if (nrOsobySala == nrOsobyTriaz2)
                        a += 1000000.0m;

                    if (nrOsobyTriaz1 == nrOsobyTriaz2)
                        a += 1000000.0m;

                    if (grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaSalOsobaNoc[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1][nrDyzuru[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]] = true;
                    }

                    if (grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaTriazyOsobaNoc[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1][nrDyzuru[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]] = true;
                    }

                    if (grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == 0)
                        a += 1000000.0m;

                    else
                    {
                        liczbaTriazyOsobaNoc[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]++;
                        dyzuryRozstaw[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1][nrDyzuru[grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]] = true;
                    }

                    for (int j = 0; j < nieTriazNoc.Length; j++)
                    {
                        if (grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || grafikDyzurow[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                        {
                            liczbaStazystowNaTriazu[(i + LICZBA_DNI)]++;
                            a += 100.0m;
                        }
                    }

                    for (int j = (i + LICZBA_DNI) * MAX_LICZBA_DYZUROW; j < (i + LICZBA_DNI + 1) * MAX_LICZBA_DYZUROW; j++)
                    {
                        if (grafikDyzurow[j] != 0)
                            nrDyzuru[grafikDyzurow[j] - 1]++;
                    }
                }
            }

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                if (liczbaStazystowNaTriazu[i] >= 2)
                    a += 10000.0m;
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                if (Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] + liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i] - oczekiwanaLiczbaFunkcji[i]) >= 2)
                    W += 0.01m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] + liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i] - oczekiwanaLiczbaFunkcji[i])));

                else if (Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] + liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i] - oczekiwanaLiczbaFunkcji[i]) >= 1)
                    W += 0.0000001m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] + liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i] - oczekiwanaLiczbaFunkcji[i])));

                if (Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] - (liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i])) > 2)
                    W += 1.0m * Convert.ToDecimal(Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] - (liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i])));

                else if (Math.Abs(liczbaSalOsobaDzien[i] + liczbaTriazyOsobaDzien[i] - (liczbaSalOsobaNoc[i] + liczbaTriazyOsobaNoc[i])) == 2)
                    W += 0.0002m;

                liczbaKonsekwentnychBezFunkcji = 0;
                for (int j = 0; j < dyzuryRozstaw[i].Length; j++)
                {
                    if (!dyzuryRozstaw[i][j])
                        liczbaKonsekwentnychBezFunkcji++;

                    else if (dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji > 3)
                    {
                        W += 0.00000001m * Convert.ToDecimal(liczbaKonsekwentnychBezFunkcji);
                        liczbaKonsekwentnychBezFunkcji = 0;
                    }

                    else if (dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji <= 3)
                        liczbaKonsekwentnychBezFunkcji = 0;

                    if (j == dyzuryRozstaw[i].Length - 1 && !dyzuryRozstaw[i][j] && liczbaKonsekwentnychBezFunkcji > 3)
                        W += 0.00000001m * Convert.ToDecimal(liczbaKonsekwentnychBezFunkcji);
                }
            }

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
                if (_scheduleManager.GetShiftById(shiftId).PresentEmployees.Count() > 0)
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
                if (_scheduleManager.GetShiftById(shiftId).PresentEmployees.Count() == 1 || _scheduleManager.GetShiftById(shiftId).PresentEmployees.Count() == 2)
                    throw new OptimizationInvalidScheduleException($"Za mało pracowników na zmianie: {shiftId}.");

                if (_scheduleManager.GetShiftById(shiftId).PresentEmployees.Count() > MAX_LICZBA_DYZUROW)
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

        private decimal StopienZdegenerowania()
        {
            decimal stopienZdegenerowania = 0.0m;
            int liczbaStazystowDzien;
            int liczbaStazystowNoc;
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                if (liczbaDyzurow[i] > 0)
                {
                    liczbaStazystowDzien = 0;
                    liczbaStazystowNoc = 0;
                    for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                    {
                        if (nieTriazDzien.Contains(grafikDyzurow[j + i * MAX_LICZBA_DYZUROW]))
                            liczbaStazystowDzien++;

                        if (nieTriazNoc.Contains(grafikDyzurow[j + i * MAX_LICZBA_DYZUROW]))
                            liczbaStazystowNoc++;
                    }

                    if (liczbaDyzurow[i] == liczbaStazystowNoc)
                        stopienZdegenerowania += 10000.0m;

                    if ((liczbaDyzurow[i] == liczbaStazystowDzien) && i < LICZBA_DNI)
                        stopienZdegenerowania += +200.0m;

                    else if ((liczbaDyzurow[i] - liczbaStazystowDzien == 1) && i < LICZBA_DNI)
                        stopienZdegenerowania += +100.0m;

                    if ((liczbaDyzurow[i] == liczbaStazystowNoc) && i >= LICZBA_DNI)
                        stopienZdegenerowania += +200.0m;

                    else if ((liczbaDyzurow[i] - liczbaStazystowNoc == 1) && i >= LICZBA_DNI)
                        stopienZdegenerowania += +100.0m;
                }
            }
            return stopienZdegenerowania;
        }

        private int[] UtworzGrafik()
        {
            int nrZmiany;
            int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];

            for (int nrDyzuru = 0; nrDyzuru < 2 * LICZBA_DNI * MAX_LICZBA_DYZUROW; nrDyzuru++)
            {
                nrZmiany = Convert.ToInt32(Math.Floor(Convert.ToDouble(nrDyzuru) / MAX_LICZBA_DYZUROW));
                if (_scheduleManager.GetShiftById(nrZmiany).PresentEmployees.Count() > nrDyzuru % MAX_LICZBA_DYZUROW)
                {
                    _scheduleManager.AssignFunctionToEmployee(nrZmiany, _scheduleManager.GetShiftById(nrZmiany).PresentEmployees[nrDyzuru % MAX_LICZBA_DYZUROW].Numer, FunctionTypes.Bez_Funkcji);
                    dyzuryGrafik[nrDyzuru] = _scheduleManager.GetShiftById(nrZmiany).PresentEmployees[nrDyzuru % MAX_LICZBA_DYZUROW].Numer;
                }

                else
                    dyzuryGrafik[nrDyzuru] = 0;
            }

            return dyzuryGrafik;
        }
    }
}
