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
        //Podświetlenie wybranego pracownika.
        void ChangeColor(int id, int color);

        //Pobieramy id wszystkich wybranych pracowników.
        IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds();

        //Pobieramy pracowników na danej zmianie jako listę.
        List<string> GetElementItemsByIdAsList(int id);

        //Usuwanie danych pracownika.
        void ClearEmployeeData(int id);
    }
}
