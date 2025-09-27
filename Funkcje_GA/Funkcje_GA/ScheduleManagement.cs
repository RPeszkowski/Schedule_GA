using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    public class ScheduleManagement : IScheduleManagement
    {
        private IEmployeeManagement _employeeManager;                   //Instancja menadżera pracowników.
        private readonly List<Shift> schedule;                                     //Lista przechowuje grafik.

        //Konstruktor
        public ScheduleManagement(IEmployeeManagement EmpManager)
        {
            //Tworzymy pusty grafik i instancję employeeManager.
            schedule = new List<Shift>(2 * LICZBA_DNI);
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                Shift newShift = new Shift(i);
                schedule.Add(newShift);
            }

            this._employeeManager = EmpManager;

            //Subskrybujemy event - usunięcie pracownika.
            _employeeManager.EmployeeDeleted += (employeeId) =>
            {
                var shifts = GetShiftsForEmployee(employeeId);
                foreach (var shift in shifts)
                    RemoveFromShift(shift.shiftId, employeeId);
            };
        }

        //Event wywoływany przy zmianie grafiku.
        public event Action<Shift> ShiftChanged;

        //Dodawanie pracownika do grafiku.
        public void AddToShift(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.                           //Zmiana.

            //Jeżeli pracownika nie ma na zmianie to go dodajemy, inkrementujemy wymiar etatu i odświeżamy listboxa.
            if (!shift.PresentEmployees.Contains(employee))
            {
                //Próbujemy dodać pracownika do zmiany.
                shift.PresentEmployees.Add(employee);
                try
                {
                    _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu + 1.0);
                }

                //Jeśli się nie uda to rzucamy wyjatek.
                catch(Exception ex)
                {
                    throw new ScheduleEmployeeManagerException($"Nie udało się dodać pracownika do zmiany {ex.Message}.", ex);
                }

                ShiftChanged?.Invoke(shift);
            }
        }

        //Dodajemy funkcje do grafiku. Funkcja wyrzuca wyjątek przy próbie prypisania funkcji do nieistniejącej osoby
        //lub próbie przypisania dwóch funkcji jednej osobie.
        public void ApplyOptimizationToSchedule(bool[] optymalneRozwiazanie)
        {
            int nrSala;                                     //Numer pracownika, który ma salę.
            int nrTriaz1;                                   //Numer pierwszego pracownika, który ma triaż.
            int nrTriaz2;                                   //Numer drugiego pracownika, który ma triaż.
            bool[] numerOsoby = new bool[MAX_LICZBA_BITOW]; //Numer osoby binarnie.
            Shift shift;                                    //Zmiana.

            //Dla każdej zmiany zdekoduj i dodaj funkcje.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                shift = schedule[nrZmiany];             //Zmiana.

                //Sprawdzamy, czy w danym miesiącu istnieje ta zmiana.
                if (shift.PresentEmployees.Count == 0)
                    continue;

                //Sprawdzamy, czy danego dnia są dyzury.
                if (schedule[nrZmiany].PresentEmployees.Count > 0)
                {
                    //Pobieramy zapisany binarnie numer osoby, która ma salę i dekodujemy.
                    Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany, numerOsoby, 0, MAX_LICZBA_BITOW);
                    nrSala = DecodeEmployeeNumber(numerOsoby);

                    //Pobieramy zapisany binarnie numer pierwszej osoby, która ma triaż i dekodujemy.
                    Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany + MAX_LICZBA_BITOW, numerOsoby, 0, MAX_LICZBA_BITOW);

                    //Dekodujemy numer.
                    nrTriaz1 = DecodeEmployeeNumber(numerOsoby);

                    //Pobieramy zapisany binarnie numer drugiej osoby, która ma triaż i dekodujemy.
                    Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany + 2 * MAX_LICZBA_BITOW, numerOsoby, 0, MAX_LICZBA_BITOW);
                    nrTriaz2 = DecodeEmployeeNumber(numerOsoby);

                    //Pobieramy numer zmiany i sprawdzamy, czy dane z optymalizacji są poprawne.            
                    if (nrSala >= shift.PresentEmployees.Count
                        || nrTriaz1 >= shift.PresentEmployees.Count
                        || nrTriaz2 >= shift.PresentEmployees.Count)
                    {
                        throw new ScheduleFunctionEncodingException(
                            $"Jeden z numerów: {nrSala}, {nrTriaz1} lub {nrTriaz2} jest większy bądź równy liczbie pracowników");
                    }

                    //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                    if (nrSala == nrTriaz1)
                        throw new ScheduleFunctionEncodingException(
                            $"Numer: {nrSala}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                    //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                    if (nrSala == nrTriaz2)
                        throw new ScheduleFunctionEncodingException(
                            $"Numer: {nrSala}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                    //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                    if (nrTriaz2 == nrTriaz1)
                        throw new ScheduleFunctionEncodingException(
                            $"Numer: {nrTriaz1}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                    //Przypisanie funkcji.
                    AssignFunctionToEmployee(shift.Id, shift.PresentEmployees[nrSala].Numer, FunctionTypes.Sala);
                    AssignFunctionToEmployee(shift.Id, shift.PresentEmployees[nrTriaz1].Numer, FunctionTypes.Triaz);
                    AssignFunctionToEmployee(shift.Id, shift.PresentEmployees[nrTriaz2].Numer, FunctionTypes.Triaz);
                }
            }
        }

        //Zmieniamy funkcje pracownika.
        public void AssignFunctionToEmployee(int shiftId, int employeeId, FunctionTypes function)
        {
            bool flagInvoke = false;                         //Oznacza konieczność odświeżenia kontrolki grafiku.

            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Sprawdzamy, czy pracownik jest na zmianie.
            if (!shift.PresentEmployees.Contains(employee)) return;

            //Przypisujemy funkcje.
            switch (function)
            {
                //Przypisanie do bez funkcji.
                case FunctionTypes.Bez_Funkcji:
                    //Jeśli miał salę to usuwamy i wzywamy.
                    if (shift.SalaEmployees.Contains(employee))
                    {
                        shift.SalaEmployees.Remove(employee);
                        flagInvoke = true;
                    }

                    //Jeśli miał triaż to usuwamy i wzywamy.
                    if (shift.TriazEmployees.Contains(employee))
                    {
                        shift.TriazEmployees.Remove(employee);
                        flagInvoke = true;
                    }
                    break;

                //Przypisanie do sali.
                case FunctionTypes.Sala:
                    //Jeśli miał triaż to zamieniamy na salę i wzywamy.
                    if (shift.TriazEmployees.Contains(employee))
                    {
                        shift.TriazEmployees.Remove(employee);
                        shift.SalaEmployees.Add(employee);
                        flagInvoke = true;
                    }

                    //Jeśli był bez funkcji do dopisujemy salę i wzywamy.
                    else if (!shift.SalaEmployees.Contains(employee))
                    {
                        shift.SalaEmployees.Add(employee);
                        flagInvoke = true;
                    }
                    break;

                //Przypisanie do triazu.
                case FunctionTypes.Triaz:
                    //Jeśli miał salę to zamieniamy na triaż i wzywamy.
                    if (shift.SalaEmployees.Contains(employee))
                    {
                        shift.SalaEmployees.Remove(employee);
                        shift.TriazEmployees.Add(employee);
                        flagInvoke = true;
                    }

                    //Jeśli był bez funkcji do dopisujemy salę i wzywamy.
                    else if (!shift.TriazEmployees.Contains(employee))
                    {
                        shift.TriazEmployees.Add(employee);
                        flagInvoke = true;
                    }
                    break;
            }

            //Wywołanie po zmianie funkcji.
            if (flagInvoke)
                ShiftChanged?.Invoke(shift);
        }

        //Dekodujemy numer pracownika na podstawie danych z rozwiązania problemu optymalizacji.
        private int DecodeEmployeeNumber(bool[] bits)
        {
            //Sprawdzamy, czy tablica nie jest pusta.
            if (bits == null || bits.Length == 0)
                throw new ArgumentException("Tablica bitów nie może być pusta.");

            //Dekodujemy numer pracownika.
            int number = 0;
            foreach (bool bit in bits)
            {
                number = (number * 2) + (bit ? 1 : 0);
            }
            return number;
        }

        //Pobierz zmianę o określonym Id.
        public Shift GetShiftById(int id)
        {
            //Sprawdzamy, czy Id jest poprawne.
            if (id < 0 || id >= schedule.Count)
                throw new ScheduleInvalidScheduleIdException($"Numer zmiany {id} jest niepoprawny");

            return schedule[id];
        }

        //Walidacja Id.
        private void ShiftValidate(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            if (shiftId < 0 || shiftId >= 2 * LICZBA_DNI)
                throw new ScheduleInvalidScheduleIdException($"Numer zmiany {shiftId} jest niepoprawny");

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            if(_employeeManager.GetEmployeeById(employeeId) == null) throw new ArgumentNullException("Osoba nie istnieje.");     //Pracownik.
            if(schedule[shiftId] == null) throw new ArgumentNullException("Zmiana nie została utworzona.");        //Zmiana.
        }

        //Zwraca zmiany i pełnione na nich funkcje danego pracownika.
        public IEnumerable<(int shiftId, FunctionTypes function)> GetShiftsForEmployee(int employeeId)
        {
            var result = new List<(int, FunctionTypes function)> ();                        //Lista zmian i pełnionych funkcji.

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            if (_employeeManager.GetEmployeeById(employeeId) == null) throw new ArgumentNullException("Osoba nie istnieje.");     //Pracownik - sprawdzamy null.

            //Sprawdzamy po kolei każdą zmianę.
            foreach (var shift in schedule)
            {
                //Sprawdzamy, czy pracownik występuje.
                if (shift.PresentEmployees.Any(e => e.Numer == employeeId))
                {
                    FunctionTypes function = FunctionTypes.Bez_Funkcji;          //Funkcja pracownika.

                    //Sprawdzamy, czy pracownik ma salę.
                    if (shift.SalaEmployees.Any(e => e.Numer == employeeId))
                        function = FunctionTypes.Sala;

                    //Sprawdzamy, czy pracownik ma triaż.
                    if (shift.TriazEmployees.Any(e => e.Numer == employeeId))
                        function = FunctionTypes.Triaz;

                    result.Add((shift.Id, function));
                }
            }

            return result;
        }

        //Usuwanie całego grafiku.
        public void RemoveAll()
        {
            //Czyścimy wszystkie zmiany i odświeżamy kontrolki.
            foreach (Shift shift in schedule)
            {
                shift.PresentEmployees.Clear();
                shift.SalaEmployees.Clear();
                shift.TriazEmployees.Clear();
                ShiftChanged?.Invoke(shift);
            }

            //Usuwamy dane o wymiarze etatu.
            foreach (Employee employee in _employeeManager.GetAllActive())
                _employeeManager.EmployeeEdit(employee, 0.0);
        }

        //Usuwamy osobę ze zmiany.
        public void RemoveFromShift(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Jeśli pracownik był na zmianie to go usuwamy.
            if (shift.PresentEmployees.Contains(employee))
            {
                shift.PresentEmployees.Remove(employee);
                shift.SalaEmployees.Remove(employee);
                shift.TriazEmployees.Remove(employee);

                //Próbujemy usunąć pracownika.
                try
                {
                    _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu - 1.0);
                }

                //Jeśli się nie udało rzucamy wyjątek.
                catch (Exception ex)
                {
                    throw new ScheduleEmployeeManagerException($"Nie udało się dodać pracownika do zmiany {ex.Message}.", ex);
                }
                ShiftChanged?.Invoke(shift);
            }
        }
    }
}
