using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Funkcje_GA.Model;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Obiekty tej klasy przechowują informacje o zmianie.
    public class Shift : IShift
    {
        public int Id { get; set; }                                //Numer id zmiany 0 - 30 dzienne zmiany, 31 - 61 nocne zmiany.
        public List<Employee> PresentEmployees { get; set; }       //Pracownicy na zmianie.
        public List<Employee> SalaEmployees { get; set; }          //Pracownicy na sali.
        public List<Employee> TriazEmployees { get; set; }         //Pracownicy na triażu.

        //Konstruktor.
        public Shift(int Id)
        {
            PresentEmployees = new List<Employee>();
            SalaEmployees = new List<Employee>();
            TriazEmployees = new List<Employee>();
            this.Id = Id;
        }

        //Dodawanie pracownika do zmiany.
        public bool AddEmployeeToShift(Employee employee)
        {
            //Jeżeli pracownika nie ma na zmianie to go dodajemy.
            if (!PresentEmployees.Contains(employee))
            {
                PresentEmployees.Add(employee);
                return true;
            }

            else return false;
        }

        //Przypisanie funkcji
        public bool AssignFunction(Employee employee, FunctionTypes function)
        {
            //Sprawdzamy, czy pracownik jest na zmianie.
            if (!PresentEmployees.Contains(employee)) return false;

            //Przypisujemy funkcje.
            switch (function)
            {
                //Przypisanie do bez funkcji.
                case FunctionTypes.Bez_Funkcji:
                    //Jeśli miał salę to usuwamy.
                    if (SalaEmployees.Contains(employee))
                    {
                        SalaEmployees.Remove(employee);
                        return true;
                    }

                    //Jeśli miał triaż to usuwamy.
                    else if (TriazEmployees.Contains(employee))
                    {
                        TriazEmployees.Remove(employee);
                        return true;
                    }

                    else return false;

                //Przypisanie do sali.
                case FunctionTypes.Sala:
                    //Jeśli miał triaż to zamieniamy na salę.
                    if (TriazEmployees.Contains(employee))
                    {
                        TriazEmployees.Remove(employee);
                        SalaEmployees.Add(employee);
                        return true;
                    }

                    //Jeśli był bez funkcji do dopisujemy salę.
                    else
                    {
                        SalaEmployees.Add(employee);
                        return true;
                    }

                //Przypisanie do triazu.
                case FunctionTypes.Triaz:
                    //Jeśli miał salę to zamieniamy na triaż.
                    if (SalaEmployees.Contains(employee))
                    {
                        SalaEmployees.Remove(employee);
                        TriazEmployees.Add(employee);
                        return true;
                    }

                    //Jeśli był bez funkcji do dopisujemy salę.
                    else
                    {
                        TriazEmployees.Add(employee);
                        return true;
                    }

                //Jeśli wybrano default, to nic nie robimy.
                default:
                    return false;
            }
        }

        //Czyścimy zmianę.
        public bool ClearShift()
        {
            //Jeśli pracowników nie było to nie czyścimy.
            if (PresentEmployees.Count == 0) return false;

            PresentEmployees.Clear();
            SalaEmployees.Clear();
            TriazEmployees.Clear();
            return true;
        }

        //Pobieramy wszystkich pracowników.
        public IEnumerable<Employee> GetEmployees() => PresentEmployees.AsEnumerable();

        //Pobieramy wszystkich pracowników pełniących daną funkcję
        public IEnumerable<Employee> GetEmployeesByFunction(FunctionTypes function)
        {
            //Zwracamy pracowników pełniących daną funkcję.
            switch(function)
            {
                //Zwracamy pracowników bez funkcji.
                case FunctionTypes.Bez_Funkcji:
                    return PresentEmployees.AsEnumerable().Where((emp) => !SalaEmployees.Contains(emp) && !TriazEmployees.Contains(emp));

                //Zwracamy pracowników z sali.
                case FunctionTypes.Sala:
                    return SalaEmployees.AsEnumerable();

                //Zwracamy pracowników z triażu.
                case FunctionTypes.Triaz:
                    return TriazEmployees.AsEnumerable();

                //Jeśli nic nie wybrano to nic nie zwracamy.
                default:
                    return Enumerable.Empty<Employee>();
            }
        }

        //Usuwanie pracownika ze zmiany.
        public bool RemoveEmployeeFromShift(Employee employee)
        {
            //Jeśli pracownik był na zmianie to go usuwamy.
            if (PresentEmployees.Contains(employee))
            {
                PresentEmployees.Remove(employee);
                SalaEmployees.Remove(employee);
                TriazEmployees.Remove(employee);
                return true;
            }

            else return false;
        }
    }
}
