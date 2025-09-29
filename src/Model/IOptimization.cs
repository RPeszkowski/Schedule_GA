using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    public interface IOptimization
    {
        //Informacja o przebiegu optymalizacji.
        event Action<string> ProgressUpdated;

        //Event do powiadamiania o ostrzeżeniach.
        event Action<string> WarningRaised;

        //Algorytm optymalizacji genetycznej.
        bool[] OptymalizacjaGA(int liczbaZmiennych, int liczbaOsobnikow, decimal tol, decimal tolX, int maxKonsekwentnychIteracji, int maxIteracji);

        //Przygotowanie danych do optymalizacji.
        void Prepare();
    }
}
