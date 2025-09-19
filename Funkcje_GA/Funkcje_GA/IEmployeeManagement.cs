using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy EmployeeManagement z resztą kodu.
    public interface IEmployeeManagement
    {
        //Event, który zostaje wywołany gdy zmienią się dane pracownika.
        event Action<Employee> EmployeeChanged;

        //Event, który zostaje wywołany, gdy pracownik zostaje usunięty.
        event Action<int> EmployeeDeleted;

        void EmployeeAdd(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc);
        void EmployeeEdit(Employee employee, double wymiarEtatu);
        void EmployeeEdit(Employee employee, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc);
        void EmployeeDelete(Employee employee);
        IEnumerable<Employee> GetAllActive();
        Employee GetEmployeeById(int numer);
    }
}
