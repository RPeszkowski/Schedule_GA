using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Intrefejs prezentera do zarządzania plikami.
    public interface IViewFile
    {

        //Powiadomienie użytkownika.
        event Action<string> UserNotificationRaise;

        //Wczytywanie pracowników.
        void LoadEmployees(string filePath);

        //Wczytywanie grafiku.
        void LoadSchedule(string filePath);

        //Zapisz pracowników.
        void SaveEmployees(string filePath);

        //Zapisywanie grafiku.
        void SaveSchedule(string filepath);
    }
}
