using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA.Presenter
{
    //Interfejs do renderera grafiku dla Winforms.
    internal interface IScheduleRendererWinforms : IScheduleRenderer
    {
        //Pobieramy wszystkie.
        IEnumerable<Control> GetAll();

        //Pobieramy kontrolkę
        Control GetControlById(int id);

        //Pobieramy etykietę.
        Control GetLabel(int id);
    }
}
