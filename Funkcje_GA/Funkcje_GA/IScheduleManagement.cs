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

        //Dodawanie pracownika do zmiany.
        void AddToShift(int shiftId, int employeeId);

        //Dopisywanie funkcji po zakończonej optymalizacji.
        void DodajFunkcje(bool[] optymalneRozwiazanie);

        //Zwraca zmianę.
        Shift GetShiftById(int id);

        //Zwraca zmiany pracownika i pełnione funkcje.
        IEnumerable<(int shiftId, int function)> GetShiftsForEmployee(int employeeId);

        //Czyści wszystkie zmiany.
        void RemoveAll();

        //Usuwa pracownika z funkcji.
        void RemoveFromShift(int shiftId, int employeeId);

        //Zmienia funkcję pracownika na bez funkcji.
        void ToBezFunkcji(int shiftId, int employeeId);

        //Zmienia funkcję pracownika na salę.
        void ToSala(int shiftId, int employeeId);

        //Zmienia funkcję pracownika na triaż.
        void ToTriaz(int shiftId, int employeeId);
    }
}
