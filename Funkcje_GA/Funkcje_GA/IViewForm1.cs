using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfejs form1.
    public interface IViewForm1
    {
        //Zdarzenie zgłaszające, że użytkownik chce dodać pracownika do zmiany.
        event Action<int, int> EmployeeAddedToShift;

        //Zdarzenie zgłaszające, że użytkownik chce wyczyścić grafik.
        event Action ScheduleCleared;

        //Zdarzenie zgłaszające, że użytkownik chce przypisać funkcje.
        event Action<IEnumerable<(int ShiftId, int EmployeeId)>, FunctionTypes> SelectedShiftsAssigned;

        //Zdarzenie zgłaszające, że użytkownik chce usunąć pracownika ze zmiany.
        event Action<IEnumerable<(int ShiftId, int EmployeeId)>> SelectedShiftsRemoved;

        //Powiadomienie użytkownika.
        void RaiseUserNotification(string message);

        //Odświeżamy zmiany.
        void UpdateShift(int shiftId, List<string> lista);
    }
}
