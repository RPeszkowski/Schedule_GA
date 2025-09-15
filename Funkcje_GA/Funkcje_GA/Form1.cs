using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using static Funkcje_GA.Form1;

namespace Funkcje_GA

{
    public partial class Form1 : Form
    {
        //Ta klasa odpowiada za dodawanie, usuwanie i edytowanie danych o pracownikach.
        public class EmployeeManagement
        {
            //Usuwanie pracownika.
            public static void EmployeeDelete(Osoba osoba)
            {
                //Próbujemy usunąć osobę z grafiku.
                try
                {
                    //Usuwamy osobę z grafiku.
                    for (int i = 0; i < LICZBA_DNI; i++)
                    {
                        //Usuwamy dyżury dzienne.
                        for (int j = 0; j < listBoxesDzien[i].Items.Count; j++)
                        {

                            if (listBoxesDzien[i].GetNumber(j) == osoba.numer)
                            {
                                listBoxesDzien[i].Items.RemoveAt(j);
                                listBoxesDzien[i].Refresh();
                            }
                        }

                        //Usuwamy dyżury nocne.
                        for (int j = 0; j < listBoxesNoc[i].Items.Count; j++)
                        {
                            if (listBoxesNoc[i].GetNumber(j) == osoba.numer)
                            {
                                listBoxesNoc[i].Items.RemoveAt(j);
                                listBoxesNoc[i].Refresh();
                            }
                        }
                    }

                    //Usuwamy etykietę, dekrementujemy ilość osób, usuwamy osobę, na koniec wyświetlamy komunikat.
                    labelsPracownicy[osoba.numer - 1].Text = "";
                    liczbaOsob--;
                    osoby[osoba.numer - 1] = null;
                    MessageBox.Show("Usunięto dane pracownika.");
                }

                //Jeśli się nie udało, to wyświetlamy komunikat.
                catch { MessageBox.Show("Nie udało się usunąć osoby."); }
            }

            //Dodawanie nowego pracownika.
            public static void EmployeeAdd(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Dodawanie pracownika o konkretnym numerze.
                //Sprawdzamy, czy nie osiągnięto maksymalne liczby pracowników.
                if (liczbaOsob >= MAX_LICZBA_OSOB)
                    throw new TooManyEmployeesException("Liczba osób wynosi obecnie " + liczbaOsob.ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

                //Sprawdzamy, czy numer osoby jest poprawny.
                if (numer > 0 && numer >= MAX_LICZBA_OSOB)
                    throw new InvalidDataException("Numer osoby wynosił " + numer.ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

                //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
                if (imie.Contains(' ') || nazwisko.Contains(' '))
                    throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

                //Sprawdzamy, czy imię i nazwisko nie są puste.
                else if (imie == "" || nazwisko == "")
                    throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

                //Tworzymy nową osobę, sprawdzamy, czy numer jest poprawny, dodajemy do tabeli, inkrementujemy liczbę osóbi pdświeżamy etykietę.
                Osoba newOsoba = new Osoba(numer, imie, nazwisko, wymiarEtatu, zaleglosci, czyTriazDzien, czyTriazNoc);
                osoby[numer - 1] = newOsoba;
                liczbaOsob = liczbaOsob++;
                UpdateEmployeeLabel(osoby[numer - 1]);
            }
            public static void EmployeeAdd(string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Dodawanie pracownika o pierwszym wolnym numerze.
                //Sprawdzamy, czy nie osiągnięto maksymalne liczby pracowników.
                if (liczbaOsob >= MAX_LICZBA_OSOB)
                    throw new TooManyEmployeesException("Liczba osób wynosi obecnie " + liczbaOsob.ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

                //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
                if (imie.Contains(' ') || nazwisko.Contains(' '))
                    throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

                //Sprawdzamy, czy imię i nazwisko nie są puste.
                else if (imie == "" || nazwisko == "")
                    throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

                //Szukamy wolnego numeru.
                int wolnyNumer = MAX_LICZBA_OSOB - 1;
                for (int i = MAX_LICZBA_OSOB - 1; i >= 0; i--)
                {
                    if (osoby[i] == null)
                        wolnyNumer = i;
                }

                //Tworzymy nową osobę, sprawdzamy, dodajemy do tabeli, inkrementujemy liczbę osób i pdświeżamy etykietę.
                Osoba newOsoba = new Osoba(wolnyNumer + 1, imie, nazwisko, wymiarEtatu, zaleglosci, czyTriazDzien, czyTriazNoc);
                osoby[wolnyNumer] = newOsoba;
                liczbaOsob = liczbaOsob++;
                UpdateEmployeeLabel(osoby[wolnyNumer]);
            }

            //Edycja danych pracownika.
            public static void EmployeeEdit(Osoba osoba, double wymiarEtatu)
            {
                //Edycja danych jednej osoby. Tylko wymiar etatu.
                if (osoba != null)
                {
                    osoba.wymiarEtatu = wymiarEtatu;
                    UpdateEmployeeLabel(osoba);
                }
            }
            public static void EmployeeEdit(Osoba[] osoby, double wymiarEtatu)
            {
                //Edycja danych wszystkich osób. Tylko wymiar etatu.
                foreach (Osoba osoba in osoby)
                {
                    if (osoba != null)
                        osoba.wymiarEtatu = wymiarEtatu;
                }

                UpdateEmployeeLabel(osoby);
            }
            public static void EmployeeEdit(Osoba osoba, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Edycja danych jednej osoby. Pełne dane.
                //Sprawdzamy, czy osoba istnieje.
                if (osoba == null)
                    throw new NullReferenceException("Dana osoba nie istnieje");

                //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
                if (imie.Contains(' ') || nazwisko.Contains(' '))
                    throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

                //Sprawdzamy, czy imię i nazwisko nie są puste.
                else if (imie == "" || nazwisko == "")
                    throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

                osoba.imie = imie;
                osoba.nazwisko = nazwisko;
                osoba.wymiarEtatu = wymiarEtatu;
                osoba.zaleglosci = zaleglosci;
                osoba.czyTriazDzien = czyTriazDzien;
                osoba.czyTriazNoc = czyTriazNoc;
                UpdateEmployeeLabel(osoba);
            }

            //Wyświetlanie informacji o pracowniku na etykiecie.
            public static void UpdateEmployeeLabel(Osoba osoba)
            {
                //Aktualizujemy pojedynczą etykietę.
                labelsPracownicy[osoba.numer - 1].Text = osoba.numer.ToString() + ". " + osoba.imie + " " + osoba.nazwisko + " " + osoba.wymiarEtatu.ToString() + " " + osoba.zaleglosci.ToString();

                //Jeśli osoba jest stażystą to podświetlamy.
                if (!(osoba.czyTriazDzien && osoba.czyTriazNoc))
                    labelsPracownicy[osoba.numer - 1].ForeColor = Color.Orange;

                else
                    labelsPracownicy[osoba.numer - 1].ForeColor = Color.Black;
            }
            public static void UpdateEmployeeLabel(Osoba[] osoby)
            {
                //Aktualizujemy wszystkie etykiety.
                foreach (Osoba osoba in osoby)
                {
                    if (osoba != null)
                    {
                        //Aktualizujemy wszystkie etykiety.
                        labelsPracownicy[osoba.numer - 1].Text = osoba.numer.ToString() + ". " + osoba.imie + " " + osoba.nazwisko + " " + osoba.wymiarEtatu.ToString() + " " + osoba.zaleglosci.ToString();

                        //Jeśli osoba jest stażystą to podświetlamy.
                        if (!(osoba.czyTriazDzien && osoba.czyTriazNoc))
                            labelsPracownicy[osoba.numer - 1].ForeColor = Color.Orange;

                        else
                            labelsPracownicy[osoba.numer - 1].ForeColor = Color.Black;
                    }
                }
            }
        }

        private class FileManagementGrafik
        {
            public static void WczytajGrafik(string plik)
            {
                string nrOsobNaJednejZmianie;
                string[] osobyNaZmianieSplit;
                string nrOsobyZFunkcja;
                string nrOsobyBezFunkcji;
                int nrOsoby;
                bool flagBreak = false;
                bool flagWarningBrakOsoby = false;

                UsunGrafik();

                try
                {
                    for (int i = 0; i < 2 * LICZBA_DNI; i++)
                    {
                        nrOsobNaJednejZmianie = File.ReadAllLines(plik).Skip(i).Take(1).First();
                        osobyNaZmianieSplit = nrOsobNaJednejZmianie.Split(' ');

                        for (int nrDyzuru = 0; nrDyzuru < osobyNaZmianieSplit.Length - 1; nrDyzuru++)
                        {
                            nrOsobyZFunkcja = osobyNaZmianieSplit[nrDyzuru];

                            try
                            {
                                if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 's' || nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 't')
                                    nrOsobyBezFunkcji = nrOsobyZFunkcja.Remove(nrOsobyZFunkcja.Length - 1);

                                else
                                    nrOsobyBezFunkcji = nrOsobyZFunkcja;

                                nrOsoby = Convert.ToInt32(nrOsobyBezFunkcji);

                                if (nrOsoby < 1 || nrOsoby > MAX_LICZBA_OSOB)
                                    throw new IndexOutOfRangeException("Numer osoby musi być liczbą naturalną z zakresu 1 - 50.");

                                if (osoby[nrOsoby - 1] == null)
                                    flagWarningBrakOsoby = true;

                                else
                                    EmployeeManagement.EmployeeEdit(osoby[nrOsoby - 1], osoby[nrOsoby - 1].wymiarEtatu + 1.0);
                            }

                            catch
                            {
                                if (i < LICZBA_DNI)
                                    MessageBox.Show("Nie udało się wczytać grafiku dla dnia: " + (i + 1).ToString() + " dyżur dzienny.");

                                else
                                    MessageBox.Show("Nie udało się wczytać grafiku dla dnia: " + (i + 1).ToString() + " dyżur nocny.");

                                listBoxesDzien[i].Items.Clear();
                                flagBreak = true;
                                break;
                            }

                            if (i < LICZBA_DNI)
                                listBoxesDzien[i].Items.Add(osobyNaZmianieSplit[nrDyzuru]);

                            else
                                listBoxesNoc[i - LICZBA_DNI].Items.Add(osobyNaZmianieSplit[nrDyzuru]);
                        }

                        if (flagBreak)
                            break;
                    }
                }

                catch
                {
                    MessageBox.Show("Nie udało się wczytać grafiku.");
                    UsunGrafik();
                }

                finally
                {
                    if (flagWarningBrakOsoby)
                        MessageBox.Show("W grafiku są osoby, których nie ma w aktualnej bazie pracowników.");
                }
            }
            public static void ZapiszGrafik(string plik)
            {
                File.WriteAllText(plik, "");
                foreach (ListBoxGrafik listBoxDzien in listBoxesDzien)
                {
                    string str = "";
                    for (int i = 0; i < listBoxDzien.Items.Count; i++)
                        str = str + listBoxDzien.Items[i].ToString() + " ";

                    str += "\n";
                    File.AppendAllText(plik, str);
                }

                foreach (ListBoxGrafik listBoxNoc in listBoxesNoc)
                {
                    string str = "";
                    for (int i = 0; i < listBoxNoc.Items.Count; i++)
                        str = str + listBoxNoc.Items[i].ToString() + " ";

                    str += "\n";
                    File.AppendAllText(plik, str);
                }
            }
        }

        //Klasa odpowiada za zapis i odczyt danych pracowników do pliku "Pracownicy.txt".
        public class FileManagementPracownicy
        {
            //Wczytujemy dane pracowników z pliku "Pracownicy.txt".
            public static void WczytajPracownikow(string plik)
            {
                for (int nrLinii = 0; nrLinii < MAX_LICZBA_OSOB; nrLinii++)
                {
                    string wczytanaLinia;                   //Wczytana linijka.
                    string[] LiniaSplit;                    //Wczytana linijka, wyrazy oddzielone spacją.

                    //Wczytujemy linijkę tekstu i rozdzielamy wyrazy.
                    wczytanaLinia = File.ReadAllLines(plik).Skip(nrLinii).Take(1).First();
                    LiniaSplit = wczytanaLinia.Split(' ');

                    //Próbujemy sprawdzić numer wczytanego pracownika.
                    if (Int32.TryParse(LiniaSplit[pracownicy_txt["NUMER"]], out int numer))
                    {
                        //Próbujemy dodać nowego pracownika.
                        try
                        {
                            //Dodajemy nowego pracownika.
                            EmployeeManagement.EmployeeAdd(numer, LiniaSplit[pracownicy_txt["IMIE"]], LiniaSplit[pracownicy_txt["NAZWISKO"]], 0.0,
                            Convert.ToInt32(LiniaSplit[pracownicy_txt["ZALEGLOSCI"]]), Convert.ToBoolean(LiniaSplit[pracownicy_txt["TRIAZ_DZIEN"]]),
                            Convert.ToBoolean(LiniaSplit[pracownicy_txt["TRIAZ_NOC"]]));
                        }

                        //Obsługa wyjątku: osiągnięto maksymalną liczbę pracowników.
                        catch (TooManyEmployeesException)
                        {
                            MessageBox.Show("Plik Pracownicy.txt jest uszkodzony. Napraw go lub usuń (osiągnięto maksymalna liczbę pracowników).");
                            return;
                        }

                        //Obsługa wyjątku: zły format danych.
                        catch (InvalidDataException)
                        {
                            MessageBox.Show("Plik Pracownicy.txt jest uszkodzony. Napraw go lub usuń (imię i nazwisko nie mogą zwierać spacji ani być puste, numer pracownika nie może być większy niż maksymalna liczba pracowników ani mniejszy niż 0).");
                            return;
                        }

                        //Obsługa wyjątku: plik jest uszkodzony.
                        catch
                        {
                            MessageBox.Show("Plik Pracownicy.txt jest uszkodzony. Napraw go lub usuń.");
                            return;
                        }
                    }

                    //Jeśli w miejscu numeru jest coś innego niż numer lub puste miejsce to wyświetlamy komunikat.
                    else if (LiniaSplit[0] != "")
                    {
                        MessageBox.Show("Plik Pracownicy.txt jest uszkodzony. Napraw go lub usuń.");
                        return;
                    }
                }
            }

            //Zapisywanie danych o pracownikach do pliku "Pracownicy.txt".
            public static void ZapiszPracownikow(string plik)
            {
                //Tworzymy pusty plik lub czyścimy istniejący.
                File.WriteAllText(plik, "");

                //Dla każdej osoby (jeśli istnieje) dopisujemy nową linijkę do pliku z danymi pracownika. Jeśli nie istnieje wpisujemy pustą linijkę.
                for (int nrOsoby = 0; nrOsoby < MAX_LICZBA_OSOB; nrOsoby++)
                {
                    //Jeśli osoba istnieje to wpisz dane.
                    if (osoby[nrOsoby] != null)
                    {
                        string danePracownika = osoby[nrOsoby].numer.ToString() + " " + osoby[nrOsoby].imie + " " + osoby[nrOsoby].nazwisko + " "
                                              + osoby[nrOsoby].zaleglosci.ToString() + " " + osoby[nrOsoby].czyTriazDzien.ToString() + " "
                                              + osoby[nrOsoby].czyTriazNoc.ToString() + "\n";
                        File.AppendAllText(plik, danePracownika);
                    }

                    //Jeśli osoba nie istnieje to wpisz pustą linijkę.
                    else
                        File.AppendAllText(plik, "\n");
                }
            }
        }

        private class ListBoxGrafik : ListBox
        {
            public int GetFunction(int index)
            {
                int nrFunkcji = -1;
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        nrFunkcji = 1;

                    else if (str[str.Length - 1] == 't')
                        nrFunkcji = 2;

                    else
                    {
                        try
                        {
                            Convert.ToInt32(str);
                            nrFunkcji = 0;
                        }
                        catch { }
                    }
                }
                return nrFunkcji;
            }

            public int GetNumber(int index)
            {
                int number;
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        str = str.Remove(str.Length - 1);

                    if (str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    try
                    {
                        number = Convert.ToInt32(str);
                    }

                    catch { number = -1; }
                    ;
                }
                else number = -1;

                return number;
            }

            public void ToBezFunkcji(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    this.Items[index] = str;
                }
            }

            public void ToSala(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        this.Items[index] = str;

                    else if (str[str.Length - 1] == 't')
                    {
                        str = str.Remove(str.Length - 1);
                        this.Items[index] = str + 's';
                    }
                    else this.Items[index] = str + 's';
                }
            }

            public void ToTriaz(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 't')
                        this.Items[index] = str;

                    else if (str[str.Length - 1] == 's')
                    {
                        str = str.Remove(str.Length - 1);
                        this.Items[index] = str + 't';
                    }
                    else this.Items[index] = str + 't';
                }
            }

        }

        //Obiekty tej klasy to pracownicy.
        public class Osoba
        {
            public int numer;               //Numer osoby.
            public string imie;             //Imię osoby.
            public string nazwisko;         //Nazwisko osoby.
            public double wymiarEtatu;      //Wymiar etatu osoby.
            public int zaleglosci;          //Zaległości osoby.
            public bool czyTriazDzien;      //Czy osobie można przydzielać triaż na dziennej zmianie?
            public bool czyTriazNoc;        //Czy osobie można przydzielać triaż na nocnej zmianie?

            //Konstruktor
            public Osoba(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                this.numer = numer;
                this.imie = imie;
                this.nazwisko = nazwisko;
                this.wymiarEtatu = wymiarEtatu;
                this.zaleglosci = zaleglosci;
                this.czyTriazDzien = czyTriazDzien;
                this.czyTriazNoc = czyTriazNoc;
            }
        }

        private class Optimization
        {
            //Obiekty tej klasy są wykorzystywane w optymalizacji genetycznej.
            private class Osobnik
            {
                public bool[] genom;            //Rozwiązanie związane z danym osobnikiem.
                public decimal wartosc;         //Wartośćfunkcji celu dla danego rozwiązania.

                //Konstruktor.
                public Osobnik(bool[] genotyp, decimal wartosc)
                {
                    this.genom = genotyp;
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

            //Deklaracja i stworzenie delegata do funkcji celu, wykorzystywanego jako argument do funkcji optymalizacji.
            public delegate decimal FunkcjaCeluUchwyt(bool[] funkcje);
            public static readonly FunkcjaCeluUchwyt handler = new FunkcjaCeluUchwyt(FunkcjaCelu);

            private static int[] dyzuryGrafik;
            private static int[] liczbaDyzurow;
            private static int[] nieTriazDzien;
            private static int[] nieTriazNoc;
            private static double[] oczekiwanaLiczbaFunkcji;
            private static decimal stopienZdegenerowania;

            public static void DodajFunkcje(bool[] optymalneRozwiazanie)
            {
                int nrSala;
                int nrTriaz1;
                int nrTriaz2;
                bool[] numerOsoby = new bool[MAX_LICZBA_BITOW];
                for (int i = 0; i < 2 * LICZBA_DNI; i++)
                {
                    if (liczbaDyzurow[i] > 0)
                    {
                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = optymalneRozwiazanie[3 * MAX_LICZBA_BITOW * i + j];

                        nrSala = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = optymalneRozwiazanie[3 * MAX_LICZBA_BITOW * i + MAX_LICZBA_BITOW + j];

                        nrTriaz1 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = optymalneRozwiazanie[3 * MAX_LICZBA_BITOW * i + 2 * MAX_LICZBA_BITOW + j];

                        nrTriaz2 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        if (i < LICZBA_DNI)
                        {
                            listBoxesDzien[i].ToSala(nrSala);
                            listBoxesDzien[i].ToTriaz(nrTriaz1);
                            listBoxesDzien[i].ToTriaz(nrTriaz2);
                        }

                        else
                        {
                            listBoxesNoc[i - LICZBA_DNI].ToSala(nrSala);
                            listBoxesNoc[i - LICZBA_DNI].ToTriaz(nrTriaz1);
                            listBoxesNoc[i - LICZBA_DNI].ToTriaz(nrTriaz2);
                        }
                    }
                }
            }

            private static decimal FunkcjaCelu(bool[] funkcje)
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
                    if (osoby[i] != null)
                        dyzuryRozstaw[i] = new bool[Convert.ToInt32(osoby[i].wymiarEtatu)];

                    else
                        dyzuryRozstaw[i] = new bool[0];

                    nrDyzuru[i] = 0;
                }

                for (int i = 0; i < 2 * LICZBA_DNI; i++)
                {
                    liczbaStazystowNaTriazu[i] = 0;
                }

                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    liczbaSalOsobaDzien[i] = 0;
                    liczbaSalOsobaNoc[i] = 0;
                    liczbaTriazyOsobaDzien[i] = 0;
                    liczbaTriazyOsobaNoc[i] = 0;
                }

                for (int i = 0; i < LICZBA_DNI; i++)
                {
                    // Dzien
                    if (liczbaDyzurow[i] > 0)
                    {
                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * i * MAX_LICZBA_BITOW + j];

                        nrOsobySala = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * i * MAX_LICZBA_BITOW + MAX_LICZBA_BITOW + j];

                        nrOsobyTriaz1 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * i * MAX_LICZBA_BITOW + 2 * MAX_LICZBA_BITOW + j];

                        nrOsobyTriaz2 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobySala] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaSalOsobaDzien[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1][nrDyzuru[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]] = true;
                        }

                        if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaTriazyOsobaDzien[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1][nrDyzuru[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]] = true;
                        }

                        if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaTriazyOsobaDzien[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1][nrDyzuru[dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]] = true;
                        }

                        if (nrOsobySala == nrOsobyTriaz1)
                            a += 1000000.0m;

                        if (nrOsobySala == nrOsobyTriaz2)
                            a += 1000000.0m;

                        if (nrOsobyTriaz1 == nrOsobyTriaz2)
                            a += 1000000.0m;

                        for (int j = 0; j < nieTriazDzien.Length; j++)
                        {
                            if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazDzien[j] || dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazDzien[j])
                                a += 100.0m;
                        }

                        for (int j = 0; j < nieTriazNoc.Length; j++)
                        {
                            if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || dyzuryGrafik[i * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                                liczbaStazystowNaTriazu[i]++;
                        }

                        for (int j = i * MAX_LICZBA_DYZUROW; j < (i + 1) * MAX_LICZBA_DYZUROW; j++)
                        {
                            if (dyzuryGrafik[j] != 0)
                                nrDyzuru[dyzuryGrafik[j] - 1]++;
                        }
                    }

                    //Noc
                    if (liczbaDyzurow[i + LICZBA_DNI] > 0)
                    {
                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + j];

                        nrOsobySala = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + MAX_LICZBA_BITOW + j];

                        nrOsobyTriaz1 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        for (int j = 0; j < MAX_LICZBA_BITOW; j++)
                            numerOsoby[j] = funkcje[3 * (i + LICZBA_DNI) * MAX_LICZBA_BITOW + 2 * MAX_LICZBA_BITOW + j];

                        nrOsobyTriaz2 = numerOsoby.Aggregate(0, (sum, val) => (sum * 2) + (val ? 1 : 0));

                        if (dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaSalOsobaNoc[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1][nrDyzuru[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobySala] - 1]] = true;
                        }

                        if (dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaTriazyOsobaNoc[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1][nrDyzuru[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] - 1]] = true;
                        }

                        if (dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == 0)
                            a += 1000000.0m;
                        else
                        {
                            liczbaTriazyOsobaNoc[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]++;
                            dyzuryRozstaw[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1][nrDyzuru[dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] - 1]] = true;
                        }

                        if (nrOsobySala == nrOsobyTriaz1)
                            a += 1000000.0m;

                        if (nrOsobySala == nrOsobyTriaz2)
                            a += 1000000.0m;

                        if (nrOsobyTriaz1 == nrOsobyTriaz2)
                            a += 1000000.0m;

                        for (int j = 0; j < nieTriazNoc.Length; j++)
                        {
                            if (dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz1] == nieTriazNoc[j] || dyzuryGrafik[(i + LICZBA_DNI) * MAX_LICZBA_DYZUROW + nrOsobyTriaz2] == nieTriazNoc[j])
                            {
                                liczbaStazystowNaTriazu[(i + LICZBA_DNI)]++;
                                a += 100.0m;
                            }
                        }

                        for (int j = (i + LICZBA_DNI) * MAX_LICZBA_DYZUROW; j < (i + LICZBA_DNI + 1) * MAX_LICZBA_DYZUROW; j++)
                        {
                            if (dyzuryGrafik[j] != 0)
                                nrDyzuru[dyzuryGrafik[j] - 1]++;
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

            private static int[] LiczbaDyzurow(int[] dyzuryGrafik)
            {
                int[] liczbaDyzurow = new int[2 * LICZBA_DNI];
                for (int i = 0; i < LICZBA_DNI; i++)
                {
                    liczbaDyzurow[i] = 0;
                    for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                    {
                        if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + j] != 0)
                            liczbaDyzurow[i]++;

                        if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + j + LICZBA_DNI * MAX_LICZBA_DYZUROW] != 0)
                            liczbaDyzurow[i + LICZBA_DNI]++;
                    }
                }

                return liczbaDyzurow;
            }

            private static int[] ListaNieTiazDzien()
            {
                int nieTriazDzienNumber = 0;
                foreach (Osoba iter in osoby)
                {
                    if (iter != null)
                    {
                        if (!iter.czyTriazDzien)
                            nieTriazDzienNumber++;
                    }
                }

                int[] nieTriazDzien = new int[nieTriazDzienNumber];
                int iter1 = 0;
                foreach (Osoba iter in osoby)
                {
                    if (iter != null)
                    {
                        if (!iter.czyTriazDzien)
                        {
                            nieTriazDzien[iter1] = iter.numer;
                            iter1++;
                        }
                    }
                }

                return nieTriazDzien;
            }

            private static int[] ListaNieTiazNoc()
            {
                int nieStazNocNumber = 0;
                foreach (Osoba iter in osoby)
                {
                    if (iter != null)
                    {
                        if (!iter.czyTriazNoc)
                            nieStazNocNumber++;
                    }
                }

                int[] nieStazNoc = new int[nieStazNocNumber];
                int iter1 = 0;
                foreach (Osoba iter in osoby)
                {
                    if (iter != null)
                    {
                        if (!iter.czyTriazNoc)
                        {
                            nieStazNoc[iter1] = iter.numer;
                            iter1++;
                        }
                    }
                }

                return nieStazNoc;
            }

            private static double[] OczekiwanaLiczbaFunkcji(Osoba[] osoby)
            {
                const double MAX_LICZBA_FUNKCJI = 8.5;
                const double MIN_LICZBA_FUNKCJI = 2.5;
                double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB];
                int liczbaDniRoboczych = 0;
                double sumaEtatow = 0.0;

                for (int i = 0; i < LICZBA_DNI; i++)
                {
                    if (listBoxesDzien[i].Items.Count > 0)
                        liczbaDniRoboczych++;

                    if (listBoxesNoc[i].Items.Count > 0)
                        liczbaDniRoboczych++;
                }

                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    if (osoby[i] != null)
                        sumaEtatow += Convert.ToInt32(osoby[i].wymiarEtatu);
                }

                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    if (osoby[i] != null)
                    {
                        oczekiwanaLiczbaFunkcji[i] = Math.Min(Math.Max(((3 * liczbaDniRoboczych * osoby[i].wymiarEtatu / sumaEtatow) - osoby[i].zaleglosci), 0), MAX_LICZBA_FUNKCJI);
                        if (osoby[i].wymiarEtatu > 0.1)
                        {
                            oczekiwanaLiczbaFunkcji[i] = Math.Max(oczekiwanaLiczbaFunkcji[i], MIN_LICZBA_FUNKCJI);
                        }
                    }
                }
                return oczekiwanaLiczbaFunkcji;
            }

            public static bool[] OptymalizacjaGA(int liczbaZmiennych, int liczbaOsobnikow, decimal tol, decimal tolX, int maxKonsekwentnychIteracji, int maxIteracji)
            {
                //Jeśli wybrano liczbę osobników mniejszą niż 10 to przypisz 10.
                if (liczbaOsobnikow < 10)
                    liczbaOsobnikow = 10;

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
                double sumaSzans = 0.0;                                                     //1 + 2 + 3 + ... + liczbaReprodukujących.
                double[] szansa = new double[Convert.ToInt32(liczbaReprodukujacych)];       //Szansa danego osobnika na reprodukcję.

                for (int i = 0; i < liczbaReprodukujacych; i++)
                    sumaSzans += (i + 1);

                szansa[0] = liczbaReprodukujacych / sumaSzans;
                for (int i = 1; i < liczbaReprodukujacych; i++)
                    szansa[i] = szansa[i - 1] + (liczbaReprodukujacych - i) / sumaSzans;

                for (int j = 0; j < liczbaOsobnikow; j++)
                {
                    bool[] bools = new bool[liczbaZmiennych];
                    bool[] bools2 = new bool[liczbaZmiennych];
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
                    osobniki[j] = new Osobnik(bools, 0.0m);
                    osobnikiTemp[j] = new Osobnik(bools2, 0.0m);
                }

                for (int i = 0; i < liczbaOsobnikow; i++)
                {
                    osobniki[i].wartosc = FunkcjaCelu(osobniki[i].genom);
                    liczbaWywolanFunkcjiCelu++;
                }

                Array.Sort(osobniki, OsComp);
                cel = osobniki[0].wartosc;

                while (nrIteracji <= maxIteracji && nrKonsekwentnejIteracji <= maxKonsekwentnychIteracji && (cel > stopienZdegenerowania + tol))
                {
                    nrIteracji++;
                    nrKonsekwentnejIteracji++;
                    for (int i = 0; i < liczbaOsobnikow; i++)
                        osobnikiTemp[i].genom = osobniki[i].genom;

                    //
                    // Krzyzowanie
                    //

                    for (int i = liczbaOsobnikow - 1; i >= liczbaElitarnych; i--)
                    {
                        temp = rnd.NextDouble();
                        for (int j = (liczbaReprodukujacych - 1); j > 0; j--)
                        {
                            if (szansa[j] <= temp)
                            {
                                nrPrzodka1 = j;
                                break;
                            }
                        }

                        temp = rnd.NextDouble();
                        for (int j = (liczbaReprodukujacych - 1); j > 0; j--)
                        {
                            if (szansa[j] <= temp)
                            {
                                nrPrzodka2 = j;
                                break;
                            }
                        }

                        czyKrzyzowanie = rnd.NextDouble();

                        if (czyKrzyzowanie < SZANSA_KRZYZOWANIE)
                        {

                            for (int k = 0; k < liczbaZmiennych / MAX_LICZBA_BITOW; k++)
                            {
                                temp = rnd.NextDouble();
                                if (temp < 0.5)
                                {
                                    for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                    {
                                        osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka1].genom[k * MAX_LICZBA_BITOW + m];
                                    }
                                }

                                else
                                {
                                    for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                    {
                                        osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka2].genom[k * MAX_LICZBA_BITOW + m];
                                    }
                                }
                            }
                        }

                        else if (czyKrzyzowanie >= SZANSA_KRZYZOWANIE)
                        {
                            temp = rnd.NextDouble();
                            for (int k = 0; k < liczbaZmiennych / MAX_LICZBA_BITOW; k++)
                            {

                                if (temp < 0.5)
                                {
                                    for (int m = 0; m < MAX_LICZBA_BITOW; m++)
                                        osobniki[i].genom[k * MAX_LICZBA_BITOW + m] = osobnikiTemp[nrPrzodka1].genom[k * MAX_LICZBA_BITOW + m];
                                }

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
                        //Odświeżanie gdy invoke required == true.
                        if (labelRaport.InvokeRequired)
                        {
                            labelRaport.Invoke(new Action(() =>
                            {
                                labelRaport.Text = "Wartość f. celu: " + cel.ToString() + " Nr. Iteracji: " + nrIteracji.ToString() + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString() + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString() + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                                labelRaport.Refresh();
                            }));
                        }

                        //Odświeżanie, gdy invoke required == false.
                        else
                        {
                            labelRaport.Text = "Wartość f. celu: " + cel.ToString() + " Nr. Iteracji: " + nrIteracji.ToString() + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString() + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString() + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                            labelRaport.Refresh();
                        }
                    }
                }

                //Odświeżanie etykiety labelRaport po zakończeniu optymalizacji.
                //Odświeżanie gdy invoke required == true.
                if (labelRaport.InvokeRequired)
                {
                    labelRaport.Invoke(new Action(() =>
                    {
                        labelRaport.Text = "Wartość f. celu: " + cel.ToString() + " Nr. Iteracji: " + nrIteracji.ToString() + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString() + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString() + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                        labelRaport.Refresh();
                    }));
                }

                //Odświeżanie, gdy invoke required == false.
                else
                {
                    labelRaport.Text = "Wartość f. celu: " + cel.ToString() + " Nr. Iteracji: " + nrIteracji.ToString() + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString() + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString() + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                    labelRaport.Refresh();
                }

                //Wyświetla infomrację, jeśli nie udało się osiągnąć celu optymalizacji.
                if (cel > stopienZdegenerowania + tol)
                {
                    //Odświeżanie gdy invoke required == true.
                    if (labelRaport.InvokeRequired)
                    {
                        labelRaport.Invoke(new Action(() =>
                        {
                            labelRaport.Text += ". Cel nie został osiągnięty. Rozważ ponowne rozdzielenie funkcji.";
                            labelRaport.Refresh();
                        }));
                    }

                    //Odświeżanie, gdy invoke required == false.
                    else
                    {
                        labelRaport.Text += ". Cel nie został osiągnięty. Rozważ ponowne rozdzielenie funkcji.";
                        labelRaport.Refresh();
                    }
                }

                //WYświetla informację, gdy cel został osiągnięty.
                else
                {
                    //Odświeżanie gdy invoke required == true.
                    if (labelRaport.InvokeRequired)
                    {
                        labelRaport.Invoke(new Action(() =>
                        {
                            labelRaport.Text += " Ukończono. ";
                            labelRaport.Refresh();
                        }));
                    }

                    //Odświeżanie, gdy invoke required == false.
                    else
                    {
                        labelRaport.Text += " Ukończono. ";
                        labelRaport.Refresh();
                    }
                }

                //Funkcja zwraca genom najlepszego osobnika.
                return osobniki[0].genom;
            }

            public static void Prepare()
            {
                dyzuryGrafik = UtworzGrafik();
                nieTriazDzien = ListaNieTiazDzien();
                nieTriazNoc = ListaNieTiazNoc();
                liczbaDyzurow = LiczbaDyzurow(dyzuryGrafik);
                oczekiwanaLiczbaFunkcji = OczekiwanaLiczbaFunkcji(osoby);
                stopienZdegenerowania = StopienZdegenerowania(liczbaDyzurow, nieTriazDzien, nieTriazNoc, dyzuryGrafik);
            }

            private static decimal StopienZdegenerowania(int[] liczbaDyzurow, int[] nieTriazDzien, int[] nieTriazNoc, int[] dyzuryGrafik)
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
                            if (nieTriazDzien.Contains(dyzuryGrafik[j + i * MAX_LICZBA_DYZUROW]))
                                liczbaStazystowDzien++;

                            if (nieTriazNoc.Contains(dyzuryGrafik[j + i * MAX_LICZBA_DYZUROW]))
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

            private static int[] UtworzGrafik()
            {
                int nrListBoxa;
                int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];
                for (int i = 0; i < LICZBA_DNI * MAX_LICZBA_DYZUROW; i++)
                {
                    nrListBoxa = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                    if (listBoxesDzien[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                    {
                        listBoxesDzien[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);
                        dyzuryGrafik[i] = Convert.ToInt32(listBoxesDzien[nrListBoxa].GetNumber(i % MAX_LICZBA_DYZUROW));
                    }

                    else
                        dyzuryGrafik[i] = 0;

                    nrListBoxa = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                    if (listBoxesNoc[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                    {
                        listBoxesNoc[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);
                        dyzuryGrafik[i + LICZBA_DNI * MAX_LICZBA_DYZUROW] = Convert.ToInt32(listBoxesNoc[nrListBoxa].GetNumber(i % MAX_LICZBA_DYZUROW));
                    }

                    else
                        dyzuryGrafik[i + LICZBA_DNI * MAX_LICZBA_DYZUROW] = 0;
                }

                return dyzuryGrafik;
            }
        }

        //Wyjątek zwracany przy próbie dodania pracownika jeśli w systemie nie ma miejsca.
        public class TooManyEmployeesException : Exception
        {
            public TooManyEmployeesException() { }

            public TooManyEmployeesException(string message) : base(message) { }

            public TooManyEmployeesException(string message, Exception inner) : base(message, inner) { }
        }

        //Ten słownik zawiera informacje o formacie danych w pliku "Pracownicy.txt".
        private static readonly Dictionary<string, int> pracownicy_txt = new Dictionary<string, int>()
        {
            {"NUMER", 0 }, {"IMIE", 1}, {"NAZWISKO", 2}, {"ZALEGLOSCI", 3}, {"TRIAZ_DZIEN", 4}, {"TRIAZ_NOC", 5}
        };

        public static Osoba[] osoby = new Osoba[MAX_LICZBA_OSOB];           //Stworzenie listy pracowników.
        public static System.Windows.Forms.Label[] labelsPracownicy = new System.Windows.Forms.Label[MAX_LICZBA_OSOB];      //Tworzenie etykiet wyświetlających dane pracowników.
        private System.Windows.Forms.Label[] labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];                //Tworzenie etykiet wyświetlających numer dziennej zmiany.
        private System.Windows.Forms.Label[] labelsNoc = new System.Windows.Forms.Label[LICZBA_DNI];                  //Tworzenie etykiet wyświetlających numer nocnej zmiany.
        private static ListBoxGrafik[] listBoxesDzien = new ListBoxGrafik[LICZBA_DNI];                                       //Tworzenie listboxów odpowiadających dziennej zmianie.
        private static ListBoxGrafik[] listBoxesNoc = new ListBoxGrafik[LICZBA_DNI];                                         //Tworzenie listboxów odpowiadających nocnej zmianie.

        private const int MAX_LICZBA_BITOW = 3;                             //Liczba bitów potrzebna do zakodowania jednej osoby (log2(MAX_LICZBA_DYZUROW)).
        private const int MAX_LICZBA_DYZUROW = 8;                           //Maksymalna liczba dyżurów jednego dnia.
        public const int MAX_LICZBA_OSOB = 50;                              //Maksymalna liczba pracowników w systemie.
        private const int LICZBA_DNI = 31;                                   //Największa liczba dni w miesiącu.
        private const int LICZBA_ZMIENNYCH = 2 * LICZBA_DNI * 3 * MAX_LICZBA_BITOW;     //Liczba zmiennych w zadaniu optymalizacji.
        private static int liczbaOsob = 0;                                   //Aktualna liczba pracowników w systemie.       
        private DateTime startOptymalizacja;                                //Pomiar czasu działania algorytmu optymalizacji.
        private TimeSpan czasOptymalizacja;                                 //Pomiar czasu działania algorytmu optymalizacji.
        
        //Konstruktor.
        public Form1()
        {
            //Generuje większaość kontrolek. Metoda stworzona przez Designera.
            InitializeComponent();

            //Generuje listboxy i etykiety grafiku i listy pracowników. Zdarzenie asynchroniczne przycisku optymalizacji.
            InitializeComponent2();

            //Wczytujemy pracowników z pliku tekstowego przy starcie programu.
            FileManagementPracownicy.WczytajPracownikow("Pracownicy.txt");

            //Jeśli plik z grafikiem istnieje, to wyświetlane jest zapytanie, czy go wczytać.
            if (File.Exists("Grafik.txt"))
            {
                var result = MessageBox.Show("Wczytać ostatni grafik?", "Wczytywanie grafiku", MessageBoxButtons.YesNo);

                //Jeśli wybrano opcje "Tak" to wczytywany jest grafik.
                if (result == DialogResult.Yes)
                    FileManagementGrafik.WczytajGrafik("Grafik.txt");
            }
        }

        //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne.
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                int idx;    //Numer wybranego indeksu.

                //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne (dzień).
                try
                {
                    idx = listBoxesDzien[nrDnia].Items.IndexOf(listBoxesDzien[nrDnia].SelectedItem);
                    listBoxesDzien[nrDnia].ToBezFunkcji(idx);
                }
                catch { }

                //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne (noc).
                try
                {
                    idx = listBoxesNoc[nrDnia].Items.IndexOf(listBoxesNoc[nrDnia].SelectedItem);
                    listBoxesNoc[nrDnia].ToBezFunkcji(idx);
                }
                catch { }
            }
        }

        //Usuwamy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana. Usuwamy grafik. Wyświetlamy informację.
            UsunPodswietlenie();
            UsunGrafik();
            MessageBox.Show("Grafik usunięty");
        }

        //Wyświetlamy Form2.
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Wyświetlamy Form2.
            Form2 dialog = new Form2();
            dialog.ShowDialog();
        }

        //Zamieniamy wszystkie wybrane dyżury na sale.
        private void buttonSala_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy wszystkie wybrane dyżury na sale.
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                int idx;    //Numer wybranego indeksu.

                //Zamieniamy wszystkie wybrane dyżury na sale (dzień).
                try
                {
                    idx = listBoxesDzien[nrDnia].Items.IndexOf(listBoxesDzien[nrDnia].SelectedItem);
                    listBoxesDzien[nrDnia].ToSala(idx);
                }
                catch { }

                //Zamieniamy wszystkie wybrane dyżury na sale (noc).
                try
                {
                    idx = listBoxesNoc[nrDnia].Items.IndexOf(listBoxesNoc[nrDnia].SelectedItem);
                    listBoxesNoc[nrDnia].ToSala(idx);
                }
                catch { }
            }
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy wszystkie wybrane dyzury na triaż.
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                int idx; //Numer wybranego indeksu.

                //Zamieniamy wszystkie wybrane dyżury na triaż (dzień).
                try
                {
                    idx = listBoxesDzien[nrDnia].Items.IndexOf(listBoxesDzien[nrDnia].SelectedItem);
                    listBoxesDzien[nrDnia].ToTriaz(idx);
                }
                catch { }

                //Zamieniamy wszystkie wybrane dyżury na triaż (noc).
                try
                {
                    idx = listBoxesNoc[nrDnia].Items.IndexOf(listBoxesNoc[nrDnia].SelectedItem);
                    listBoxesNoc[nrDnia].ToTriaz(idx);
                }
                catch { }
            }
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Jeśli był wybrany jakiś dyżur, to uaktulaniamy wymair etatu danego pracownika.
            foreach (ListBoxGrafik listBoxDzien in listBoxesDzien)
            {
                if (listBoxDzien.SelectedItem != null)
                    EmployeeManagement.EmployeeEdit(osoby[listBoxDzien.GetNumber(listBoxDzien.SelectedIndex) - 1], (osoby[listBoxDzien.GetNumber(listBoxDzien.SelectedIndex) - 1].wymiarEtatu - 1.0));
            }

            foreach (ListBoxGrafik listBoxNoc in listBoxesNoc)
            {
                if (listBoxNoc.SelectedItem != null)
                    EmployeeManagement.EmployeeEdit(osoby[listBoxNoc.GetNumber(listBoxNoc.SelectedIndex) - 1], (osoby[listBoxNoc.GetNumber(listBoxNoc.SelectedIndex) - 1].wymiarEtatu - 1.0));
            }

            //Usuwamy wybrane dyżury.
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].Items.Remove(listBoxesDzien[j].SelectedItem);
                listBoxesNoc[j].Items.Remove(listBoxesNoc[j].SelectedItem);
            }
        }

        //Wczytujemy grafik z pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Próbujemy wczytać grafik
            try
            {
                FileManagementGrafik.WczytajGrafik("Grafik.txt");
                MessageBox.Show("Grafik wczytany.");
            }

            catch
            {
                MessageBox.Show("Grafik nie został wczytany.");
            }
        }

        //Zapisujemy grafik do pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Próbujemy zapisać grafik
            try
            {
                FileManagementGrafik.ZapiszGrafik("Grafik.txt");
                MessageBox.Show("Grafik zapisany.");
            }
            catch { MessageBox.Show("Grafik nie został zapisany."); }
        }

        //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana i usuwamy zaznaczenie.
        private void formClick(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Usuwamy zaznaczenie.
            for (int nrListBoxa = 0; nrListBoxa < LICZBA_DNI; nrListBoxa++)
            {
                listBoxesDzien[nrListBoxa].ClearSelected();
                listBoxesNoc[nrListBoxa].ClearSelected();
            }
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Generuje listboxy i etykiety grafiku i listy pracowników. Zdarzenie asynchroniczne przycisku optymalizacji.
        private void InitializeComponent2()
        {
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                //Tworzymy listBoxy dla dyżurów dziennych.
                listBoxesDzien[nrDnia] = new ListBoxGrafik();
                listBoxesDzien[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                listBoxesDzien[nrDnia].Size = Size = new System.Drawing.Size(40, 400);
                listBoxesDzien[nrDnia].AllowDrop = true;
                tableLayoutPanel2.Controls.Add(listBoxesDzien[nrDnia], nrDnia, 1);

                //Tworzymy etykiety dla dyżurów dziennych.
                labelsDzien[nrDnia] = new System.Windows.Forms.Label();
                labelsDzien[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsDzien[nrDnia].Size = Size = new System.Drawing.Size(340, 40);
                labelsDzien[nrDnia].Text = (nrDnia + 1).ToString();
                tableLayoutPanel2.Controls.Add(labelsDzien[nrDnia], nrDnia, 0);

                //Tworzymy listBoxy dla dyżurów nocnych.
                listBoxesNoc[nrDnia] = new ListBoxGrafik();
                listBoxesNoc[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                listBoxesNoc[nrDnia].Size = Size = new System.Drawing.Size(40, 400);
                listBoxesNoc[nrDnia].AllowDrop = true;
                tableLayoutPanel3.Controls.Add(listBoxesNoc[nrDnia], nrDnia, 1);

                //Tworzymy etykiety dla dyżurów nocnych.
                labelsNoc[nrDnia] = new System.Windows.Forms.Label();
                labelsNoc[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsNoc[nrDnia].Size = Size = new System.Drawing.Size(340, 40);
                labelsNoc[nrDnia].Text = (nrDnia + 1).ToString();
                tableLayoutPanel3.Controls.Add(labelsNoc[nrDnia], nrDnia, 0);

                //Przypisujemy delegaty do zdarzeń drag and drop.
                int iter3 = nrDnia;
                listBoxesDzien[nrDnia].DragEnter += new DragEventHandler(this.listBoxDzien_DragEnter);
                listBoxesDzien[nrDnia].DragDrop += new DragEventHandler((sender, e) => listBoxDzien_DragDrop(sender, e, iter3));
                listBoxesNoc[nrDnia].DragEnter += new DragEventHandler(this.listBoxNoc_DragEnter);
                listBoxesNoc[nrDnia].DragDrop += new DragEventHandler((sender, e) => listBoxNoc_DragDrop(sender, e, iter3));
            }

            //Tworzymy etykiety wyświetlające dane pracowników i delegaty do zdarzeń drag and drop.
            for (int nrOsoby = 0; nrOsoby < MAX_LICZBA_OSOB; nrOsoby++)
            {
                //Tworzymy etykiety wyświetlające dane pracowników.
                labelsPracownicy[nrOsoby] = new System.Windows.Forms.Label();
                labelsPracownicy[nrOsoby].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsPracownicy[nrOsoby].Size = Size = new System.Drawing.Size(340, 40);
                labelsPracownicy[nrOsoby].Text = "";
                tableLayoutPanel1.Controls.Add(labelsPracownicy[nrOsoby], nrOsoby / 10, nrOsoby % 10);

                //Przypisujemy delegaty do zdarzeń drag and drop.
                int temp = nrOsoby;
                labelsPracownicy[nrOsoby].MouseDown += new MouseEventHandler((sender, e) => labelsPracownicy_MouseDown(sender, e, temp));
            }

            //Zdarzenie asynchroniczne po kliknięciu przycisku "Opt".
            buttonOptymalizacja.Click += async (sender, e) =>
            {
                //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
                UsunPodswietlenie();

                //Próbujemy przeprowadzić optymalizację.
                try
                {
                    bool[] optymalneRozwiazanie;           //Rozwiąznie (genom) uzyskany w wyniku optymalizacji.
                    decimal tol = 0.0000003m;              //Wartość f. celu jaką należy osiągnąć, aby zakończyć optymalizację.
                    decimal tolX = 0.00000000001m;         //Minimalna zmiana f. celu skutkująca zresetowaniem licznika konsekwentnych iteracji.
                    int maxIterations = 200000;            //Maksymalna liczba iteracji. Po jej osiągnięciu algorytm kończy działać, nawet jeśli nie osiągnął wartości tol.
                    int maxConsIterations = 40000;         //Maksymalna liczba iteracji od ostatniej poprawy f. celu przynajmniej o tolX. Po jej osiągnięciu algorytm kończy działać, nawet jeśli nie osiągnął wartości tol.
                    int liczbaOsobnikow = 100;             //Liczebność populacji.

                    //Dezaktywujemy wszystkie kontrolki z wyjątkiem etykiety labelRaport.
                    foreach (Control control in this.Controls)
                    {
                        if (control != labelRaport)
                            control.Enabled = false;
                    }

                    Optimization.Prepare();

                    //Jeżeli w dowolnym dniu liczba dyżurów jest większa niz MAX_LICZBA_DYZUROW to optymalizacja nie startuje.
                    for (int i = 0; i < LICZBA_DNI; i++)
                    {
                        if (listBoxesDzien[i].Items.Count > MAX_LICZBA_DYZUROW || listBoxesNoc[i].Items.Count > MAX_LICZBA_DYZUROW)
                        {
                            MessageBox.Show("Aby móc wykorzystać automatyczne rozdzielanie funkcji liczba dyżurów danego dnia nie może przekraczać " + MAX_LICZBA_DYZUROW.ToString() + ".");
                            return;
                        }
                    }

                    //Jeżeli w dowolnym dniu przypisany jest dokładnie jeden lub dokładnie dwa dyżury to optymalizacja nie startuje.
                    for (int i = 0; i < LICZBA_DNI; i++)
                    {
                        if (listBoxesDzien[i].Items.Count == 1 || listBoxesDzien[i].Items.Count == 2 || listBoxesNoc[i].Items.Count == 1 || listBoxesNoc[i].Items.Count == 2)
                        {
                            MessageBox.Show("Aby móc wykorzystać automatyczne rozdzielanie funkcji liczba dyżurów na każdej zmianie musi wynosić conajmniej 3.");
                            return;
                        }
                    }

                    //Jeśli wszystko jest w porządku uruchamia się optymalizacja i mierzony jest czas.
                    startOptymalizacja = DateTime.Now;
                    {
                        optymalneRozwiazanie = await Task.Run(() => Optimization.OptymalizacjaGA(LICZBA_ZMIENNYCH, liczbaOsobnikow, tol, tolX, maxConsIterations, maxIterations));
                    }
                    czasOptymalizacja = DateTime.Now - startOptymalizacja;

                    //Wyświetl grafik, zapisz grafik i wyświetl czas.
                    Optimization.DodajFunkcje(optymalneRozwiazanie);
                    FileManagementGrafik.ZapiszGrafik("GrafikGA.txt");
                    MessageBox.Show("Przydzielanie funkcji ukończone w: " + czasOptymalizacja.ToString() + ".");
                }

                //Wyświetla informację, jeśli nie udało się przeprowadzić optymalizacji.
                catch
                {
                    MessageBox.Show("Przydzielanie funkcji nie powiodło się.");
                }

                //Odblokowuje UI po zakończeniu optymalizacji.
                finally
                {
                    foreach (Control control in this.Controls)
                    {
                        if (control != labelRaport)
                            control.Enabled = true;
                    }
                }
            };
        }

        //Drag and drop, listBoxDzien. Efekt wizualny i kopiowanie tekstu.
        private void listBoxDzien_DragEnter(object sender, DragEventArgs e)
        {
            //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        //Drag and drop, listBoxDzien. Dodajemy dyżur.
        private void listBoxDzien_DragDrop(object sender, DragEventArgs e, int nrListBoxa)
        {
            //Pobieramy dane, dzielimy i uzyskujemy numer osoby.
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            string[] subs = pom.Split('.');
            int nrOsoby = (Convert.ToInt32(subs[0]) - 1);

            //Jeśli numer osoby występuje w danym listBoxie, to nic się nie dzieje.
            for (int item = 0; item < listBoxesDzien[nrListBoxa].Items.Count; item++)
            {
                if (listBoxesDzien[nrListBoxa].GetNumber(item).ToString() == subs[0])
                    return;
            }

            //Jeśli dana osoba nie miała dyżuru, to dodajemy do listBoxa.
            listBoxesDzien[nrListBoxa].Items.Add(subs[0]);
            EmployeeManagement.EmployeeEdit(osoby[nrOsoby], osoby[nrOsoby].wymiarEtatu + 1.0);
        }

        //Drag and drop, listBoxNoc. Efekt wizualny i kopiowanie tekstu.
        private void listBoxNoc_DragEnter(object sender, DragEventArgs e)
        {
            //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        //Drag and drop, listBoxNoc. Dodajemy dyżur.
        private void listBoxNoc_DragDrop(object sender, DragEventArgs e, int nrListBoxa)
        {
            //Pobieramy dane, dzielimy i uzyskujemy numer osoby.
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            string[] subs = pom.Split('.');
            int nrOsoby = (Convert.ToInt32(subs[0]) - 1);

            //Jeśli numer osoby występuje w danym listBoxie, to nic się nie dzieje.
            for (int item = 0; item < listBoxesNoc[nrListBoxa].Items.Count; item++)
            {
                if (listBoxesNoc[nrListBoxa].GetNumber(item).ToString() == subs[0])
                    return;
            }

            //Jeśli dana osoba nie miała dyżuru, to dodajemy do listBoxa.
            listBoxesNoc[nrListBoxa].Items.Add(subs[0]);
            EmployeeManagement.EmployeeEdit(osoby[nrOsoby], osoby[nrOsoby].wymiarEtatu + 1.0);
        }

        //Drag and drop, etykieta Pracownicy.
        private void labelsPracownicy_MouseDown(object sender, MouseEventArgs e, int nrOsoby)
        {
            //Usuwamy podświetlenie, jeśli ktoś był zaznaczony.
            UsunPodswietlenie();

            //Sprawdzamy po kolei każdy dyżur, jeśli osoba występuje to podśiwtlamy: czerwony - bez funkcji, zielony - sala, niebieski - triaż.
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                if (osoby[nrOsoby] != null)
                {
                    //Sprawdzamy dyżury nocne.
                    for (int nrDyzuru = 0; nrDyzuru < listBoxesDzien[nrDnia].Items.Count; nrDyzuru++)
                    {
                        if (listBoxesDzien[nrDnia].GetNumber(nrDyzuru) == osoby[nrOsoby].numer)
                        {
                            switch (listBoxesDzien[nrDnia].GetFunction(nrDyzuru))
                            {
                                case 0:
                                    listBoxesDzien[nrDnia].BackColor = Color.Red;
                                    break;

                                case 1:
                                    listBoxesDzien[nrDnia].BackColor = Color.Green;
                                    break;

                                case 2:
                                    listBoxesDzien[nrDnia].BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }

                    //Sprawdzamy dyżury nocne.
                    for (int nrDyzuru = 0; nrDyzuru < listBoxesNoc[nrDnia].Items.Count; nrDyzuru++)
                    {
                        if (listBoxesNoc[nrDnia].GetNumber(nrDyzuru) == osoby[nrOsoby].numer)
                        {
                            switch (listBoxesNoc[nrDnia].GetFunction(nrDyzuru))
                            {
                                case 0:
                                    listBoxesNoc[nrDnia].BackColor = Color.Red;
                                    break;

                                case 1:
                                    listBoxesNoc[nrDnia].BackColor = Color.Green;
                                    break;

                                case 2:
                                    listBoxesNoc[nrDnia].BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }
                }
            }

            if (osoby[nrOsoby] != null)
                labelsPracownicy[nrOsoby].DoDragDrop(labelsPracownicy[nrOsoby].Text, DragDropEffects.Copy | DragDropEffects.Move);
        }

        //Usuń grafik.
        private static void UsunGrafik()
        {
            //Usuwamy dane z listBoxów
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                listBoxesDzien[nrDnia].Items.Clear();
                listBoxesNoc[nrDnia].Items.Clear();
            }

            //Usuwamy dane o wymiarze etatu.
            EmployeeManagement.EmployeeEdit(osoby, 0.0);
        }

        //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
        private void UsunPodswietlenie()
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            for (int nrListBoxa = 0; nrListBoxa < LICZBA_DNI; nrListBoxa++)
            {
                listBoxesDzien[nrListBoxa].ResetBackColor();
                listBoxesNoc[nrListBoxa].ResetBackColor();
            }
        }



















    }
}
