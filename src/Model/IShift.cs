using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA.Model
{
    //Interfejs pojedynczej zmiany.
    public interface IShift
    {
        //Id zmiany.
        int Id { get; }

        //Dodawanie pracownika do zmiany.
        bool AddEmployeeToShift(Employee employee);

        //Przypisanie funkcji
        bool AssignFunction(Employee employee, FunctionTypes function);

        //Czyścimy zmianę.
        bool ClearShift();

        //Pobieramy wszystkich pracowników.
        IEnumerable<Employee> GetEmployees();

        //Pobieramy wszystkich pracowników pełniących daną funkcję.
        IEnumerable<Employee> GetEmployeesByFunction(FunctionTypes function);

        //Usuwanie pracownika ze zmiany.
        bool RemoveEmployeeFromShift(Employee employee);
    }
}
