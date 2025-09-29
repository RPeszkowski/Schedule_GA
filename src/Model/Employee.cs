using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Obiekty tej klasy to pracownicy.
    public class Employee
    {
        public int Numer { get; set; }                  //Numer osoby.
        public string Imie { get; set; }                //Imię osoby.
        public string Nazwisko { get; set; }            //Nazwisko osoby.
        public double WymiarEtatu { get; set; }         //Wymiar etatu osoby.
        public int Zaleglosci { get; set; }             //Zaległości osoby.
        public bool CzyTriazDzien { get; set; }         //Czy osobie można przydzielać triaż na dziennej zmianie?
        public bool CzyTriazNoc { get; set; }           //Czy osobie można przydzielać triaż na nocnej zmianie?

        //Konstruktor
        public Employee(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool czyTriazDzien, bool czyTriazNoc)
        {
            this.Numer = numer;
            this.Imie = imie;
            this.Nazwisko = nazwisko;
            this.WymiarEtatu = wymiarEtatu;
            this.Zaleglosci = zaleglosci;
            this.CzyTriazDzien = czyTriazDzien;
            this.CzyTriazNoc = czyTriazNoc;
        }
    }
}
