using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfesjs wczytywania/zapisywania danych pracowników..
    public interface IEmployeesFileService
    {
        //Wczytywanie paacowników.
        void WczytajPracownikow(string plik);

        //Zapisywanie pracowników.
        void ZapiszPracownikow(string plik);
    }
}
