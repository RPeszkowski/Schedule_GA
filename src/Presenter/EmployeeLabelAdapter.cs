using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA.Presenter
{
    internal class EmployeeLabelAdapter : IEmployeeControl
    {
        private readonly System.Windows.Forms.Label label;          //Etykieta pracownika.
        private readonly int nrOsoby;                               //Id pracownika, którego dane są na etykiecie.

        //Konstruktor.
        public EmployeeLabelAdapter(int nrOsoby)
        {
            this.label = new System.Windows.Forms.Label();
            this.nrOsoby = nrOsoby;
        }

        //Pole ForeColor.
        public EmployeeColor ForeColor
        {
            get => new EmployeeColor(label.ForeColor.R, label.ForeColor.G, label.ForeColor.B);
            set => label.ForeColor = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
        }

        //Pole Tag.
        public int? Tag
        {
            get => label.Tag as int?;
            set => label.Tag = value;
        }

        //Pole Text.
        public string Text
        {
            get => label.Text;
            set => label.Text = value;
        }

        //Zwracamy etykietę jako kontrolkę.
        public Control AsControl => label;

        //Inicjalizacja etykiety.
        public void Initialize(Action<int> dragCallback)
        {
            //Dodawanie etykiet.
            label.Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            label.Size = new System.Drawing.Size(340, 40);
            label.Text = "";

            //Przypisujemy lambdy do zdarzeń drag and drop.
            int _nrOsoby = nrOsoby;
            label.MouseDown += (sender, e) =>
            {
                //Sprawdzamy, czy etykieta nie jest pusta.
                if (label.Tag == null)
                    return;

                //Wywołujemy event w presenterze.
                dragCallback?.Invoke(_nrOsoby);

                //Rozpoczynamy drag & drop.
                label.DoDragDrop(label.Tag.ToString(), DragDropEffects.Copy | DragDropEffects.Move);
            };
        }
    }
}
