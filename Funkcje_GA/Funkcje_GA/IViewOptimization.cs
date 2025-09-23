using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Ten interfejś odpowiada za połączenie optymalizacji z view.
    public interface IViewOptimization
    {
        //Powiadomienie użytkownika.
        event Action<string> ProgressUpdated;

        //Powiadomienie użytkownika.
        event Action<string> UserNotificationRaise;

        //Powiadomienie użytkownika - warning.
        event Action<string> UserNotificationRaiseWarning;

        //Przeprowadzamy optymalizację.
        Task RunOptimizationAsync();
    }
}
