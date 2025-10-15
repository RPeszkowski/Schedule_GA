using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA.Presenter
{
    //Struktura określa kolor etykiety pracownika i nie zalezy od implementacji UI.
    public struct EmployeeColor
    {
        public byte R { get; set; }         //Kolor czerwony.
        public byte G { get; set; }         //Kolor zielony.
        public byte B { get; set; }         //Kolor niebieski.

        //Konstruktor.
        public EmployeeColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        //Przypisujemy czarny.
        public static EmployeeColor Black => new EmployeeColor(0, 0, 0);

        //Przypisujemy pomarańczowy.
        public static EmployeeColor Orange => new EmployeeColor(255, 165, 0);
    }
}
