using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfejs pośredniczący między UI a prezenterem obsługującym pliki.
    public interface IViewFile : IUserNotifier
    {
        //Zdarzenie - zmiana daty.
        event Action<string, string> DateChanged;

        //Zdarzenie - załaduj grafik i pracowników przy starcie programu.
        event Action LoadAtStart;

        //Zdarzenie - załadowanie grafiku.
        event Action ViewLoadSchedule;

        //Zdarzenie - zapisanie grafiku po optymalizacji.
        event Action SaveOptimalSchedule;

        //Zdarzenie - zapisanie grafiku.
        event Action ViewSaveSchedule;

        //Pytamy użytkownika.
        bool AskUserConfirmation(string message);
    }
}
