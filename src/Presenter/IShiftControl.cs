using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA.Presenter
{
    //Neutralny interfejs kontrolki grafiku.
    public interface IShiftControl
    {
        //Umożliwiamy drop w drag and drop.
        bool AllowDrop { get; set; }

        //Indeks wybranej osoby.
        int SelectedIndex { get; }

        //Dodawanie elementu.
        void Add(string item);

        //Czyszczenie kontrolki.
        void Clear();

        //Pobieramy numer osoby znajdującej się na danej pozycji.
        int GetNumber(int index);

        //Czyścimy wybranie.
        void ClearSelected();

        //Resetujemy kolor.
        void ResetBackColor();
    }
}
