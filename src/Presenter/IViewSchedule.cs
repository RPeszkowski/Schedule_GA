using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfejs do wyświetlania grafiku na UI.
    internal interface IViewSchedule : IUserNotifier
    {
        //Zdarzenie zgłaszające, że użytkownik chce dodać pracownika do zmiany.
        event Action<int, int> EmployeeAddedToShift;

        //Zdarzenie zgłaszające, że użytkownik chce wyczyścić grafik.
        event Action ScheduleCleared;

        //Zdarzenie zgłaszające, że użytkownik chce przypisać funkcje.
        event Action<IEnumerable<(int ShiftId, int EmployeeId)>, FunctionTypes> SelectedShiftsAssigned;

        //Zdarzenie zgłaszające, że użytkownik chce usunąć pracownika ze zmiany.
        event Action<IEnumerable<(int ShiftId, int EmployeeId)>> SelectedShiftsRemoved;

        //Odświeżamy zmiany kontrolki.
        void UpdateShift(int shiftId, List<string> lista);
    }
}
