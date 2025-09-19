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
    //Ta klasa odpowiada za zarządzanie pracownikami.
    public class EmployeeManagement : IEmployeeManagement
    {
        private readonly List<Employee> employees;                                 //Deklaracja listy pracowników.

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

        //Event, który zostaje wywołany gdy zmienią się dane pracownika.
        public event Action<Employee> EmployeeChanged;

        //Event, który zostaje wywołany, gdy pracownik zostaje usunięty.
        public event Action<int> EmployeeDeleted;

        //Dodawanie nowego pracownika.
        public void EmployeeAdd(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
        {
            //Dodawanie pracownika o konkretnym numerze.
            //Sprawdzamy, czy nie osiągnięto maksymalne liczby pracowników.
            if (GetAllActive().Count() >= MAX_LICZBA_OSOB)
                throw new TooManyEmployeesException("Maksymalna liczba osób to: " + MAX_LICZBA_OSOB.ToString() + " .");

            //Sprawdzamy, czy numer osoby jest poprawny.
            if (numer < 1 || numer > MAX_LICZBA_OSOB)
                throw new InvalidDataException("Maksymalna liczba osób to: " + MAX_LICZBA_OSOB.ToString() + " .");

            //Sprawdzamy, czy imię i nazwisko nie zawierają spacji.
            if (imie.Contains(' ') || nazwisko.Contains(' '))
                throw new InvalidDataException("Imię i nazwisko nie mogą zawierać spacji.");

            //Sprawdzamy, czy imię i nazwisko nie są puste.
            else if (imie == "" || nazwisko == "")
                throw new InvalidDataException("Imię i nazwisko nie mogą mogą być puste.");

            //Tworzymy nową osobę, sprawdzamy, dodajemy.
            Employee newEmployee = new Employee(numer, imie, nazwisko, wymiarEtatu, zaleglosci, czyTriazDzien, czyTriazNoc);
            employees[numer - 1] = newEmployee;
            EmployeeChanged?.Invoke(employees[numer - 1]);
        }

        //Edycja danych pracownika.
        public void EmployeeEdit(Employee employee, double wymiarEtatu)
        {
            //Edycja danych jednej osoby. Tylko wymiar etatu.
            //Sprawdzamy, czy osoba istnieje.
            if (employee == null)
                throw new NullReferenceException("Dana osoba nie istnieje");

            employee.WymiarEtatu = wymiarEtatu;
            EmployeeChanged?.Invoke(employee);
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
            EmployeeChanged?.Invoke(employee);
        }

        //Usuwanie pracownika.
        public void EmployeeDelete(Employee employee)
        {
            //Próbujemy usunąć osobę z grafiku.
            if (employees[employee.Numer - 1] == null)
                throw new NullReferenceException("Pracownik nie istnieje w systemie");

            else
            {
                //Usuwamy etykietę, usuwamy osobę, na koniec wyświetlamy komunikat.
                EmployeeDeleted?.Invoke(employee.Numer - 1);
                // _uiManager.ClearEmployeeData(employee.Numer - 1);
                employees[employee.Numer - 1] = null;
            }
        }

        //Interfejs do wybierani wszystkich pracowników (tylko pola niepuste).
        public IEnumerable<Employee> GetAllActive() => employees.Where(emp => emp != null);

        //Interfejs do wybierania pracowników.
        public Employee GetEmployeeById(int numer) => employees[numer - 1];
    }
}
