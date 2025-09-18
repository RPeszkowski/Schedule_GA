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
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;
using static Funkcje_GA.Employee;
using static Funkcje_GA.ScheduleManagement;
using static Funkcje_GA.Shift;
using static Funkcje_GA.FileService;
using static Funkcje_GA.UIForm1Management;
using static Funkcje_GA.IEmployees;
using static Funkcje_GA.IUISchedule;
using static Funkcje_GA.IShifts;

namespace Funkcje_GA
{
    public partial class Form1 : Form
    {
        //Ta klasa odpowiada za zarządzanie pracownikami.
        public class EmployeeManagement : IEmployees
        {
            private readonly List<Employee>  employees;                                 //Deklaracja listy pracowników.

            //Konstruktor.
            public EmployeeManagement()
            {
                //Tworzymy listę i wypełniamy wartościami null. Tworzymy menadżera UI.
                employees = new List<Employee>(MAX_LICZBA_OSOB);                        //Lista pracowników.
                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    employees.Add(null);
                }
            }

            //Usuwanie pracownika.
            public void EmployeeDelete(Employee employee)
            {
                //Próbujemy usunąć osobę z grafiku.
                try
                {
                    //Usuwamy etykietę, usuwamy osobę, na koniec wyświetlamy komunikat.
                    labelsPracownicy[employee.Numer - 1].Text = "";
                    employees[employee.Numer - 1] = null;
                    MessageBox.Show("Usunięto dane pracownika.");
                }

                //Jeśli się nie udało, to wyświetlamy komunikat.
                catch { MessageBox.Show("Nie udało się usunąć osoby."); }
            }

            //Dodawanie nowego pracownika.
            public void EmployeeAdd(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Dodawanie pracownika o konkretnym numerze.
                //Sprawdzamy, czy nie osiągnięto maksymalne liczby pracowników.
                if (employeeManager.GetAllEmployeesNotNull().Count() >= MAX_LICZBA_OSOB)
                    throw new TooManyEmployeesException("Liczba osób wynosi obecnie " + employeeManager.GetAllEmployeesNotNull().Count().ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

                //Sprawdzamy, czy numer osoby jest poprawny.
                if (numer > 0 && numer >= MAX_LICZBA_OSOB)
                    throw new InvalidDataException("Numer osoby wynosił " + numer.ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

                //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
                if (imie.Contains(' ') || nazwisko.Contains(' '))
                    throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

                //Sprawdzamy, czy imię i nazwisko nie są puste.
                else if (imie == "" || nazwisko == "")
                    throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

                //Tworzymy nową osobę, sprawdzamy, czy numer jest poprawny, dodajemy do tabeli.
                Employee newEmployee = new Employee(numer, imie, nazwisko, wymiarEtatu, zaleglosci, czyTriazDzien, czyTriazNoc);
                employees[numer - 1] = newEmployee;
                UpdateEmployeeLabel(employees[numer - 1]);
            }
            public void EmployeeAdd(string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Dodawanie pracownika o pierwszym wolnym numerze.
                //Sprawdzamy, czy nie osiągnięto maksymalne liczby pracowników.
                if (employeeManager.GetAllEmployeesNotNull().Count() >= MAX_LICZBA_OSOB)
                    throw new TooManyEmployeesException("Liczba osób wynosi obecnie " + employeeManager.GetAllEmployeesNotNull().Count().ToString() + " podczas, gdy maksimum to " + MAX_LICZBA_OSOB.ToString() + " .");

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
                    if (employees[i] == null)
                        wolnyNumer = i;
                }

                //Tworzymy nową osobę, sprawdzamy, dodajemy do tabeli i pdświeżamy etykietę.
                Employee newEmployee = new Employee(wolnyNumer + 1, imie, nazwisko, wymiarEtatu, zaleglosci, czyTriazDzien, czyTriazNoc);
                employees[wolnyNumer] = newEmployee;
                UpdateEmployeeLabel(employees[wolnyNumer]);
            }

            //Edycja danych pracownika.
            public void EmployeeEdit(Employee employee, double wymiarEtatu)
            {
                //Edycja danych jednej osoby. Tylko wymiar etatu.
                //Sprawdzamy, czy osoba istnieje.
                if (employee == null)
                    throw new NullReferenceException("Dana osoba nie istnieje");

                employee.WymiarEtatu = wymiarEtatu;
                UpdateEmployeeLabel(employee);
            }
            public void EmployeeEdit(double wymiarEtatu)
            {
                //Edycja danych wszystkich osób. Tylko wymiar etatu.
                foreach (Employee employee in employees)
                {
                    if (employee != null)
                        employee.WymiarEtatu = wymiarEtatu;
                }

                UpdateEmployeeLabel();
            }
            public void EmployeeEdit(Employee employee, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
            {
                //Edycja danych jednej osoby. Pełne dane.
                //Sprawdzamy, czy osoba istnieje.
                if (employee == null)
                    throw new NullReferenceException("Dana osoba nie istnieje");

                //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
                if (imie.Contains(' ') || nazwisko.Contains(' '))
                    throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

                //Sprawdzamy, czy imię i nazwisko nie są puste.
                else if (imie == "" || nazwisko == "")
                    throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

                employee.Imie = imie;
                employee.Nazwisko = nazwisko;
                employee.WymiarEtatu = wymiarEtatu;
                employee.Zaleglosci = zaleglosci;
                employee.CzyTriazDzien = czyTriazDzien;
                employee.CzyTriazNoc = czyTriazNoc;
                UpdateEmployeeLabel(employee);
            }

            //Interfejs do wybierani wszystkich pracowników.
            public IEnumerable<Employee> GetAllEmployees() => employees;

            //Interfejs do wybierani wszystkich pracowników (tylko pola niepuste).
            public IEnumerable<Employee> GetAllEmployeesNotNull() => employees.Where(emp => emp != null);

            //Interfejs do wybierania pracowników.
            public Employee GetEmployeeById(int numer) => employees.First(emp => (emp != null && emp.Numer == numer));

            //Wyświetlanie informacji o pracowniku na etykiecie.
            public void UpdateEmployeeLabel(Employee employee)
            {
                //Aktualizujemy pojedynczą etykietę.
                labelsPracownicy[employee.Numer - 1].Text = employee.Numer.ToString() + ". " + employee.Imie + " " + employee.Nazwisko + " " + employee.WymiarEtatu.ToString() + " " + employee.Zaleglosci.ToString();

                //Jeśli osoba jest stażystą to podświetlamy.
                if (!(employee.CzyTriazDzien && employee.CzyTriazNoc))
                    labelsPracownicy[employee.Numer - 1].ForeColor = Color.Orange;

                else
                    labelsPracownicy[employee.Numer - 1].ForeColor = Color.Black;
            }
            public void UpdateEmployeeLabel()
            {
                //Aktualizujemy wszystkie etykiety.
                foreach (Employee employee in employees)
                {
                    if (employee != null)
                    {
                        //Aktualizujemy wszystkie etykiety.
                        labelsPracownicy[employee.Numer - 1].Text = employee.Numer.ToString() + ". " + employee.Imie + " " + employee.Nazwisko + " " + employee.WymiarEtatu.ToString() + " " + employee.Zaleglosci.ToString();

                        //Jeśli osoba jest stażystą to podświetlamy.
                        if (!(employee.CzyTriazDzien && employee.CzyTriazNoc))
                            labelsPracownicy[employee.Numer - 1].ForeColor = Color.Orange;

                        else
                            labelsPracownicy[employee.Numer - 1].ForeColor = Color.Black;
                    }
                }
            }
        }

        //Klasa odpowiada za zapis i odczyt danych grafiku do pliku "Grafik.txt".
        private class FileManagementGrafik
        {
            //Metoda służy do wczytywania grafiku z pliku "Grafik.txt".
            public static void WczytajGrafik(string plik)
            {
                bool flagBreak = false;                 //Flaga do wychodzenia z zewnętrznej pętli for.
                string linijka;                         //Wczytana linijka tekstu.
                int nrOsoby;                            //Numer pojedynczej osoby bez znaczku funkcji.
                string nrOsobyZFunkcja;                 //Numer pojedynczej osoby ze znaczkiem funkcji.
                string nrOsobyBezFunkcji;               //Numer pojedynczej osoby bez znaczku funkcji.
                string[] osobyNaZmianieSplit;           //Numery osób z funkcjami, rozdzielone.

                //Usuwamy grafik.
                scheduleManager.RemoveAll();

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
                                else if (employeeManager.GetEmployeeById(nrOsoby) == null)
                                    throw new InvalidDataException("Osoba nie istnieje w bazie pracowników");

                                //Jeśli wszystko jest ok to dodajemy dyżur do grafiku.
                                else
                                {
                                    scheduleManager.AddToShift(nrLinii, nrOsoby);

                                    if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 's')
                                        scheduleManager.ToSala(nrLinii, nrOsoby);
                                    
                                    if (nrOsobyZFunkcja[nrOsobyZFunkcja.Length - 1] == 't')
                                        scheduleManager.ToTriaz(nrLinii, nrOsoby);
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
                                scheduleManager.GetShiftById(nrLinii).Present_employees.Clear();
                                scheduleManager.GetShiftById(nrLinii).Sala_employees.Clear();
                                scheduleManager.GetShiftById(nrLinii).Triaz_employees.Clear();
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
                    scheduleManager.RemoveAll();
                }
            }

            //Metoda służy do zapisywania grafiku do pliku "Grafik.txt".
            public static void ZapiszGrafik(string plik)
            {
                //Czyśicmy plik, a jeśli nie istniał to go tworzymy.
                File.WriteAllText(plik, "");

                //Każda zmiana odpowiada jednej linijce tekstu w pliku.
                for (int nrZmiany = 0; nrZmiany < 2*LICZBA_DNI; nrZmiany++)
                {
                    string str = "";                                //Linijka odpowiadająca jednej zmianie.
                    int nrOsoby;                                    //Numer osoby.
                    //Jeżeli zmiana jest obsadzona t oformatujemy string.
                    if(scheduleManager.GetShiftById(nrZmiany).Present_employees.Count > 0)
                    {
                        for (int j = 0; j < scheduleManager.GetShiftById(nrZmiany).Present_employees.Count; j++)
                        {
                            //Pobieramy numer osoby. Jeśli ma salę to dopisujemy "s" plus spacja.
                            nrOsoby = scheduleManager.GetShiftById(nrZmiany).Present_employees[j].Numer;
                            if (scheduleManager.GetShiftById(nrZmiany).Sala_employees.Contains(employeeManager.GetEmployeeById(nrOsoby)))
                                str = str + nrOsoby.ToString() + "s ";

                            //Jeśli osoba ma triaż to dopisujemy "t" plus spacja.
                            else if (scheduleManager.GetShiftById(nrZmiany).Triaz_employees.Contains(employeeManager.GetEmployeeById(nrOsoby)))
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
            //Wczytujemy dane pracowników z pliku "Pracownicy.txt".
            public static void WczytajPracownikow(string plik)
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
                            employeeManager.EmployeeAdd(numer, LiniaSplit[pracownicy_txt["IMIE"]], 
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
            public static void ZapiszPracownikow(string plik)
            {
                //Tworzymy pusty plik lub czyścimy istniejący.
                File.WriteAllText(plik, "");

                //Dla każdej osoby (jeśli istnieje) dopisujemy nową linijkę do pliku z danymi pracownika. Jeśli nie istnieje wpisujemy pustą linijkę.
                for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
                {
                    //Jeśli osoba istnieje to wpisz dane.
                    if (employeeManager.GetEmployeeById(nrOsoby) != null)
                    {
                        string danePracownika = employeeManager.GetEmployeeById(nrOsoby).Numer.ToString() + " " 
                                              + employeeManager.GetEmployeeById(nrOsoby).Imie + " "
                                              + employeeManager.GetEmployeeById(nrOsoby).Nazwisko + " "
                                              + employeeManager.GetEmployeeById(nrOsoby).Zaleglosci.ToString() + " "
                                              + employeeManager.GetEmployeeById(nrOsoby).CzyTriazDzien.ToString() + " "
                                              + employeeManager.GetEmployeeById(nrOsoby).CzyTriazNoc.ToString() + "\n";
                        File.AppendAllText(plik, danePracownika);
                    }

                    //Jeśli osoba nie istnieje to wpisz pustą linijkę.
                    else
                        File.AppendAllText(plik, "\n");
                }
            }
        }

        //Ta klasa zawiera ListBoxy przedstawiające grafik.
        public class ListBoxGrafik : ListBox
        {
            public int Id { get; set; }                 //Numer listBoxa.

            //Konstruktor.
            public ListBoxGrafik(int id)
            {
                this.Id = id;
            }

            //Zwraca funkcję danej osoby. 0 - bez funkcji, 1 - sala, 2 - triaż.
            public int GetFunction(int index)
            {
                //Sprawdzamy poprawność danych.
                try
                {
                    CheckData(index);
                }

                //Jeśli dane są błędne to wyświetlamy komunikat.
                catch
                {
                    MessageBox.Show("Zły format danych grafiku.");
                }

                int nrFunkcji;                                      //Numer funkcji. 0 - bez funkcji, 1 - sala, 2 - triaż.
                string str = this.Items[index].ToString();          //Numer osoby, ewentualnie z dodatkową literą.

                //Jeśli funkcja to sala zwracamy 1.
                if (str[str.Length - 1] == 's')
                    nrFunkcji = 1;

                //Jeśli funkcja to triaż zwracamy 2.
                else if (str[str.Length - 1] == 't')
                    nrFunkcji = 2;

                //Jeśli nie ma funkcj izwracamy 0.
                else
                    nrFunkcji = 0;

                //Zwracamy wartość.
                return nrFunkcji;
            }

            //Zwraca numer osoby wskazanej przez index.
            public int GetNumber(int index)
            {
                //Sprawdzamy poprawność danych.
                try
                {
                    CheckData(index);
                }

                //Jeśli dane są błędne to wyświetlamy komunikat.
                catch
                {
                    MessageBox.Show("Zły format danych grafiku.");
                }

                int number;                                         //Numer pracownika.
                string str = this.Items[index].ToString();          //Wartość pobrana z listBoxa przerobiona na string.

                //Jeśli jest literka s lub t to ją usuwamy. Konwertujemy do int.
                if (str[str.Length - 1] == 's')
                    str = str.Remove(str.Length - 1);

                if (str[str.Length - 1] == 't')
                    str = str.Remove(str.Length - 1);

                number = Convert.ToInt32(str);

                //Zwracamy numer osoby.
                return number;
            }

            //Zmienia funkcję wybranej osoby na bez funkcji.
            public void ToBezFunkcji(int index)
            {
                //Sprawdzamy poprawność danych.
                try
                {
                    CheckData(index);
                }

                //Jeśli dane są błędne to wyświetlamy komunikat.
                catch
                {
                    MessageBox.Show("Zły format danych grafiku.");
                }

                string str = this.Items[index].ToString();                          //Wartość pobrana z listBoxa przerobiona na string.

                //Jeśli jest literka s lub t to ją usuwamy i podmieniamy item w listBoxie.
                if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                    str = str.Remove(str.Length - 1);

                this.Items[index] = str;
            }

            //Zmienia funkcję wybranej osoby na salę.
            public void ToSala(int index)
            {
                //Sprawdzamy poprawność danych.
                try
                {
                    CheckData(index);
                }

                //Jeśli dane są błędne to wyświetlamy komunikat.
                catch
                {
                    MessageBox.Show("Zły format danych grafiku.");
                }

                string str = this.Items[index].ToString();              //Wartość pobrana z listBoxa przerobiona na string.

                //Jeśli jest literka s to nic nie robimy.
                if (str[str.Length - 1] == 's')
                    this.Items[index] = str;

                //Jeśli jest literka t to podmieniamy na s.
                else if (str[str.Length - 1] == 't')
                {
                    str = str.Remove(str.Length - 1);
                    this.Items[index] = str + 's';
                }

                //Jeśli nie ma literki to dopisujemy s.
                else this.Items[index] = str + 's';
            }

            //Zmienia funkcję wybranej osoby na triaż.
            public void ToTriaz(int index)
            {
                //Sprawdzamy poprawność danych.
                try
                {
                    CheckData(index);
                }

                //Jeśli dane są błędne to wyświetlamy komunikat.
                catch
                {
                    MessageBox.Show("Zły format danych grafiku.");
                }

                string str = this.Items[index].ToString();                  //Wartość pobrana z listBoxa przerobiona na string.

                //Jeśli jest literka t to nic nie robimy.
                if (str[str.Length - 1] == 't')
                    this.Items[index] = str;

                //Jeśli jest literka s to podmieniamy na t.
                else if (str[str.Length - 1] == 's')
                { 
                    str = str.Remove(str.Length - 1);
                    this.Items[index] = str + 't';
                }

                //Jeśli nie ma literki to dopisujemy t.
                else this.Items[index] = str + 't';
            }

            //Sprawdzamy poprawność danych.
            private void CheckData(int index)
            {
                //Sprawdzamy, czy wybrany item istnieje.
                if (this.Items == null)
                    throw new InvalidDataException("ListBox " + this.Name + " nie zawiera elementów.");

                //Pobieramy item jako string.
                string str = this.Items[index].ToString();

                //Sprawdzamy, czy item nie jest pusty.
                if (str == "")
                    throw new InvalidDataException("Element " + index.ToString() + " ListBoxa " + this.Name + " jest pusty.");

                //Sprawdzamy, czy item jest liczbą z ewentualną literą s lub t.
                if (!Int32.TryParse(str, out int number))
                {
                    if (str[str.Length - 1] != 's' && str[str.Length - 1] != 't')
                        throw new InvalidDataException("Niepoprawne dane " + str + " w ListBoxie" + this.Name + " .");
                }
            }
        }

        private class Optimization
        {
            //Obiekty tej klasy są wykorzystywane w optymalizacji genetycznej.
            private class Osobnik
            {
                public bool[] genom;            //Rozwiązanie związane z danym osobnikiem.
                public decimal wartosc;         //Wartość funkcji celu dla danego rozwiązania.

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
            private static Employee[] employees = employees = employeeManager.GetAllEmployees().ToArray();
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
                            scheduleManager.ToSala(i, scheduleManager.GetShiftById(i).Present_employees[nrSala].Numer);
                            scheduleManager.ToTriaz(i, scheduleManager.GetShiftById(i).Present_employees[nrTriaz1].Numer);
                            scheduleManager.ToTriaz(i, scheduleManager.GetShiftById(i).Present_employees[nrTriaz2].Numer);
                        }

                        else
                        {
                            scheduleManager.ToSala(i, scheduleManager.GetShiftById(i).Present_employees[nrSala].Numer);
                            scheduleManager.ToTriaz(i, scheduleManager.GetShiftById(i).Present_employees[nrTriaz1].Numer);
                            scheduleManager.ToTriaz(i, scheduleManager.GetShiftById(i).Present_employees[nrTriaz2].Numer);
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
                    if (employees[i] != null)
                        dyzuryRozstaw[i] = new bool[Convert.ToInt32(employees[i].WymiarEtatu)];

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

            //Generuje tablicę z numerami osób, które nie powinny mieć triażu za dnia.
            private static int[] ListaNieTiazDzien()
            {
                //Pobieramy listę osób, które nie powinny być przypisane do triażu w ciągu dnia i zamieniamy na tablicę int.
                Employee[] employeesNieTriazDzien = employeeManager.GetAllEmployeesNotNull().Where(emp => emp.CzyTriazDzien == false).ToArray();
                int[] nieTriazDzien = new int[employeesNieTriazDzien.Count()];          //Liczba osób, które nie powinny mieć triażu w ciągu dnia.
                for (int i = 0; i < employeesNieTriazDzien.Count(); i++)
                    nieTriazDzien[i] = employeesNieTriazDzien[i].Numer;

                //Zwracamy tablicę int.
                return nieTriazDzien;
            }

            //Generuje tablicę z numerami osób, które nie powinny mieć triażu w nocy.
            private static int[] ListaNieTiazNoc()
            {
                //Pobieramy listę osób, które nie powinny być przypisane do triażu w ciągu nocy i zamieniamy na tablicę int.
                Employee[] employeesNieTriazNoc = employeeManager.GetAllEmployeesNotNull().Where(emp => emp.CzyTriazNoc == false).ToArray();
                int[] nieTriazNoc = new int[employeesNieTriazNoc.Count()];          //Liczba osób, które nie powinny mieć triażu w ciągu nocy.
                for (int i = 0; i < employeesNieTriazNoc.Count(); i++)
                    nieTriazNoc[i] = employeesNieTriazNoc[i].Numer;

                //Zwracamy tablicę int.
                return nieTriazNoc;
            }

            private static double[] OczekiwanaLiczbaFunkcji(Employee[] osoby)
            {
                const double MAX_LICZBA_FUNKCJI = 8.5;
                const double MIN_LICZBA_FUNKCJI = 2.5;
                double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB];
                int liczbaDniRoboczych = 0;
                double sumaEtatow = 0.0;

                for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
                {
                    if(scheduleManager.GetShiftById(shiftId).Present_employees.Count() > 0)
                        liczbaDniRoboczych++;
                }

                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    if (osoby[i] != null)
                        sumaEtatow += Convert.ToInt32(osoby[i].WymiarEtatu);
                }

                for (int i = 0; i < MAX_LICZBA_OSOB; i++)
                {
                    if (osoby[i] != null)
                    {
                        oczekiwanaLiczbaFunkcji[i] = Math.Min(Math.Max(((3 * liczbaDniRoboczych * osoby[i].WymiarEtatu / sumaEtatow) - osoby[i].Zaleglosci), 0), MAX_LICZBA_FUNKCJI);
                        if (osoby[i].WymiarEtatu > 0.1)
                        {
                            oczekiwanaLiczbaFunkcji[i] = Math.Max(oczekiwanaLiczbaFunkcji[i], MIN_LICZBA_FUNKCJI);
                        }
                    }
                }
                return oczekiwanaLiczbaFunkcji;
            }

            //Algorytm optymalizacji genetycznej.
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
            
            //Przygotowanie danych do optymalizacji.
            public static void Prepare()
            {
                //Sprawdzamy, czy liczba pracowników na każdej zmianie wynosi 0 lub od 3 do MAX_LICZBA_DYZUROW.
                for(int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
                {
                    if (scheduleManager.GetShiftById(shiftId).Present_employees.Count() == 1 || scheduleManager.GetShiftById(shiftId).Present_employees.Count() == 2)
                        throw new InvalidDataException("Za mało pracowników na zmianie: " + shiftId.ToString() + " .");

                    if (scheduleManager.GetShiftById(shiftId).Present_employees.Count() > MAX_LICZBA_DYZUROW)
                        throw new InvalidDataException("Za dużo pracowników na zmianie: " + shiftId.ToString() + " .");
                }

                //Przygotowujemy niezbędne dane.
                dyzuryGrafik = UtworzGrafik();
                nieTriazDzien = ListaNieTiazDzien();
                nieTriazNoc = ListaNieTiazNoc();
                liczbaDyzurow = LiczbaDyzurow(dyzuryGrafik);
                oczekiwanaLiczbaFunkcji = OczekiwanaLiczbaFunkcji(employees);
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
                int nrZmiany;
                int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];

                for (int nrDyzuru = 0; nrDyzuru < 2 * LICZBA_DNI * MAX_LICZBA_DYZUROW; nrDyzuru++)
                {
                    nrZmiany = Convert.ToInt32(Math.Floor(Convert.ToDouble(nrDyzuru) / MAX_LICZBA_DYZUROW));
                    if (scheduleManager.GetShiftById(nrZmiany).Present_employees.Count() > nrDyzuru % MAX_LICZBA_DYZUROW)
                    {
                        scheduleManager.ToBezFunkcji(nrZmiany, scheduleManager.GetShiftById(nrZmiany).Present_employees[nrDyzuru % MAX_LICZBA_DYZUROW].Numer);
                        dyzuryGrafik[nrDyzuru] = scheduleManager.GetShiftById(nrZmiany).Present_employees[nrDyzuru % MAX_LICZBA_DYZUROW].Numer;
                    }

                    else
                        dyzuryGrafik[nrDyzuru] = 0;
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

        private static System.Windows.Forms.Label[] labelsPracownicy = new System.Windows.Forms.Label[MAX_LICZBA_OSOB];     //Tworzenie etykiet wyświetlających dane pracowników.

        private TimeSpan czasOptymalizacja;                                                         //Pomiar czasu działania algorytmu optymalizacji.
        public static EmployeeManagement employeeManager = new EmployeeManagement();                //Instancja do zarządzania pracownikami.   
        private static UIForm1Management uiManager = new UIForm1Management();                                  //Instancja do zarządzania kontrolkami wyświetlającymi grafik.
        private static ScheduleManagement scheduleManager = new ScheduleManagement(employeeManager, uiManager); //Instancja do zarządzania grafikiem.
        private DateTime startOptymalizacja;                                                        //Pomiar czasu działania algorytmu optymalizacji.
        
        //Konstruktor.
        public Form1()
        {
            //Generuje większość kontrolek. Metoda stworzona przez Designera.
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
                {
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
            }
        }

        //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                //Zamieniamy wszystkie wybrane dyżury na bez funkcyjne.
                if (uiManager.GetSelectedIndex(nrZmiany) != -1)
                    scheduleManager.ToBezFunkcji(nrZmiany, uiManager.GetSelectedEmployeeNumber(nrZmiany, uiManager.GetSelectedIndex(nrZmiany)));
            }
        }

        //Usuwamy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana. Usuwamy grafik. Wyświetlamy informację.
            UsunPodswietlenie();
            scheduleManager.RemoveAll();
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
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                //Zamieniamy wszystkie wybrane dyżury na sale.
                if (uiManager.GetSelectedIndex(nrZmiany) != -1)
                    scheduleManager.ToSala(nrZmiany, uiManager.GetSelectedEmployeeNumber(nrZmiany, uiManager.GetSelectedIndex(nrZmiany)));
            }
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy wszystkie wybrane dyzury na triaż.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                //Zamieniamy wszystkie wybrane dyżury na triaż.
                if (uiManager.GetSelectedIndex(nrZmiany) != -1)
                    scheduleManager.ToTriaz(nrZmiany, uiManager.GetSelectedEmployeeNumber(nrZmiany, uiManager.GetSelectedIndex(nrZmiany)));
            }
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Usuwamy dyżur.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                int nrOsoby;                        //Numer osoby;

                //Usuwamy zaznaczone dyżury.
                if (uiManager.GetSelectedIndex(nrZmiany) != -1)
                {
                    nrOsoby = uiManager.GetSelectedEmployeeNumber(nrZmiany, uiManager.GetSelectedIndex(nrZmiany));
                    scheduleManager.RemoveFromShift(nrZmiany, nrOsoby);
                }
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
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                uiManager.GetElementById(nrZmiany).ClearSelected();
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Generuje kontrolki grafiku i etykiety grafiku i listy pracowników. Zdarzenie asynchroniczne przycisku optymalizacji.
        private void InitializeComponent2()
        {
            //Inicjalizujemy kontrolki grafiku.
            uiManager.InitializeControls();

            for (int nrZmiany = 0; nrZmiany <  2 *LICZBA_DNI; nrZmiany++)
            { 
                //Przypisujemy delegaty do zdarzeń drag and drop.
                int iter3 = nrZmiany;
                uiManager.GetElementById(nrZmiany).DragEnter += new DragEventHandler(this.scheduleControl_DragEnter);
                uiManager.GetElementById(nrZmiany).DragDrop += new DragEventHandler((sender, e) => scheduleControl_DragDrop(sender, e, iter3));

                //Dodawanie kontrolek do formularza.
                if (nrZmiany < LICZBA_DNI)
                    tableLayoutPanel2.Controls.Add(uiManager.GetElementById(nrZmiany), nrZmiany, 1);

                else
                    tableLayoutPanel3.Controls.Add(uiManager.GetElementById(nrZmiany), nrZmiany - LICZBA_DNI, 1);
            }

            //Tworzymy etykiety wyświetlające dane pracowników i delegaty do zdarzeń drag and drop.
            for (int nrIndeksu = 0; nrIndeksu < MAX_LICZBA_OSOB; nrIndeksu++)
            {
                //Tworzymy etykiety wyświetlające dane pracowników.
                labelsPracownicy[nrIndeksu] = new System.Windows.Forms.Label();
                labelsPracownicy[nrIndeksu].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsPracownicy[nrIndeksu].Size = Size = new System.Drawing.Size(340, 40);
                labelsPracownicy[nrIndeksu].Text = "";
                tableLayoutPanel1.Controls.Add(labelsPracownicy[nrIndeksu], nrIndeksu / 10, nrIndeksu % 10);

                //Przypisujemy delegaty do zdarzeń drag and drop.
                int nrOsoby = nrIndeksu + 1;
                labelsPracownicy[nrIndeksu].MouseDown += new MouseEventHandler((sender, e) => labelsPracownicy_MouseDown(sender, e, nrOsoby));
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

                    //Przygotowujemy dane do optymalizacji.
                    Optimization.Prepare();

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

                //Wyświetla informację, jeśli nie udało się przeprowadzić optymalizacji ze względu na złą liczbę pracowników.
                catch (InvalidDataException)
                {
                    MessageBox.Show("Aby przeprowadzić przydzielanie funkcji na każdej zmianie musi być od 3 do " + MAX_LICZBA_DYZUROW.ToString() + " .");
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

        //Drag and drop, etykieta Pracownicy.
        private void labelsPracownicy_MouseDown(object sender, MouseEventArgs e, int nrOsoby)
        {
            //Usuwamy podświetlenie, jeśli ktoś był zaznaczony.
            UsunPodswietlenie();

            //Sprawdzamy po kolei każdy dyżur, jeśli osoba występuje to podświetlamy: czerwony - bez funkcji, zielony - sala, niebieski - triaż.
            for (int nrDnia = 0; nrDnia < 2 * LICZBA_DNI; nrDnia++)
            {
                if (employeeManager.GetEmployeeById(nrOsoby) != null)
                {
                    //Sprawdzamy dyżury.
                    for (int nrDyzuru = 0; nrDyzuru < uiManager.GetElementItemsByIdAsList(nrDnia).Count; nrDyzuru++)
                    {
                        if (uiManager.GetSelectedEmployeeNumber(nrDnia, nrDyzuru) == employeeManager.GetEmployeeById(nrOsoby).Numer)
                        {
                            switch (uiManager.GetSelectedEmployeeFunction(nrDnia, nrDyzuru))
                            {
                                case 0:
                                    uiManager.GetElementById(nrDnia).BackColor = Color.Red;
                                    break;

                                case 1:
                                    uiManager.GetElementById(nrDnia).BackColor = Color.Green;
                                    break;

                                case 2:
                                    uiManager.GetElementById(nrDnia).BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }
                }
            }

            if (employeeManager.GetEmployeeById(nrOsoby) != null)
                labelsPracownicy[nrOsoby - 1].DoDragDrop(labelsPracownicy[nrOsoby - 1].Text, DragDropEffects.Copy | DragDropEffects.Move);
        }

        //Drag and drop. Efekt wizualny i kopiowanie tekstu.
        private void scheduleControl_DragEnter(object sender, DragEventArgs e)
        {
            //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        //Drag and drop. Dodajemy dyżur.
        private void scheduleControl_DragDrop(object sender, DragEventArgs e, int nrZmiany)
        {
            //Pobieramy dane, dzielimy i uzyskujemy numer osoby.
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            string[] subs = pom.Split('.');

            //Jeśli dane są ok, to dodajemy do grafiku.
            if (Int32.TryParse(subs[0], out int nrOsoby))
                scheduleManager.AddToShift(nrZmiany, nrOsoby);
        }

        //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
        private void UsunPodswietlenie()
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                uiManager.GetElementById(nrZmiany).ResetBackColor();
        }
    }
}
