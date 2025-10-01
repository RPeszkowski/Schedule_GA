using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Funkcje_GA.Model;
using Serilog;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za zarządzanie wyświetlaniem grafiku na UI.
    internal class PresenterSchedule
    {
        private readonly Dictionary<int, List<string>> uiScheduleControls;      //Tu przechowywane są kontrolki z danymi grafiku.

        private readonly IScheduleManagement _scheduleManager;                  //Instancja do zarządzania grafikiem.
        private readonly IViewSchedule _viewSchedule;                           //Interfejs do wyświetlania grafiku na UI.
        
        //Konstruktor.
        public PresenterSchedule(IScheduleManagement scheduleManager, IViewSchedule viewSchedule)
        {
            this._scheduleManager = scheduleManager;
            this._viewSchedule = viewSchedule;

            uiScheduleControls = new Dictionary<int, List<string>> (2 * LICZBA_DNI);                               //Inicjalizujemy zestaw kontrolek grafiku.

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                List<string> list = new List<string>();
                uiScheduleControls[i] = list;
            }

            //Subskrybujemy event - modyfikacja zmiany.
            _scheduleManager.ShiftChanged += shifts => UpdateScheduleControl(shifts);

            //Subskrybujemy event - dodanie pracownika do zmiany.
            _viewSchedule.EmployeeAddedToShift += (shiftId, employeeId) =>
            {
                AddEmployeeToShift(shiftId, employeeId);
            };

            //Subskrybujemy event - grafik wyczyszczony.
            _viewSchedule.ScheduleCleared += () => _scheduleManager.RemoveAll();

            //Subskrybujemy event - ustawienie funkcji.
            _viewSchedule.SelectedShiftsAssigned += (selected, ft) => SetSelectedShifts(selected, ft);

            //Subskrybujemy event - usunięcie pracownika ze zmiany.
            _viewSchedule.SelectedShiftsRemoved += (selected) => RemoveSelectedShifts(selected);
        }

        //Dodawanie pracownika do zmiany.
        private void AddEmployeeToShift(int shiftId, int employeeId) => _scheduleManager.AddToShift(shiftId, employeeId);

        //Usuwamy zaznaczone dyżury.
        private void RemoveSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            var result = new List<IShift>();                               //Lista dyżurów do usunięcia.
            foreach (var (shiftId, employeeId) in selected)
            {
                //Usuwamy pracownika.
                _scheduleManager.RemoveFromShift(shiftId, employeeId);
                result.Add(_scheduleManager.GetShiftById(shiftId));
            }

            //Uaktualniamy kontrolkę.
            UpdateScheduleControl(result);
        }

        //Przypisujemy funkcje.
        private void SetSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected, FunctionTypes function)
        {
            var result = new List<IShift>();                               //Lista dyżurów do zmiany.

            //Przypisujemy w oparciu o argument function.
            foreach (var (shiftId, employeeId) in selected)
            {
                //Zmieniamy dyżury.
                _scheduleManager.AssignFunctionToEmployee(shiftId, employeeId, function);
                result.Add(_scheduleManager.GetShiftById(shiftId));
            }

            //Uaktualniamy kontrolki.
            UpdateScheduleControl(result);
        }

        //Wyświetlamy dane wybranej zmiany.
        public void UpdateScheduleControl(IEnumerable<IShift> shifts)
        {
            foreach (var shift in shifts)
            {
                //Czyścimy kontrolkę.
                if (uiScheduleControls.TryGetValue(shift.Id, out List<string> list))
                {
                    list.Clear();
                }

                else return;

                var employees = shift.GetEmployees();       //Pobieramy pracowników.

                //Wyświetlamy pracowników
                if (shift.GetEmployees().Count() > 0)
                {
                    //Próbujemy dodać pracowników do kontrolki.
                    try
                    {
                        for (int nrOsoby = 0; nrOsoby < employees.Count(); nrOsoby++)
                        {
                            //Dopisujemy pracownika, jeśli ma salę.
                            if (shift.GetEmployeesByFunction(FunctionTypes.Sala).Any(emp => (employees.ToList()[nrOsoby].Numer == emp.Numer)))
                                uiScheduleControls[shift.Id].Add(employees.ToList()[nrOsoby].Numer.ToString() + "s");

                            //Dopisujemy pracownika, jeśli ma triaż.
                            else if (shift.GetEmployeesByFunction(FunctionTypes.Triaz).Any(emp => (employees.ToList()[nrOsoby].Numer == emp.Numer)))
                                uiScheduleControls[shift.Id].Add(employees.ToList()[nrOsoby].Numer.ToString() + "t");

                            //Dopisujemy pracownika, jeśli nie ma funkcji.
                            else
                                uiScheduleControls[shift.Id].Add(employees.ToList()[nrOsoby].Numer.ToString());
                        }
                    }

                    //Jeśli się nie udało to rzucamy wyjatek.
                    catch (Exception ex)
                    {
                        throw new FormatException($"Kontrolka: {shift.Id} ma niepoprawne dane {ex.Message}.", ex);
                    }
                    ;
                }

                //Zmiana kontrolki.
                _viewSchedule.UpdateShift(shift.Id, uiScheduleControls[shift.Id]);
            }
        }
    }
}
