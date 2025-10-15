using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA.Presenter
{
    //Neutralny interfejs kontrolki pracownika.
    internal interface IEmployeeControl
    {
        //Pole ForeColor.
        EmployeeColor ForeColor { get;  set; }

        //Pole Text.
        string Text { get; set; }

        //Pole Tag.
        int? Tag {  get; set; }

        //Inicjalizacja kontrolki.
        void Initialize(Action<int> dragCallback);
    }
}
