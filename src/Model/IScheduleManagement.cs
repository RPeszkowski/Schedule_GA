using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Funkcje_GA.Model;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy ShiftManagement z resztą kodu.
    public interface IScheduleManagement
    {
        //Event wywoływany przy zmianie grafiku.
        event Action<IEnumerable<IShift>> ShiftChanged;

        //Dodawanie pracownika do zmiany.
        void AddToShift(int shiftId, int employeeId);

        //Dopisywanie funkcji po zakończonej optymalizacji.
        void ApplyOptimizationToSchedule(bool[] optymalneRozwiazanie);

        //Przypisuje funkcje wybranemu pracownikowi.
        void AssignFunctionToEmployee(int shiftId, int employeeId, FunctionTypes function);

        //Zwraca zmianę.
        IShift GetShiftById(int id);

        //Zwraca zmiany pracownika i pełnione funkcje.
        IEnumerable<(int shiftId, FunctionTypes function)> GetShiftsForEmployee(int employeeId);

        //Czyści wszystkie zmiany.
        void RemoveAll();

        //Usuwa pracownika z funkcji.
        void RemoveFromShift(int shiftId, int employeeId);
    }
}
