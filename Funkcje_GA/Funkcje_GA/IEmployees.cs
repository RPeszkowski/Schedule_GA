using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy EmployeeManagement z resztą kodu.
    public interface IEmployees
    {
        IEnumerable<Employee> GetAllEmployees();
        IEnumerable<Employee> GetAllEmployeesNotNull();
        Employee GetEmployeeById(int numer);
    }
}
