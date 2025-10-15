using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constants;

namespace Funkcje_GA.Presenter
{
    //Klasa odpowiada za tworzenie elementów UI pracowników na Winforms.
    internal class EmployeeRendererWinforms : AbstractEmployeeRenderer, IEmployeeRendererWinforms
    {
        //Konstruktor
        public EmployeeRendererWinforms()
        {
            //Tworzymy kontrolki.
            for (int employeeNumber = 0; employeeNumber < LICZBA_ZMIAN * LICZBA_DNI; employeeNumber++)
                elementEmployee[employeeNumber] = new EmployeeLabelAdapter(employeeNumber);
        }

        //Zwracamy jako kontrolkę.
        public Control GetControlById(int id) =>  ((EmployeeLabelAdapter)elementEmployee[id]).AsControl;

        //Inicjalizacja kontrolek.
        public override void Initialize()
        {
            for (int employeeNumber = 0; employeeNumber < LICZBA_ZMIAN * LICZBA_DNI; employeeNumber++)
                elementEmployee[employeeNumber].Initialize(DropHandler);
        }

        //Uaktulaniamy informacje o pracowniku.
        public override void UpdateEmployeeControl(int employeeId, (string data, EnumLabelStatus status) info, bool tag)
        {
            //Id - numer kontrolki, Item1 - tekst, Item2 - kolor.
            elementEmployee[employeeId].Text = info.Item1;
            elementEmployee[employeeId].ForeColor = info.Item2 == EnumLabelStatus.Normal ? EmployeeColor.Black : EmployeeColor.Orange;

            //Uaktualniamy tag.
            if (tag)
                elementEmployee[employeeId].Tag = employeeId;

            else
                elementEmployee[employeeId].Tag = null;
        }
    }
}
