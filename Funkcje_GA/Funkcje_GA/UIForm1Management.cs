using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;

namespace Funkcje_GA
{
    public class UIForm1Management : IUISchedule
    {
        public List<ListBoxGrafik> uiScheduleControls;
        private System.Windows.Forms.Label[] labelsDzien;                      //Tworzenie etykiet wyświetlających numer dziennej zmiany.
        private System.Windows.Forms.Label[] labelsNoc;                        //Tworzenie etykiet wyświetlających numer nocnej zmiany.

        public UIForm1Management()
        {
            uiScheduleControls = new List<ListBoxGrafik>(2 * LICZBA_DNI);
            labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];
            labelsNoc = new System.Windows.Forms.Label[LICZBA_DNI];

            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                ListBoxGrafik listBoxGrafik = new ListBoxGrafik(nrZmiany);
                uiScheduleControls.Add(listBoxGrafik);
            }
        }

        public void AddToControl(int id, string data) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).Items.Add(data);

        public void AddToControl(int id, int data) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).Items.Add(data);

        public void ClearControlById(int id) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).Items.Clear();

        public List<string> GetElementItemsByIdAsList(int id) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).Items.Cast<string>().ToList();

        public ListBoxGrafik GetElementById(int id) => uiScheduleControls.First(uiControl => (uiControl.Id == id));

        public void InitializeControls()
        {
            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                //Tworzymy listBoxy dla dyżurów dziennych.
                uiScheduleControls[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiScheduleControls[nrDnia].Size = new System.Drawing.Size(40, 400);
                uiScheduleControls[nrDnia].AllowDrop = true;

                //Tworzymy etykiety dla dyżurów dziennych.
                labelsDzien[nrDnia] = new System.Windows.Forms.Label();
                labelsDzien[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsDzien[nrDnia].Size = new System.Drawing.Size(340, 40);
                labelsDzien[nrDnia].Text = (nrDnia + 1).ToString();
            }

            for (int nrDnia = 0; nrDnia < LICZBA_DNI; nrDnia++)
            {
                //Tworzymy listBoxy dla dyżurów nocnych.
                uiScheduleControls[nrDnia + LICZBA_DNI].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiScheduleControls[nrDnia + LICZBA_DNI].Size = new System.Drawing.Size(40, 400);
                uiScheduleControls[nrDnia + LICZBA_DNI].AllowDrop = true;

                //Tworzymy etykiety dla dyżurów nocnych.
                labelsNoc[nrDnia] = new System.Windows.Forms.Label();
                labelsNoc[nrDnia].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsNoc[nrDnia].Size = new System.Drawing.Size(340, 40);
                labelsNoc[nrDnia].Text = (nrDnia + 1).ToString();
            }
        }

        public int GetSelectedEmployeeFunction(int id, int index) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).GetFunction(index);

        public int GetSelectedEmployeeNumber(int id, int index) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).GetNumber(index);

        public int GetSelectedIndex(int id) => uiScheduleControls.First(uiControl => (uiControl.Id == id)).SelectedIndex;
    }
}
