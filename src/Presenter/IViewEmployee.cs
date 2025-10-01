using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie presenterEmployee z UI.
    internal interface IViewEmployee : IUserNotifier
    {
        //Zdarzenie zgłaszające, że uzytkownik kliknął na etykietę pracownika.
        event Action<int> EmployeeLabelMouseDown;

        //Obsługa drag and drop - podświetlenie kontrolek.
        void HandleEmployeeMouseDown(IEnumerable<(int shiftId, Color color)> highlights);

        //Odświeżanie etykiet
        void UpdateEmployeeLabel(int employeeId, (string data, EnumLabelStatus status) info, bool tag);
    }
}
