using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constants;

namespace Funkcje_GA.Presenter
{
    //Abstrakcyjna klasa zajmująca się tworzeniem neutralnych obiektów, na których wyświetlany będzie grafik.
    internal abstract class AbstractShiftRenderer : IScheduleRenderer
    {
        protected readonly IShiftControl[] elementSchedule = new IShiftControl[2 * LICZBA_DNI];                             //Tworzenie listboxów grafiku.

        //Konstruktor
        public AbstractShiftRenderer() { }

        //Prywatny delegat, który możemy przekazywać.
        protected Action<int, int> DropHandler => (shiftId, employeeId) => Drop?.Invoke(shiftId, employeeId);

        //Dodajemy element.
        public void Add(int id, List<string> lista)
        {
            foreach (var item in lista)
                elementSchedule[id].Add(item);
        }

        //Zdarzenie - dodanie pracownika do zmiany.
        public event Action<int, int> Drop;

        //Czyścimy kontrolkę.
        public void Clear(int id) => elementSchedule[id].Clear();

        //Czyścimy wybrane indeksy.
        public void ClearSelected()
        {
            foreach (var ctrl in elementSchedule)
                ctrl.ClearSelected();
        }

        //Pobieramy numery zaznaczonych pracowników na wszytskich zmianach.
        public IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds()
        {
            var result = new List<(int ShiftId, int EmployeeId)>();     //Zwracany element.

            //Sprawdzamy po kolei którzy pracownicy i które zmiany są wybrane.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                int selectedIndex = elementSchedule[shiftId].SelectedIndex;  //Sprawdzamy zaznaczony indeks w danej kontrolce.

                //Jeśli kontrolka ma zaznaczony element to pobieramy numer zaznaczonego pracownika.
                if (selectedIndex != -1)
                {
                    try
                    {
                        int employeeId = elementSchedule[shiftId].GetNumber(selectedIndex);
                        result.Add((shiftId, employeeId));
                    }

                    catch (Exception ex)
                    {
                        throw new FormatException($"Kontrolka: {shiftId} ma niepoprawne dane {ex.Message}.", ex);
                    }
                    ;
                }
            }

            return result;
        }

        //Inicjalizacja kontrolek.
        public abstract void Initialize();

        //Usunięcie podświetlenia.
        public void UsunPodswietlenie()
        {
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                elementSchedule[nrZmiany].ResetBackColor();
        }
    }
}

