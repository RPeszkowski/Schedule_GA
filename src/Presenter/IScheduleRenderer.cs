using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA.Presenter
{
    //Neutralny interfejs do renderowania kontrolek grafiku.
    internal interface IScheduleRenderer
    {
        //Zdarzenie - dodanie pracownika do zmiany.
        event Action<int, int> Drop;

        //Dodajemy element.
        void Add(int id, List<string> lista);

        //Czyścimy kontrolkę.
        void Clear(int id);

        //Czyścimy wybrane indeksy.
        void ClearSelected();

        //Zwracamy id zaznaczonych pracowników.
        IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds();

        //Inicjalizacja kontrolek.
        void Initialize();

        //Usunięcie podświetlenia.
        void UsunPodswietlenie();
    }
}
