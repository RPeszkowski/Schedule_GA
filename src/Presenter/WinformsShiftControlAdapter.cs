using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA.Presenter
{
    //Adapter do tworzenia listboxów.
    internal class WinformsShiftControlAdapter : IShiftControl
    {
        private readonly ListBoxGrafik listBox;             //Listbox.
        private readonly int id;                            //Id.
        private Action<int, int> dropCallback;              //Prywatny delegat do przekazywania przy drag and drop.

        public bool AllowDrop { get => listBox.AllowDrop; set => listBox.AllowDrop = value; }       //Allow drop.
        public int SelectedIndex => listBox.SelectedIndex;                                          //Wybrany indeks.

        //Konstruktor.
        public WinformsShiftControlAdapter(int id, ListBoxGrafik listBox)
        {
            this.id = id;
            this.listBox = listBox;
        }

        //Dodawanie pracowników.
        public void Add(string item) => listBox.Items.Add(item);

        //Czyszczenie kontrolki.
        public void Clear() => listBox.Items.Clear();

        //Pobieramy numer pracownika o konkretnym indeksie.
        public int GetNumber(int index) => listBox.GetNumber(index);

        //Czyścimy zaznaczenie.
        public void ClearSelected() => listBox.ClearSelected();

        //Resetujemy kolor tła.
        public void ResetBackColor() => listBox.ResetBackColor();

        //Inicjalizacja kontrolek i zdarzenia drag and drop.
        public void Initialize(Action<int, int> dropCallback)
        {
            this.dropCallback = dropCallback;

            //Tworzenie kontrolek grafiku.
            listBox.Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            listBox.Size = new System.Drawing.Size(40, 400);
            listBox.AllowDrop = true;

            //Przypisujemy delegaty do zdarzeń drag and drop.
            listBox.DragEnter += (sender, e) =>
            {
                //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
                if (e.Data.GetDataPresent(DataFormats.Text) && e.Data.GetData(DataFormats.Text).ToString().Length != 0)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };

            listBox.DragDrop += (sender, e) =>
            {
                //Pobieramy dane i dodajemy osobę do zmiany.
                string pom = e.Data.GetData(DataFormats.Text).ToString();
                dropCallback?.Invoke(listBox.Id, Convert.ToInt32(pom));
            };
        }

        //Wrapper do Control.
        public Control ListBoxAsControl => listBox;
    }
}
