using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constants;

namespace Funkcje_GA.Presenter
{
    //Klasa odpowiada za stworzenie elementów UI grafiku na Winforms.
    internal class ScheduleRendererWinforms : AbstractShiftRenderer, IScheduleRendererWinforms
    {
        private readonly Label[] labelsDzien = new Label[LICZBA_DNI];           //Etykiety grafiku.
        
        //Konstruktor
        public ScheduleRendererWinforms() 
        {
            //Tworzymy kontrolki.
            for (int nrZmiany = 0; nrZmiany < LICZBA_ZMIAN * LICZBA_DNI; nrZmiany++)
                elementSchedule[nrZmiany] = new ShiftListBoxAdapter(nrZmiany);
        }

        //Pobieramy wszystkie.
        public IEnumerable<Control> GetAll() => elementSchedule.OfType<ShiftListBoxAdapter>().Select(a => a.AsControl);

        //Pobieramy kontrolkę
        public Control GetControlById(int id) => ((ShiftListBoxAdapter)elementSchedule[id]).AsControl;

        //Pobieramy etykietę.
        public Control GetLabel(int id) => labelsDzien[id];

        //Inicjalizacja kontrolek.
        public override void Initialize()
        {
            //Tworzenie etykiet i dodawanie kontrolek grafiku.
            for (int nrZmiany = 0; nrZmiany < LICZBA_ZMIAN * LICZBA_DNI; nrZmiany++)
            {
                ((ShiftListBoxAdapter)elementSchedule[nrZmiany]).Initialize(DropHandler);

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
