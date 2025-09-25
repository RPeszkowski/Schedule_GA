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

        //Przypisujemy funkcje.
        void SetSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected, FunctionTypes function);

        //Usuwamy wybrane dyżury.
        void RemoveSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected);

        //Wyświetlamy dane wybranej zmiany.
        void UpdateScheduleControl(Shift shift);
    }
}
