using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA.Presenter
{
    //Neutralny interfejs do renderera pracowników.
    internal interface IEmployeeRenderer
    {
        //Zdarzenie - naciśnięcie etykiety.
        event Action<int> EmployeeLabelMouseDown;

        //Inicjalizacja elementów.
        void Initialize();

        //Uaktulaniamy informacje o pracowniku.
        void UpdateEmployeeControl(int employeeId, (string data, EnumLabelStatus status) info, bool tag);
    }
}
