using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy ShiftManagement z resztą kodu.
    public interface IScheduleManagement
    {
        //Event wywoływany przy zmianie grafiku.
        event Action<Shift> ShiftChanged;
        void AddToShift(int shiftId, int employeeId);

        Shift GetShiftById(int id);

        IEnumerable<(int shiftId, int function)> GetShiftsForEmployee(int employeeId);

        void RemoveAll();

        bool RemoveFromShift(int shiftId, int employeeId);

        void ToBezFunkcji(int shiftId, int employeeId);

        void ToSala(int shiftId, int employeeId);

        void ToTriaz(int shiftId, int employeeId);
    }
}
