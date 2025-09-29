using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Wczytywanie/zapis grafiku.
    public interface IScheduleFileService
    {
        //Wczytywanie grafiku.
        void WczytajGrafik(string plik);

        //Zapis grafiku.
        void ZapiszGrafik(string plik);
    }
}
