using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Constants;

using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Określamy, czy osoba jest stażystą.
    public enum EmployeeLabelStatus
    {
        Normal = 0,      //Może być na triażu.
        Intern = 1     //Stażysta, ograniczenia dyżurów.
    }

    //Ta klas odpowiada za wyświetlanie informacji o pracownikach.
    public class ViewEmployee : IViewEmployee
    {
        private readonly Dictionary<int, (string, int)> uiEmployeesControls;           //Tu przechowywane są kontrolki z danymi pracowników.

        private readonly IEmployeeManagement _employeeManager;                          //Instancja do zarządzania pracownikami.
        private readonly IScheduleManagement _scheduleManager;                          //Instancja do zarządzania grafikiem.

        //Konstruktor
        public ViewEmployee(IEmployeeManagement employeeManager, IScheduleManagement scheduleManager)
        {
            this._employeeManager = employeeManager;
            this._scheduleManager = scheduleManager;

            uiEmployeesControls = new Dictionary<int, (string, int)>(MAX_LICZBA_OSOB);

            for (int i = 1; i <= MAX_LICZBA_OSOB; i++)
                uiEmployeesControls[i] = ("", 0);
        }

        //Akcja zmiana danych pracownika.
        public event Action<int, (string, int)> EmployeeLabelChanged;

        //Czyścimy kontrolkę z danymi pracownika.
        public void ClearEmployeeLabel(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli tak, to resetujemy tekst.
            if (id < 1 || id > MAX_LICZBA_OSOB) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            uiEmployeesControls[id] = ("", (int)EmployeeLabelStatus.Normal);

            //Wywołujemy zdarzenie uaktualniono opis pracownika.
            EmployeeLabelChanged?.Invoke(id, uiEmployeesControls[id]);
        }

        //Funkcja realizowana po naciśnięciu etykiety pracownika.
        public IEnumerable<(int shiftId, Color color)> HandleEmployeeMouseDown(int employeeId)
        {
            //Pobieramy pracownika.
            var employee = _employeeManager.GetEmployeeById(employeeId);
            if (employee == null) yield break;

            //Sprawdzamy funkcje i wyświetlamy kolor. Bez funkcji - czerwony, sala - zielony, triaż - niebieski.
            var shifts = _scheduleManager.GetShiftsForEmployee(employee.Numer);
            foreach (var (shiftId, function) in shifts)
            {
                Color color;

                //Wybieramy kolor w zalezności od funkcji. Bez funkcji - czerwony, sala - zielony, triaż - niebieski.
                switch (function)
                {
                    case (int)FunctionTypes.Bez_Funkcji:
                        color = Color.Red;
                        break;

                    case (int)FunctionTypes.Sala:
                        color = Color.Green;
                        break;

                    case (int)FunctionTypes.Triaz:
                        color = Color.Blue;
                        break;

                    default:
                        color = Color.White;
                        break;
                }

                //Event podśiwetlenie.
                yield return (shiftId, color);
            }
        }

        //Wyświetlanie informacji o pracowniku na etykiecie.
        public void UpdateEmployeeLabel(Employee employee)
        {
            if (employee == null) return;

            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli nie, to rzucamy wyjątek.
            if (employee.Numer < 1 || employee.Numer > MAX_LICZBA_OSOB) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            //Aktualizujemy pojedynczą etykietę.
            string employeeData = employee.Numer.ToString() + ". "
                                    + employee.Imie + " " + employee.Nazwisko + " "
                                    + employee.WymiarEtatu.ToString() + " "
                                    + employee.Zaleglosci.ToString();

            //Jeśli osoba jest nie jest stazystą i może być na triażu w dzień i w nocy to jest wyświetlana na czarno.
            //Jeśli jest stażystą i nie może być na triażu w za dnia i/lub w nocy to jest podświetlana na pomarańczowo.
            if (employee.CzyTriazDzien && employee.CzyTriazNoc)
                uiEmployeesControls[employee.Numer] = (employeeData, (int)EmployeeLabelStatus.Normal);

            else
                uiEmployeesControls[employee.Numer] = (employeeData, (int)EmployeeLabelStatus.Intern);

            //Wywołujemy zdarzenie auktualniono opis pracownika.
            EmployeeLabelChanged?.Invoke(employee.Numer, uiEmployeesControls[employee.Numer]);
        }
    }
}
