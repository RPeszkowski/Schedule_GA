using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klas odpowiada za wyświetlanie informacji o pracownikach.
    public class PresenterEmployee
    {
        private readonly Dictionary<int, (string, EmployeeLabelStatus)> uiEmployeesControls;           //Tu przechowywane są kontrolki z danymi pracowników.

        private readonly IEmployeeManagement _employeeManager;                          //Instancja do zarządzania pracownikami.
        private readonly IScheduleManagement _scheduleManager;                          //Instancja do zarządzania grafikiem.
        private readonly IViewEmployee _viewEmployee;                                         //Instancja widoku Form1.  
        private readonly IViewForm2 _viewForm2;                                         //Instancja widoku Form2.  

        //Konstruktor
        public PresenterEmployee(IEmployeeManagement employeeManager, IScheduleManagement scheduleManager, IViewEmployee viewEmployee, IViewForm2 viewForm2)
        {
            this._employeeManager = employeeManager;
            this._scheduleManager = scheduleManager;
            this._viewEmployee = viewEmployee;
            this._viewForm2 = viewForm2;

            uiEmployeesControls = new Dictionary<int, (string, EmployeeLabelStatus)>(MAX_LICZBA_OSOB);

            for (int i = 1; i <= MAX_LICZBA_OSOB; i++)
                uiEmployeesControls[i] = ("", 0);

            //Subskrybujemy event - modyfikacja danych pracownika.
            _employeeManager.EmployeeChanged += (emp) => UpdateEmployeeLabel(emp); 

            //Subskrybujemy event - usunięcie danych pracownika.
            _employeeManager.EmployeeDeleted += (id) => ClearEmployeeLabel(id);

            //Subskrybujemy akcję - prośba o podświetlenie kontrolek.
            _viewEmployee.EmployeeLabelMouseDown += (employeeId) =>
            {
                var highlights = HandleEmployeeMouseDown(employeeId);
                _viewEmployee.HandleEmployeeMouseDown(highlights);
            };

            //Subskrybujemy akcję - dodawanie pracownika z UI.
            _viewForm2.EmployeeAddedFromUI += (imie, nazwisko, zaleglosci, triazDzien, triazNoc) => 
            {
                try
                {
                    //Szukamy wolnego numeru.
                    int wolnyNumer = Enumerable.Range(1, MAX_LICZBA_OSOB).FirstOrDefault(i => _employeeManager.GetEmployeeById(i) == null);

                    if (wolnyNumer == 0)
                        throw new TooManyEmployeesException($"Maksymalna liczba osób to: {MAX_LICZBA_OSOB}.");

                    //Tworzymy osobę z pierwszym wolnym numerem i danymi takimi, jakie zostały wprowadzone do boxów.
                    //Dodajemy nową osobę do listy osób. Wyświetlamy numery istniejących w systemie osób.
                    _employeeManager.EmployeeAdd(wolnyNumer, imie, nazwisko, 0.0, zaleglosci, triazDzien, triazNoc);
                    var lista = GetActiveEmployeesNumbers();
                    _viewForm2.UpdateControlNumerOsoby(lista);
                }

                //Sprawdzamy, czy nie została osiągnięta maksymalna liczba osób w systemie.
                catch (TooManyEmployeesException ex)
                {
                    Log.Error(ex.Message);
                    _viewForm2.RaiseUserNotification($"Maksymalna liczba pracowników to {MAX_LICZBA_OSOB}.");
                }

                //Obsługa wyjątku: niepoprawne dane.
                catch (EmployeeNameSurnameException ex)
                {
                    Log.Error(ex.Message);
                    _viewForm2.RaiseUserNotification("Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
                }
            };

            //Subskrybujemy akcję - edycja danych pracownika z UI.
            _viewForm2.EmployeeEditedFromUI += (nrOsoby, imie, nazwisko, zaleglosci, triazDzien, triazNoc) =>
            {
                try
                {
                    Employee employee = _employeeManager.GetEmployeeById(nrOsoby);       //Pracownik.

                    //Edycja pracownika, informacja i uaktualnienie listy osób.
                    _employeeManager.EmployeeEdit(employee, imie, nazwisko, employee.WymiarEtatu, zaleglosci, triazDzien, triazNoc);
                    _viewForm2.RaiseUserNotification($"Zmieniono dane pracownika: {employee.Numer} {employee.Imie} {employee.Nazwisko}.");
                    var lista = GetActiveEmployeesNumbers();
                    _viewForm2.UpdateControlNumerOsoby(lista);
                }

                //Obsługa wyjątku: niepoprawne dane.
                catch (EmployeeNameSurnameException ex)
                {
                    Log.Error(ex.Message);
                    _viewForm2.RaiseUserNotification("Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
                }
            };

            //Subskrybujemy akcję - usunieto pracownika z UI
            _viewForm2.EmployeeDeletedFromUI += (nrOsoby) =>
            {
                //Usuwamy pracownika, odświeżamy UI, wyświetlamy komunikat.
                _employeeManager.EmployeeDelete(nrOsoby);
                var lista = GetActiveEmployeesNumbers();
                _viewForm2.UpdateControlNumerOsoby(lista);
                _viewForm2.RaiseUserNotification("Usunięto dane pracownika.");
            };

            //Subskrybujemy akcję - załadowano Form2.
            _viewForm2.Form2Loaded += () =>
            {
                //Wyświetlamy numery aktywnych pracowników.
                var lista = GetActiveEmployeesNumbers();
                _viewForm2.UpdateControlNumerOsoby(lista);
            };

            //Subskrybujemy akcję - zmieniono wyświetlanego pracownika.
            _viewForm2.SelectedEmployeeChanged += (nrOsoby) =>
            {
                var employee = _employeeManager.GetEmployeeById(nrOsoby);       //Pracownik.

                //Wyświetlamy dane pracownika.
                _viewForm2.ShowEmployeeData(employee.Imie, employee.Nazwisko, employee.Zaleglosci, employee.CzyTriazDzien, employee.CzyTriazNoc);
            };
        }

        //Czyścimy kontrolkę z danymi pracownika.
        public void ClearEmployeeLabel(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli tak, to resetujemy tekst.
            if (id < 1 || id > MAX_LICZBA_OSOB) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            uiEmployeesControls[id] = ("", (int)EmployeeLabelStatus.Normal);

            //Uaktualniamy opis pracownika.
            _viewEmployee.UpdateEmployeeLabel(id, uiEmployeesControls[id], false);
        }

        //Pobieramy numery aktywnych pracowników.
        private List<int> GetActiveEmployeesNumbers()
        {
            List<int> lista = new List<int>();                            //Lista numerów aktywnych pracowników.

            //Pobieramy aktywnych i zamieniamy numery na string.
            IEnumerable<Employee> employees = _employeeManager.GetAllActive();
            foreach (Employee emp in employees)
                lista.Add(emp.Numer);

            return lista;
        }

        //Funkcja realizowana po naciśnięciu etykiety pracownika.
        public IEnumerable<(int shiftId, FunctionTypes function)> HandleEmployeeMouseDown(int employeeId)
        {
            //Pobieramy pracownika.
            var employee = _employeeManager.GetEmployeeById(employeeId);
            if (employee == null) yield break;

            //Sprawdzamy funkcje i wyświetlamy kolor. Bez funkcji - czerwony, sala - zielony, triaż - niebieski.
            var shifts = _scheduleManager.GetShiftsForEmployee(employee.Numer);

            if (shifts == null)
                yield break;

            foreach (var (shiftId, function) in shifts)
            {
                yield return (shiftId, function);
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
                uiEmployeesControls[employee.Numer] = (employeeData, EmployeeLabelStatus.Normal);

            else
                uiEmployeesControls[employee.Numer] = (employeeData, EmployeeLabelStatus.Intern);

            //Wywołujemy zdarzenie auktualniono opis pracownika.
            _viewEmployee.UpdateEmployeeLabel(employee.Numer, uiEmployeesControls[employee.Numer], true);
        }
    }
}
