using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA.Presenter
{
    //Interfejs do renderera pracowników na Winforms.
    internal interface IEmployeeRendererWinforms : IEmployeeRenderer
    {
        //Pobieramy kontrolkę
        Control GetControlById(int id);
    }
}
