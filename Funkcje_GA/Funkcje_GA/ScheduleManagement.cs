using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;

namespace Funkcje_GA
{
    public class ScheduleManagement : IShifts
    {
        private IEmployees EmpManager { get; set; }                     //Instancja menadżera pracowników.
        private IUISchedule UIControlManager { get; set; }              //Instancja do zarządzania UI.

        private readonly List<Shift> schedule;                          //Lista przechowuje grafik.

        //Konstruktor
        public ScheduleManagement(IEmployees EmpManager, IUISchedule uIManager)
        {
            //Tworzymy pusty grafik i instancję employeeManager.
            schedule = new List<Shift>(2 * LICZBA_DNI);
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                Shift newShift = new Shift(i);
                schedule.Add(newShift);
            }

            this.EmpManager = EmpManager;
            this.UIControlManager = uIManager;
        }

        //Dodawanie pracownika do grafiku.
        public void AddToShift(int shiftId, int employeeId)
        {
            Employee employee = EmpManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");       //Pracownik.
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");                            //Zmiana.

            //Jeżeli pracownika nie ma na zmianie to go dodajemy, inkrementujemy wymiar etatu i odświeżamy listboxa.
            if (!shift.Present_employees.Contains(employee))
            {
                shift.Present_employees.Add(employee);
                employeeManager.EmployeeEdit(employee, employee.WymiarEtatu + 1.0);
                UpdateScheduleControls(shift);
            }

        }

        //Pobierz zmianę o określonym Id.
        public Shift GetShiftById(int id) => schedule.First(sched => (sched != null && sched.Id == id));

        //Usuwanie całego grafiku.
        public void RemoveAll()
        {
            //Czyścimy wszystkie zmiany.
            foreach (Shift shift in schedule)
            {
                schedule[shift.Id].Present_employees.Clear();
                schedule[shift.Id].Sala_employees.Clear();
                schedule[shift.Id].Triaz_employees.Clear();
            }

            //Usuwamy dane o wymiarze etatu.
            employeeManager.EmployeeEdit(0.0);

            //Usuwamy dane z listBoxów
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                UIControlManager.ClearControlById(nrZmiany);
        }

        public void RemoveFromShift(int shiftId, int employeeId)
        {
            Employee employee = EmpManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee))
            {
                shift.Present_employees.Remove(employee);
                shift.Sala_employees.Remove(employee);
                shift.Triaz_employees.Remove(employee);
                employeeManager.EmployeeEdit(employee, employee.WymiarEtatu - 1.0);
                UpdateScheduleControls(shift);
            }
        }

        public void ToBezFunkcji(int shiftId, int employeeId)
        {
            Employee employee = EmpManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee))
            {
                if (shift.Sala_employees.Contains(employee))
                {
                    shift.Sala_employees.Remove(employee);
                    UpdateScheduleControls(shift);
                }

                else if (shift.Triaz_employees.Contains(employee))
                {
                    shift.Triaz_employees.Remove(employee);
                    UpdateScheduleControls(shift);
                }
            }
        }

        public void ToSala(int shiftId, int employeeId)
        {
            Employee employee = EmpManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee) && !shift.Sala_employees.Contains(employee))
            {
                if (shift.Triaz_employees.Contains(employee))
                    shift.Triaz_employees.Remove(employee);

                shift.Sala_employees.Add(employee);
                UpdateScheduleControls(shift);
            }
        }

        public void ToTriaz(int shiftId, int employeeId)
        {
            Employee employee = EmpManager.GetEmployeeById(employeeId) ?? throw new ArgumentNullException("Osoba nie istnieje.");
            Shift shift = schedule[shiftId] ?? throw new ArgumentNullException("Zmiana nie została utworzona.");

            if (shift.Present_employees.Contains(employee) && !shift.Triaz_employees.Contains(employee))
            {
                if (shift.Sala_employees.Contains(employee))
                    shift.Sala_employees.Remove(employee);

                shift.Triaz_employees.Add(employee);
                UpdateScheduleControls(shift);
            }
        }

        private void UpdateScheduleControls(Shift shift)
        {
            UIControlManager.ClearControlById(shift.Id);
            for (int nrOsoby = 0; nrOsoby < shift.Present_employees.Count; nrOsoby++)
            {
                if (shift.Present_employees.Count > 0)
                    UIControlManager.AddToControl(shift.Id, shift.Present_employees[nrOsoby].Numer.ToString());
            }

            for (int liczbaSal = 0; liczbaSal < shift.Sala_employees.Count; liczbaSal++)
            {
                if (shift.Sala_employees.Count > 0)
                    UIControlManager.GetElementById(shift.Id).ToSala(UIControlManager.GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Sala_employees[liczbaSal].Numer.ToString()));
            }

            for (int liczbaTriazy = 0; liczbaTriazy < shift.Triaz_employees.Count; liczbaTriazy++)
            {
                if (shift.Triaz_employees.Count > 0)
                    UIControlManager.GetElementById(shift.Id).ToTriaz(UIControlManager.GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Triaz_employees[liczbaTriazy].Numer.ToString()));
            }
        }
    }
}
