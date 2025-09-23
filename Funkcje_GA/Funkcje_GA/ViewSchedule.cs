using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za zarządzanie wyświetlaniem grafiku na UI.
    public class ViewSchedule : IViewSchedule
    {
        private readonly Dictionary<int, List<string>> uiScheduleControls;      //Tu przechowywane są kontrolki z danymi grafiku.

        private readonly IScheduleManagement _scheduleManager;                  //Instancja do zarządzania grafikiem.

        //Konstruktor.
        public ViewSchedule(IScheduleManagement scheduleManager)
        {
            this._scheduleManager = scheduleManager;

            uiScheduleControls = new Dictionary<int, List<string>> (2 * LICZBA_DNI);                               //Inicjalizujemy zestaw kontrolek grafiku.

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                List<string> list = new List<string>();
                uiScheduleControls[i] = list;
            }
        }


        //Akcja zmiana kontrolki grafiku.
        public event Action<int, List<string>> ScheduleControlChanged;

        //Powiadomienie użytkownika.
        public event Action<string> UserNotificationRaise;

        //Dodawanie pracownika do zmiany.
        public void AddEmployeeToShift(int shiftId, int employeeId) => _scheduleManager.AddToShift(shiftId, employeeId);

        //Czyszczenie grafiku.
        public void ClearSchedule()
        {
            //Usuwamy grafik.
            _scheduleManager.RemoveAll();

            //Odświeżamy wszystkie kontrolki grafiku.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                ScheduleControlChanged?.Invoke(shiftId, new List<string>());
            }

            // Powiadamiamy View
            UserNotificationRaise?.Invoke("Grafik usunięty");
        }

        //Usuwamy zaznaczone dyżury.
        public void RemoveSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
            {
                _scheduleManager.RemoveFromShift(shiftId, employeeId);
            }
        }

        //Przypisujemy wybranym pracownikom brak funkcji.
        public void SetSelectedShiftsToBezFunkcji(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToBezFunkcji(shiftId, employeeId);
        }

        //Przypisujemy wybranym pracownikom sale.
        public void SetSelectedShiftsToSala(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToSala(shiftId, employeeId);
        }

        //Przypisujemy wybranym pracownikom triaz.
        public void SetSelectedShiftsToTriaz(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToTriaz(shiftId, employeeId);
        }

        //Wyświetlamy dane wybranej zmiany.
        public void UpdateScheduleControl(Shift shift)
        {
            //Czyścimy kontrolkę.
            if (uiScheduleControls.TryGetValue(shift.Id, out List<string> list))
            {
                list.Clear();
            }

            else return;

            //Wyświetlamy pracowników
            if (shift.PresentEmployees.Count > 0)
            {
                //Próbujemy dodać pracowników do kontrolki.
                try
                {
                    for (int nrOsoby = 0; nrOsoby < shift.PresentEmployees.Count; nrOsoby++)
                    {
                        //Dopisujemy pracownika, jeśli ma salę.
                        if (shift.SalaEmployees.Any(emp => (shift.PresentEmployees[nrOsoby].Numer == emp.Numer)))
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString() + "s");

                        //Dopisujemy pracownika, jeśli ma triaż.
                        else if (shift.TriazEmployees.Any(emp => (shift.PresentEmployees[nrOsoby].Numer == emp.Numer)))
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString() + "t");

                        //Dopisujemy pracownika, jeśli nie ma funkcji.
                        else
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString());

                    }
                }

                //Jeśli się nie udało to rzucamy wyjatek.
                catch (Exception ex)
                {
                    throw new FormatException($"Kontrolka: {shift.Id} ma niepoprawne dane {ex.Message}.", ex);
                }
                ;
            }
            //Podnosimy zdarzenie, zmiana kontrolki grafiku.
            ScheduleControlChanged?.Invoke(shift.Id, uiScheduleControls[shift.Id]);
        }
    }
}
