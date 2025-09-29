using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Obiekty tej klasy przechowują informacje o zmianie.
    public class Shift
    {
        public int Id { get; set; }                                 //Numer id zmiany 0 - 30 dzienne zmiany, 31 - 61 nocne zmiany.
        public List<Employee> PresentEmployees { get; set; }       //Pracownicy na zmianie.
        public List<Employee> SalaEmployees { get; set; }          //Pracownicy na sali.
        public List<Employee> TriazEmployees { get; set; }         //Pracownicy na triażu.

        //Konstruktor.
        public Shift(int Id)
        {
            PresentEmployees = new List<Employee>();
            SalaEmployees = new List<Employee>();
            TriazEmployees = new List<Employee>();
            this.Id = Id;
        }
    }
}
