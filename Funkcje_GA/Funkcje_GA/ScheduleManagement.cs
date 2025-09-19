using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;

namespace Funkcje_GA
{
    public class ScheduleManagement : IScheduleManagement
    {
        private EmployeeManagement _employeeManager { get; set; }                     //Instancja menadżera pracowników.

        private readonly List<Shift> schedule;                          //Lista przechowuje grafik.

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
            Employee employee = _employeeManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");       //Pracownik.
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");                            //Zmiana.

            //Jeżeli pracownika nie ma na zmianie to go dodajemy, inkrementujemy wymiar etatu i odświeżamy listboxa.
            if (!shift.Present_employees.Contains(employee))
            {
                shift.Present_employees.Add(employee);
                _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu + 1.0);
                ShiftChanged?.Invoke(shift);
            }

        }


        //Pobierz zmianę o określonym Id.
        public Shift GetShiftById(int id) => schedule[id];

        public IEnumerable<(int shiftId, int function)> GetShiftsForEmployee(int employeeId)
        {
            var result = new List<(int, int)>();

            foreach (var shift in schedule)
            {
                if (shift.Present_employees.Any(e => e.Numer == employeeId))
                {
                    int function = (int)FunctionTypes.Bez_Funkcji;

                    if (shift.Sala_employees.Any(e => e.Numer == employeeId))
                        function = (int)FunctionTypes.Sala;

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
                schedule[shift.Id].Present_employees.Clear();
                schedule[shift.Id].Sala_employees.Clear();
                schedule[shift.Id].Triaz_employees.Clear();
                ShiftChanged?.Invoke(shift);
            }

            //Usuwamy dane o wymiarze etatu.
            foreach (Employee employee in _employeeManager.GetAllActive())
                _employeeManager.EmployeeEdit(employee, 0.0);
        }

        public bool RemoveFromShift(int shiftId, int employeeId)
        {
            Employee employee = _employeeManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee))
            {
                shift.Present_employees.Remove(employee);
                shift.Sala_employees.Remove(employee);
                shift.Triaz_employees.Remove(employee);
                _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu - 1.0);
                ShiftChanged?.Invoke(shift);

                return true;
            }

            else return false;
        }

        public void ToBezFunkcji(int shiftId, int employeeId)
        {
            Employee employee = _employeeManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee))
            {
                if (shift.Sala_employees.Contains(employee))
                {
                    shift.Sala_employees.Remove(employee);
                    ShiftChanged?.Invoke(shift);
                }

                else if (shift.Triaz_employees.Contains(employee))
                {
                    shift.Triaz_employees.Remove(employee);
                    ShiftChanged?.Invoke(shift);
                }
            }
        }

        public void ToSala(int shiftId, int employeeId)
        {
            Employee employee = _employeeManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee) && !shift.Sala_employees.Contains(employee))
            {
                if (shift.Triaz_employees.Contains(employee))
                    shift.Triaz_employees.Remove(employee);

                shift.Sala_employees.Add(employee);
                ShiftChanged?.Invoke(shift);
            }
        }

        public void ToTriaz(int shiftId, int employeeId)
        {
            Employee employee = _employeeManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee) && !shift.Triaz_employees.Contains(employee))
            {
                if (shift.Sala_employees.Contains(employee))
                    shift.Sala_employees.Remove(employee);

                shift.Triaz_employees.Add(employee);
                ShiftChanged?.Invoke(shift);
            }
        }
    }
}
