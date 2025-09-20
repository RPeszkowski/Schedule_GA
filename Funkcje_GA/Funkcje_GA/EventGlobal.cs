using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ta klasa definiuje zdarzenia globalne.
    public class EventGlobal
    {
        //Akcja klikęcie głównego okna.
        public static event Action MainWindowClick;

        //Wezwanie subskrybentów akcji.
        public static void RaiseForm1Click() => MainWindowClick?.Invoke();
    }
}
