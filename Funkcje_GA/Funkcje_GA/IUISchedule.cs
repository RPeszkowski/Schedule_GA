using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy UIManagement z resztą kodu.
    public interface IUISchedule
    {
        void AddToControl(int id, string data);

        void AddToControl(int id, int data);

        void ClearControlById(int id);

        List<string> GetElementItemsByIdAsList(int id);

        ListBoxGrafik GetElementById(int id);
        int GetSelectedEmployeeFunction(int id, int index);

        int GetSelectedEmployeeNumber(int id, int index);

        int GetSelectedIndex(int id);
    }
}
