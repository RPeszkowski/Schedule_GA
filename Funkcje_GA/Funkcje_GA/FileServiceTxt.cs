using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za operacje na plikach.
    public class FileServiceTxt
    {
        //Klasa odpowiada za zapis i odczyt danych grafiku do pliku "Grafik.txt".
        public class FileManagementGrafik : IScheduleFileService
        {
            private readonly IEmployeeManagement _employeeManager;               //Instancja do zarządzania pracownikami.
            private readonly IScheduleManagement _scheduleManager;         //Instancja do zarządzania grafikiem.

            //Konstruktor
            public FileManagementGrafik(IEmployeeManagement empManager, IScheduleManagement scheduleManager)
            {
                //Przypisanie instancji.
                this._employeeManager = empManager;
                this._scheduleManager = scheduleManager;
            }

            //Zapisuje dane o osobie do grafiku.
            private void ApplyShift(int nrZmiany, IEnumerable<(int nrOsoby, char? suffix)> entries)
            {
                foreach (var (nrOsoby, suffix) in entries)
                {
                    //Sprawdzamy, czy osoba istnieje w bazie.
                    Employee employee = _employeeManager.GetEmployeeById(nrOsoby) ?? throw new InvalidDataException($"Osoba o numerze {nrOsoby} nie istnieje w bazie.");

                    //Dodajemy do zmiany i przypisujemy funkcję.
                    _scheduleManager.AddToShift(nrZmiany, nrOsoby);

                    if (suffix == 's')
                        _scheduleManager.ToSala(nrZmiany, nrOsoby);

                    if (suffix == 't')
                        _scheduleManager.ToTriaz(nrZmiany, nrOsoby);
                }
            }

            //Formatuje linię tekstu do zapisu do pliku.
            private string FormatShiftLine(Shift shift)
            {
                //Jeśli zmiana nie obsadzona zwracamy pusty string.
                if (shift.PresentEmployees.Count == 0)
                    return string.Empty;

                List <string> tokens = new List<string>();                        //Lista przechowuje numery pracowników na zmianie wraz z funkcjami.

                //Dla każdego pracownika sprawdzamy funkcję i dopisujemy do listy.
                foreach (Employee employee in shift.PresentEmployees)
                {
                    string entry = employee.Numer.ToString();

                    if (shift.SalaEmployees.Contains(employee))
                        entry += "s";
                    else if (shift.TriazEmployees.Contains(employee))
                        entry += "t";

                    tokens.Add(entry);
                }

                //Zwracamy linijkę tekstu.
                return string.Join(" ", tokens);
            }

            //Przygotowuje dane pracownika do wppisania do grafiku.
            private IEnumerable<(int nrOsoby, char? suffix)> ParseShiftLine(string linia)
            {
                //Przygotowujemy po kolei każdą osobę.
                foreach (var token in linia.Split(' '))
                {
                    char? suffix = null;                    //Sufiks określa rodzaj funkcji.
                    string nrOsobyStr = token;              //Nr osoby.

                    //Sprawdzamy, czy osoba pełni funkcję.
                    if (token.EndsWith("s") || token.EndsWith("t"))
                    {
                        suffix = token[token.Length - 1];
                        nrOsobyStr = token.Remove(token.Length - 1);
                    }

                    //Jeżeli dane są niepoprawne, rzucamy wyjątek.
                    if (!int.TryParse(nrOsobyStr, out int nrOsoby) || nrOsoby < 1 || nrOsoby > MAX_LICZBA_OSOB)
                        throw new InvalidDataException($"Niepoprawny numer osoby: {nrOsobyStr}");

                    //Przechodzimy do następnej osoby.
                    yield return (nrOsoby, suffix);
                }
            }

            //Metoda służy do wczytywania grafiku z pliku "Grafik.txt".
            public void WczytajGrafik(string plik)
            {
                string linijka;                         //Wczytana linijka tekstu.
                string[] linijkaPodzielona;           //Numery osób z funkcjami, rozdzielone.

                //Usuwamy grafik.
                _scheduleManager.RemoveAll();

                //Próbujemy wczytać grafik.
                try
                {
                    string[] wszystkieLinie = File.ReadAllLines(plik);      //Wczytany plik

                    //Wczytujemy po kolei każdą linijkę.
                    for (int nrLinii = 0; nrLinii < 2 * LICZBA_DNI; nrLinii++)
                    {
                        //Wczytujemy linijke, dzielimy dane w miejscach, gdzie jest spacja.
                        linijka = wszystkieLinie[nrLinii];
                        linijkaPodzielona = linijka.Split(' ');

                        //Dla każdego dyzuru w linijce dodajemy zmianę do grafiku.
                        for (int nrDyzuru = 0; nrDyzuru < linijkaPodzielona.Length; nrDyzuru++)
                        {
                            //Pomijamy puste linijki.
                            if(string.IsNullOrWhiteSpace(linijka)) continue;

                            //Entries zawierają informacje o osobie i pełnionej funkcji (sufiks).
                            var entries = ParseShiftLine(linijka);

                            //Próbujemy dodać osobę do grafiku.
                            try
                            {
                                ApplyShift(nrLinii, entries);
                            }

                            //Jeśli się nie uda rzucamy wyjątek.
                            catch(Exception ex)
                            {
                                //Czyścimy grafik.
                                foreach (Employee employee in _scheduleManager.GetShiftById(nrLinii).PresentEmployees)
                                    _scheduleManager.RemoveFromShift(nrLinii, employee.Numer);

                                //Wiadomość o błędzie.
                                string info = nrLinii < LICZBA_DNI  
                                            ? $"dnia {nrLinii + 1}, dyżur dzienny" 
                                            : $"dnia {nrLinii + 1 - LICZBA_DNI}, dyżur nocny";

                                //Rzucamy wyjątek.
                                throw new FileServiceInvalidScheduleFormat( $"Nie udało się wczytać grafiku dla {info}: {ex.Message}", ex);
                            }
                        }
                    }
                }

                //Jeśli się nie udało, to rzucamy wyjątek i czyścimy grafik.
                catch(Exception ex)
                {
                    //Czyścimy grafik i rzucamy wyjątek.
                    _scheduleManager.RemoveAll();
                    throw new FileServiceInvalidScheduleFormat($"Nie udało się wczytać grafiku. {ex.Message}", ex);
                }
            }

            //Metoda służy do zapisywania grafiku do pliku "Grafik.txt".
            public void ZapiszGrafik(string plik)
            {
                List<string> linie = new List<string>();                //Tu przechowujemy kolejne linie.

                //Każdą linię formatujemy i dodajemy do listy.
                for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                {
                    Shift shift = _scheduleManager.GetShiftById(nrZmiany);
                    string linia = FormatShiftLine(shift);
                    linie.Add(linia);
                }

                //Zapisujemy plik.
                File.WriteAllLines(plik, linie);
            }
        }

        //Klasa odpowiada za zapis i odczyt danych pracowników do pliku "Pracownicy.txt".
        public class FileManagementPracownicy : IEmployeesFileService
        {
            private readonly IEmployeeManagement _employeeManager;               //Instancja do zarządzania pracownikami.

            //Ten słownik zawiera informacje o formacie danych w pliku "Pracownicy.txt".
            private static readonly Dictionary<string, int> pracownicy_txt = new Dictionary<string, int>()
            {
                {"NUMER", 0 }, {"IMIE", 1}, {"NAZWISKO", 2}, {"ZALEGLOSCI", 3}, {"TRIAZ_DZIEN", 4}, {"TRIAZ_NOC", 5}
            };

            //Konstruktor
            public FileManagementPracownicy(IEmployeeManagement empManager)
            {
                //Przypisanie instancji.
                this._employeeManager = empManager;
            }

            //Wczytujemy dane pracowników z pliku "Pracownicy.txt".
            public void WczytajPracownikow(string plik)
            {
                //Próbujemy wczytać plik.
                try
                {
                    string[] wszystkieLinie = File.ReadAllLines(plik);              //Wczytujemy wszystkie linie.

                    for (int nrLinii = 0; nrLinii < MAX_LICZBA_OSOB && nrLinii < wszystkieLinie.Length; nrLinii++)
                    {
                        string wczytanaLinia = wszystkieLinie[nrLinii];             //Wczytana linia.

                        // Jeśli linia pusta to pomijamy.
                        if (string.IsNullOrWhiteSpace(wczytanaLinia))
                            continue;

                        string[] pola = wczytanaLinia.Split(' ');                   //Podziellona linia.

                        //Sprawdzamy, czy dane są pełne.
                        if (pola.Length < 6)
                            throw new FileServiceInvalidEmployeesFormat($"Niepełne dane w linii {nrLinii + 1}.");

                        //Sprawdzamy umer pracownika.
                        if (!int.TryParse(pola[pracownicy_txt["NUMER"]], out int numer))
                            throw new FileServiceInvalidEmployeesFormat($"Niepoprawny numer pracownika w linii {nrLinii + 1}.");

                        //Wczytujemy resztę danych.
                        string imie = pola[pracownicy_txt["IMIE"]];
                        string nazwisko = pola[pracownicy_txt["NAZWISKO"]];

                        //Sprawdzamy resztę danych.
                        if (!int.TryParse(pola[pracownicy_txt["ZALEGLOSCI"]], out int zaleglosci))
                            throw new FileServiceInvalidEmployeesFormat($"Niepoprawne zaległości w linii {nrLinii + 1}.");

                        if (!bool.TryParse(pola[pracownicy_txt["TRIAZ_DZIEN"]], out bool triazDzien))
                            throw new FileServiceInvalidEmployeesFormat($"Niepoprawna flaga triażu dziennego w linii {nrLinii + 1}.");

                        if (!bool.TryParse(pola[pracownicy_txt["TRIAZ_NOC"]], out bool triazNoc))
                            throw new FileServiceInvalidEmployeesFormat($"Niepoprawna flaga triażu nocnego w linii {nrLinii + 1}.");

                        //Próbujemy dodać osobę.
                        try
                        {
                            _employeeManager.EmployeeAdd(numer, imie, nazwisko, 0.0, zaleglosci, triazDzien, triazNoc);
                        }

                        //Jeśli się nie uda rzucamy wyjątek.
                        catch (Exception ex)
                        {
                            throw new FileServiceInvalidEmployeesFormat($"Nie można dodać pracownika z linii {nrLinii + 1}. {ex.Message}", ex);
                        }
                    }
                }

                //Jeśli plik się nie wczyta rzucamy wyjątek.
                catch (Exception ex)
                {
                    throw new FileServiceInvalidEmployeesFormat($"Plik Pracownicy.txt jest uszkodzony. {ex.Message}", ex);
                }
            }

            //Zapisywanie danych o pracownikach do pliku "Pracownicy.txt".
            public void ZapiszPracownikow(string plik)
            {
                List<string> linie = new List<string>();            //Tu przechowujemy kolejne linie pliku.

                //Dla każdej osoby pobieramy dane i zapisujemy do pliku.
                for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
                {
                    Employee employee = _employeeManager.GetEmployeeById(nrOsoby);          //Pracownik.

                    //Jeśli pracownik istnieje to formatujemy dane.
                    if (employee != null)
                    {
                        string linia = string.Join(" ",
                            employee.Numer,
                            employee.Imie,
                            employee.Nazwisko,
                            employee.Zaleglosci,
                            employee.CzyTriazDzien,
                            employee.CzyTriazNoc);

                        linie.Add(linia);
                    }
                    else
                    {
                        // Jeśli brak pracownika, dodaj pustą linijkę
                        linie.Add(string.Empty);
                    }
                }

                File.WriteAllLines(plik, linie);
            }
        }
    }
}
