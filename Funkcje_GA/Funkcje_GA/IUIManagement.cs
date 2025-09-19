using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy UIManagement z resztą kodu.
    public interface IUIManagement
    {
        void ChangeColor(int id, int color);

        void ClearIndex(int id);

        IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds();

        List<string> GetElementItemsByIdAsList(int id);

        string GetEmployeeData(int id);

        void ClearEmployeeData(int id);
    }
}
