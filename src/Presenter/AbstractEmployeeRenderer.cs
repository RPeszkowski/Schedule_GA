using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Constants;

namespace Funkcje_GA.Presenter
{
    //Abstrakcyjna klasa zajmująca się tworzeniem neutralnych obiektów, na których wyświetlane będą informacje o pracownikach.
    internal abstract class AbstractEmployeeRenderer : IEmployeeRenderer
    {
        protected readonly Dictionary<int, IEmployeeControl> elementEmployee = new Dictionary<int, IEmployeeControl>(MAX_LICZBA_OSOB);   //Tworzenie etykiet pracowników.

        //Prywatny delegat, który możemy przekazywać.
        protected Action<int> DropHandler => (employeeId) => EmployeeLabelMouseDown?.Invoke(employeeId);

        //Zdarzenie - naciśnięcie etykiety.
        public event Action<int> EmployeeLabelMouseDown;

        //Inicjalizujemy kontrolki.
        public abstract void Initialize();

        //Uaktualniamy informacje o pracowniku.
        public abstract void UpdateEmployeeControl(int employeeId, (string data, EnumLabelStatus status) info, bool tag);
    }
}
