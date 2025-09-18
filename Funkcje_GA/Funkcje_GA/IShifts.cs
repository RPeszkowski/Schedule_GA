using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy ShiftManagement z resztą kodu.
    public interface IShifts
    {
        Shift GetShiftById(int id);
    }
}
