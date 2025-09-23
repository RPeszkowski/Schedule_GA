using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Intrefejs prezentera do zarządzania etykietami pracowników.
    public interface IViewEmployee
    {
        //Akcja zmiana danych pracownika.
        event Action<int, (string, int)> EmployeeLabelChanged;

        //Usuwanie danych pracownika.
        void ClearEmployeeLabel(int id);

        //Drag and drop etykiety pracownika.
        IEnumerable<(int shiftId, Color color)> HandleEmployeeMouseDown(int employeeId);

        //Wyświetlamy dane wybranej zmiany.
        void UpdateEmployeeLabel(Employee employee);
    }
}
