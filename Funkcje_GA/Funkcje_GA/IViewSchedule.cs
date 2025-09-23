using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy ViewSchedule z resztą kodu.
    public interface IViewSchedule
    {
        //Akcja zmiana kontrolki grafiku.
        event Action<int, List<string>> ScheduleControlChanged;

        //Powiadomienie użytkownika.
        event Action<string> UserNotificationRaise;

        //Dodawanie pracownika do zmiany.
        void AddEmployeeToShift(int shiftId, int employeeId);

        //Usuwanie grafiku.
        void ClearSchedule();

        //Przypisujemy wybranym pracownikom brak funkcji.
        void SetSelectedShiftsToBezFunkcji(IEnumerable<(int ShiftId, int EmployeeId)> selected);

        //Przypisujemy wybranym pracownikom sale.
        void SetSelectedShiftsToSala(IEnumerable<(int ShiftId, int EmployeeId)> selected);

        //Przypisujemy wybranym pracownikom triaz.
        void SetSelectedShiftsToTriaz(IEnumerable<(int ShiftId, int EmployeeId)> selected);

        //Usuwamy wybrane dyżury.
        void RemoveSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected);

        //Wyświetlamy dane wybranej zmiany.
        void UpdateScheduleControl(Shift shift);
    }
}
