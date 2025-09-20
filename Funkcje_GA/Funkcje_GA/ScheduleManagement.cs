using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    public class ScheduleManagement : IScheduleManagement
    {
        private EmployeeManagement _employeeManager { get; set; }                     //Instancja menadżera pracowników.
        private readonly List<Shift> schedule;                                     //Lista przechowuje grafik.

        //Konstruktor
        public ScheduleManagement(EmployeeManagement EmpManager)
        {
            //Tworzymy pusty grafik i instancję employeeManager.
            schedule = new List<Shift>(2 * LICZBA_DNI);
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                Shift newShift = new Shift(i);
                schedule.Add(newShift);
            }

            this._employeeManager = EmpManager;
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
            if (!shift.Present_employees.Contains(employee))
            {
                //Próbujemy dodać pracownika do zmiany.
                shift.Present_employees.Add(employee);
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
            if (shiftId < 0 || shiftId >= schedule.Count)
                throw new ScheduleInvalidScheduleIdException($"Numer zmiany {shiftId} jest niepoprawny");

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            if(_employeeManager.GetEmployeeById(employeeId) == null) throw new ArgumentNullException("Osoba nie istnieje.");     //Pracownik.
            if(schedule[shiftId] == null) throw new ArgumentNullException("Zmiana nie została utworzona.");        //Zmiana.
        }

        //Zwraca zmiany i pełnione na nich funkcje danego pracownika.
        public IEnumerable<(int shiftId, int function)> GetShiftsForEmployee(int employeeId)
        {
            var result = new List<(int, int)>();                        //Lista zmian i pełnionych funkcji.

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            //Sprawdzamy po kolei każdą zmianę.
            foreach (var shift in schedule)
            {
                //Sprawdzamy, czy pracownik występuje.
                if (shift.Present_employees.Any(e => e.Numer == employeeId))
                {
                    int function = (int)FunctionTypes.Bez_Funkcji;          //Funkcja pracownika.

                    //Sprawdzamy, czy pracownik ma salę.
                    if (shift.Sala_employees.Any(e => e.Numer == employeeId))
                        function = (int)FunctionTypes.Sala;

                    //Sprawdzamy, czy pracownik ma triaż.
                    if (shift.Triaz_employees.Any(e => e.Numer == employeeId))
                        function = (int)FunctionTypes.Triaz;

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
                shift.Present_employees.Clear();
                shift.Sala_employees.Clear();
                shift.Triaz_employees.Clear();
                ShiftChanged?.Invoke(shift);
            }

            //Usuwamy dane o wymiarze etatu.
            foreach (Employee employee in _employeeManager.GetAllActive())
                _employeeManager.EmployeeEdit(employee, 0.0);
        }

        public void RemoveFromShift(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Jeśli pracownik był na zmianie to go usuwamy.
            if (shift.Present_employees.Contains(employee))
            {
                shift.Present_employees.Remove(employee);
                shift.Sala_employees.Remove(employee);
                shift.Triaz_employees.Remove(employee);

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

        //Przydziela pracownikowi dyżur bez funkcji. 
        public void ToBezFunkcji(int shiftId, int employeeId)
        {
            bool flagInvoke = false;            //Określa, czy nastąpi wywołanie.
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Sprawdzamy, czy pracownik jest na zmianie.
            if (shift.Present_employees.Contains(employee))
            {
                //Jeśli miał salę to usuwamy i wzywamy.
                if (shift.Sala_employees.Contains(employee))
                {
                    shift.Sala_employees.Remove(employee);
                    flagInvoke = true;
                }

                //Jeśli miał triaż to usuwamy i wzywamy.
                else if (shift.Triaz_employees.Contains(employee))
                {
                    shift.Triaz_employees.Remove(employee);
                    flagInvoke = true;
                }

                //Wywołanie po zmianie funkcji.
                if(flagInvoke)
                    ShiftChanged?.Invoke(shift);
            }
        }

        //Przydziela pracownikowi o wybranym Id salę na danej zmianie. 
        public void ToSala(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId); //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Sprawdzamy, czy pracownik jest na zmianie i czy nie ma już sali.
            if (shift.Present_employees.Contains(employee) && !shift.Sala_employees.Contains(employee))
            {
                //Jeśli miał triaż to usuwamy.
                if (shift.Triaz_employees.Contains(employee))
                    shift.Triaz_employees.Remove(employee);

                //Dodajemy do sali i wzywamy subskrybentów.
                shift.Sala_employees.Add(employee);
                ShiftChanged?.Invoke(shift);
            }
        }

        //Przydziela pracownikowi o wybranym Id triaż na danej zmianie. 
        public void ToTriaz(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            Shift shift = schedule[shiftId];        //Zmiana.

            //Sprawdzamy, czy pracownik jest na zmianie i czy nie ma już triażu.
            if (shift.Present_employees.Contains(employee) && !shift.Triaz_employees.Contains(employee))
            {
                //Jeśli miał salę to usuwamy.
                if (shift.Sala_employees.Contains(employee))
                    shift.Sala_employees.Remove(employee);

                //Dodajemy do triażu i wzywamy subskrybentów.
                shift.Triaz_employees.Add(employee);
                ShiftChanged?.Invoke(shift);
            }
        }
    }
}
