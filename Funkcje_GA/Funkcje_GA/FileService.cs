using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za operacje na plikach.
    public class FileService
    {
        //Klasa odpowiada za zapis i odczyt danych grafiku do pliku "Grafik.txt".
        public class FileManagementGrafik
        {
            private readonly EmployeeManagement _employeeManager;               //Instancja do zarządzania pracownikami.
            private readonly ScheduleManagement _scheduleManager;         //Instancja do zarządzania grafikiem.

            //Konstruktor
            public FileManagementGrafik(EmployeeManagement empManager, ScheduleManagement scheduleManager)
            {
                //Przypisanie instancji.
                this._employeeManager = empManager;
                this._scheduleManager = scheduleManager;
            }

            //Metoda służy do wczytywania grafiku z pliku "Grafik.txt".
            public void WczytajGrafik(string plik)
            {
                bool flagBreak = false;                 //Flaga do wychodzenia z zewnętrznej pętli for.
                string linijka;                         //Wczytana linijka tekstu.
                int nrOsoby;                            //Numer pojedynczej osoby bez znaczku funkcji.
                string nrOsobyZFunkcja;                 //Numer pojedynczej osoby ze znaczkiem funkcji.
                string nrOsobyBezFunkcji;               //Numer pojedynczej osoby bez znaczku funkcji.
                string[] osobyNaZmianieSplit;           //Numery osób z funkcjami, rozdzielone.

                //Usuwamy grafik.
                _scheduleManager.RemoveAll();

                //Próbujemy wczytać grafik.
                try
                {
                    //Wczytujemy po kolei każdą linijkę.
                    for (int nrLinii = 0; nrLinii < 2 * LICZBA_DNI; nrLinii++)
                    {
                        //Wczytujemy linijke, dzielimy dane w miejscach, gdzie jest spacja.
                        linijka = File.ReadAllLines(plik).Skip(nrLinii).Take(1).First();
                        osobyNaZmianieSplit = linijka.Split(' ');

                        //Dla każdego dyzuru w linijce dodajemy zmianę do grafiku.
                        for (int nrDyzuru = 0; nrDyzuru < osobyNaZmianieSplit.Length - 1; nrDyzuru++)
                        {
                            //Wybieramy pojedynczy numer osoby.
                            nrOsobyZFunkcja = osobyNaZmianieSplit[nrDyzuru];

                            //Próbujemy dodać osobę do grafiku.
                            try
                            {
                                //Jeśli osoba ma przydzieloną salę lub triaż to usuwamy literkę.
                                if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 's' || nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 't')
                                    nrOsobyBezFunkcji = nrOsobyZFunkcja.Remove(nrOsobyZFunkcja.Length - 1);

                                else
                                    nrOsobyBezFunkcji = nrOsobyZFunkcja;

                                //Conwertujemy numer osoby do inta.
                                nrOsoby = Convert.ToInt32(nrOsobyBezFunkcji);

                                //Sprawdzamy, czy numer osoby jest poprawny.
                                if (nrOsoby < 1 || nrOsoby > MAX_LICZBA_OSOB)
                                    throw new InvalidDataException("Numer osoby musi być liczbą naturalną z zakresu 1 - 50.");

                                //Sprawdzamy, czy osoba istnieje w bazie.
                                else if (_employeeManager.GetEmployeeById(nrOsoby) == null)
                                    throw new InvalidDataException("Osoba nie istnieje w bazie pracowników");

                                //Jeśli wszystko jest ok to dodajemy dyżur do grafiku.
                                else
                                {
                                    _scheduleManager.AddToShift(nrLinii, nrOsoby);

                                    if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 's')
                                        _scheduleManager.ToSala(nrLinii, nrOsoby);

                                    if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 't')
                                        _scheduleManager.ToTriaz(nrLinii, nrOsoby);
                                }
                            }

                            //Jeśli się nie udało wyświetlamy komunikat i wychodzimy z pętli z flagą flagBreak.
                            catch
                            {
                                //Komunikat, gdy nie udało się wycztać dziennej zmiany.
                                if (nrLinii < LICZBA_DNI)
                                    MessageBox.Show("Nie udało się wczytać grafiku dla dnia: " + (nrLinii + 1).ToString() + " dyżur dzienny.");

                                //Komunikat, gdy nie udało się wycztać nocnej zmiany.
                                else
                                    MessageBox.Show("Nie udało się wczytać grafiku dla dnia: " + (nrLinii + 1 - LICZBA_DNI).ToString() + " dyżur nocny.");

                                //Czyścimy grafik, ustawiamy flagę flagBreak, wychodzimy z pętli.
                                _scheduleManager.GetShiftById(nrLinii).Present_employees.Clear();
                                _scheduleManager.GetShiftById(nrLinii).Sala_employees.Clear();
                                _scheduleManager.GetShiftById(nrLinii).Triaz_employees.Clear();
                                flagBreak = true;
                                break;
                            }
                        }

                        //Jeśli nie udało się wczytać grafiku wychodzimy z pętli i przerywamy działanie programu.
                        if (flagBreak)
                            break;
                    }
                }

                //Jeśli się nie udało, to wyświetlamy komunikat i czyścimy grafik.
                catch
                {
                    MessageBox.Show("Nie udało się wczytać grafiku.");
                    _scheduleManager.RemoveAll();
                }
            }

            //Metoda służy do zapisywania grafiku do pliku "Grafik.txt".
            public void ZapiszGrafik(string plik)
            {
                //Czyśicmy plik, a jeśli nie istniał to go tworzymy.
                File.WriteAllText(plik, "");

                //Każda zmiana odpowiada jednej linijce tekstu w pliku.
                for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                {
                    string str = "";                                //Linijka odpowiadająca jednej zmianie.
                    int nrOsoby;                                    //Numer osoby.
                    //Jeżeli zmiana jest obsadzona t oformatujemy string.
                    if (_scheduleManager.GetShiftById(nrZmiany).Present_employees.Count > 0)
                    {
                        for (int j = 0; j < _scheduleManager.GetShiftById(nrZmiany).Present_employees.Count; j++)
                        {
                            //Pobieramy numer osoby. Jeśli ma salę to dopisujemy "s" plus spacja.
                            nrOsoby = _scheduleManager.GetShiftById(nrZmiany).Present_employees[j].Numer;
                            if (_scheduleManager.GetShiftById(nrZmiany).Sala_employees.Contains(_employeeManager.GetEmployeeById(nrOsoby)))
                                str = str + nrOsoby.ToString() + "s ";

                            //Jeśli osoba ma triaż to dopisujemy "t" plus spacja.
                            else if (_scheduleManager.GetShiftById(nrZmiany).Triaz_employees.Contains(_employeeManager.GetEmployeeById(nrOsoby)))
                                str = str + nrOsoby.ToString() + "t ";

                            //Jeśli osoba jest bez funkcji to dopisujemy spację.
                            else
                                str = str + nrOsoby.ToString() + " ";
                        }

                        //Na koniec dopisujemy przejście do nowej linii.
                        str += "\n";
                    }

                    //Jeżeli zmiana nie jest obsadzona, to linijka tekstu ma postać jak poniżej.
                    else
                        str += "\n";

                    //Zapisujemy linijkę tekstu.
                    File.AppendAllText(plik, str);
                }
            }
        }

        //Klasa odpowiada za zapis i odczyt danych pracowników do pliku "Pracownicy.txt".
        public class FileManagementPracownicy
        {
            private readonly EmployeeManagement _employeeManager;               //Instancja do zarządzania pracownikami.

            //Ten słownik zawiera informacje o formacie danych w pliku "Pracownicy.txt".
            private static readonly Dictionary<string, int> pracownicy_txt = new Dictionary<string, int>()
            {
                {"NUMER", 0 }, {"IMIE", 1}, {"NAZWISKO", 2}, {"ZALEGLOSCI", 3}, {"TRIAZ_DZIEN", 4}, {"TRIAZ_NOC", 5}
            };

            //Konstruktor
            public FileManagementPracownicy(EmployeeManagement empManager)
            {
                //Przypisanie instancji.
                this._employeeManager = empManager;
            }

            //Wczytujemy dane pracowników z pliku "Pracownicy.txt".
            public void WczytajPracownikow(string plik)
            {
                for (int nrLinii = 0; nrLinii < MAX_LICZBA_OSOB; nrLinii++)
                {
                    string wczytanaLinia;                   //Wczytana linijka.
                    string[] LiniaSplit;                    //Wczytana linijka, wyrazy oddzielone spacją.

                    //Wczytujemy linijkę tekstu i rozdzielamy wyrazy.
                    try
                    {
                        wczytanaLinia = File.ReadAllLines(plik).Skip(nrLinii).Take(1).First();
                        LiniaSplit = wczytanaLinia.Split(' ');
                    }

                    catch
                    {
                        MessageBox.Show("Plik Pracownicy.txt jest uszkodzony. Napraw go lub usuń.");
                        return;
                    }

                    //Próbujemy sprawdzić numer wczytanego pracownika.
                    if (Int32.TryParse(LiniaSplit[pracownicy_txt["NUMER"]], out int numer))
                    {
                        //Próbujemy dodać nowego pracownika.
                        try
                        {
                            //Dodajemy nowego pracownika.
                            _employeeManager.EmployeeAdd(numer, LiniaSplit[pracownicy_txt["IMIE"]],
                                                        LiniaSplit[pracownicy_txt["NAZWISKO"]], 0.0,
                                                        Convert.ToInt32(LiniaSplit[pracownicy_txt["ZALEGLOSCI"]]),
                                                        Convert.ToBoolean(LiniaSplit[pracownicy_txt["TRIAZ_DZIEN"]]),
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
            public void ZapiszPracownikow(string plik)
            {
                //Tworzymy pusty plik lub czyścimy istniejący.
                File.WriteAllText(plik, "");

                //Dla każdej osoby (jeśli istnieje) dopisujemy nową linijkę do pliku z danymi pracownika. Jeśli nie istnieje wpisujemy pustą linijkę.
                for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
                {
                    //Jeśli osoba istnieje to wpisz dane.
                    if (_employeeManager.GetEmployeeById(nrOsoby) != null)
                    {
                        string danePracownika = _employeeManager.GetEmployeeById(nrOsoby).Numer.ToString() + " "
                                              + _employeeManager.GetEmployeeById(nrOsoby).Imie + " "
                                              + _employeeManager.GetEmployeeById(nrOsoby).Nazwisko + " "
                                              + _employeeManager.GetEmployeeById(nrOsoby).Zaleglosci.ToString() + " "
                                              + _employeeManager.GetEmployeeById(nrOsoby).CzyTriazDzien.ToString() + " "
                                              + _employeeManager.GetEmployeeById(nrOsoby).CzyTriazNoc.ToString() + "\n";
                        File.AppendAllText(plik, danePracownika);
                    }

                    //Jeśli osoba nie istnieje to wpisz pustą linijkę.
                    else
                        File.AppendAllText(plik, "\n");
                }
            }
        }
    }
}
