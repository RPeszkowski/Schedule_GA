using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ta klasa definiuje zdarzenia globalne.
    public class EventGlobal
    {


        //Akcja pomyślne zakończńczenie optymalizacji.
        public static event Action<bool[]> OptimizationFinishedSuccesfuly;

        //Wezwanie subskrybentów akcji OptimizationFinishedSuccesfuly.
        public static void RaiseOptimizationFinishedSuccesfuly(bool[] optymalneRozwiązanie) => OptimizationFinishedSuccesfuly?.Invoke(optymalneRozwiązanie);

    }
}
