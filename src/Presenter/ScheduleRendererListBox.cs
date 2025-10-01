using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constants;

namespace Funkcje_GA.Presenter
{
    //Klasa odpowiada za stworzenie listboxów grafiku na Winforms.
    internal class ScheduleRendererListBox : AbstractShiftRenderer, IScheduleRendererWinforms
    {
        private readonly Label[] labelsDzien = new Label[LICZBA_DNI];           //Etykiety grafiku.
        
        //Konstruktor
        public ScheduleRendererListBox() 
        {
            //Tworzymy kontrolki.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                var listBox = new ListBoxGrafik(nrZmiany);
                elementSchedule[nrZmiany] = new WinformsShiftControlAdapter(nrZmiany, listBox);
            }
        }

        //Pobieramy wszystkie.
        public IEnumerable<Control> GetAll() => elementSchedule.OfType<WinformsShiftControlAdapter>().Select(a => a.ListBoxAsControl);

        //Pobieramy kontrolkę
        public Control GetControlById(int id) => ((WinformsShiftControlAdapter)elementSchedule[id]).ListBoxAsControl;

        //Pobieramy etykietę.
        public Control GetLabel(int id) => labelsDzien[id];

        //Inicjalizacja kontrolek.
        public override void Initialize()
        {
            //Tworzenie etykiet i dodawanie kontrolek grafiku.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                ((WinformsShiftControlAdapter)elementSchedule[nrZmiany]).Initialize(DropHandler);

                //Tworzymy etykiety dla dyżurów dziennych.
                if (nrZmiany < LICZBA_DNI)
                {
                    //Tworzenie etykiet.
                    labelsDzien[nrZmiany] = new System.Windows.Forms.Label();
                    labelsDzien[nrZmiany].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                    labelsDzien[nrZmiany].Size = new System.Drawing.Size(340, 40);
                    labelsDzien[nrZmiany].Text = (nrZmiany + 1).ToString();
                }
            }
        }
    }
}
