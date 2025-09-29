using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    internal interface IViewForm2 : IUserNotifier
    {      
        //Zdarzenie zgłaszające, że użytkownik chce dodać pracownika do repozytorium.
        event Action<string, string, int, bool, bool> EmployeeAddedFromUI;

        //Zdarzenie zgłaszające, że użytkownik chce edytować dane pracownika.
        event Action<int, string, string, int, bool, bool> EmployeeEditedFromUI;

        //Zdarzenie zgłaszające, że użytkownik chce usunąć dane pracownika.
        event Action<int> EmployeeDeletedFromUI;

        //Zdarzenie zgłaszające, że załadowano Form2.
        event Action Form2Loaded;

        //Zdarzenie zgłaszające, że użytkownik chce zapisać pracowników
        event Action SaveEmployees;

        //Zdarzenie zgłaszające, że zmieniono wybraną osobę i trzeba wyświetlić nowe dane.
        event Action<int> SelectedEmployeeChanged;

        //Wyświetlamy dane wybranego pracownika.
        void ShowEmployeeData(string imie, string nazwisko, int zaleglosci, bool triazDzien, bool triazNoc);

        //Wyświetlamy numery istniejących w systemie osób.
        void UpdateControlNumerOsoby(List<int> listaNumerow);
    }
}
