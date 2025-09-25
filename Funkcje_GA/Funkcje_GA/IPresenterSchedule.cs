using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{
    //Ten interfejs odpowiada za połączenie klasy ViewSchedule z resztą kodu.
    public interface IPresenterSchedule
    {
        //Akcja zmiana kontrolki grafiku.
        event Action<int, List<string>> ScheduleControlChanged;

        //Wyświetlamy dane wybranej zmiany.
        void UpdateScheduleControl(Shift shift);
    }
}
