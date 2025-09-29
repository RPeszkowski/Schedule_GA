using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfejs do powiadamiania użytkownika.
    internal interface IUserNotifier
    {
        //Powiadomienie użytkownika.
        void RaiseUserNotification(string message);
    }
}
