using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Klasa zawiera zmienne stałowartościowe.
    public class Constans
    {
        public const int LICZBA_DNI = 31;                                                          //Największa liczba dni w miesiącu.
        public const int LICZBA_ZMIENNYCH = 2 * LICZBA_DNI * 3 * MAX_LICZBA_BITOW;                 //Liczba zmiennych w zadaniu optymalizacji. 
        public const int MAX_LICZBA_BITOW = 3;                                                     //Liczba bitów potrzebna do zakodowania jednej osoby (log2(MAX_LICZBA_DYZUROW)).
        public const int MAX_LICZBA_DYZUROW = 8;                                                   //Maksymalna liczba dyżurów jednego dnia.
        public const int MAX_LICZBA_OSOB = 50;                                                      //Maksymalna liczba pracowników w systemie. 
    }
}
