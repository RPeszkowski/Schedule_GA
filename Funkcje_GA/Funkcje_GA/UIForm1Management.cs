using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constans;
using static Funkcje_GA.Form1;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za zarządzanie wyświetlaniem grafiku i informacji o pracownikach na UI.
    public class UIForm1Management : IUIManagement
    {
        private readonly Dictionary<int, System.Windows.Forms.Label> uiEmployeesControls;           //Tu przechowywane są kontrolki z danymi pracowników.
        private readonly List<ListBoxGrafik> uiScheduleControls;                                    //Tu przechowywane są kontrolki z danymi grafiku.

        public UIForm1Management()
        {
            uiEmployeesControls = new Dictionary<int, System.Windows.Forms.Label>(MAX_LICZBA_OSOB);     //Inicjalizujemy zestaw kontrolek pracowników.
            uiScheduleControls = new List<ListBoxGrafik>(2 * LICZBA_DNI);                               //Inicjalizujemy zestaw kontrolek grafiku.

            //Dodajemy listboxy z grafikiem.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                ListBoxGrafik listBoxGrafik = new ListBoxGrafik(nrZmiany);
                uiScheduleControls.Add(listBoxGrafik);
                uiScheduleControls[nrZmiany].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiScheduleControls[nrZmiany].Size = new System.Drawing.Size(40, 400);
                uiScheduleControls[nrZmiany].AllowDrop = true;
            }

            //Dodajemy etykiety pracowników.
            for (int nrOsoby = 0; nrOsoby < MAX_LICZBA_OSOB; nrOsoby++)
            {
                uiEmployeesControls[nrOsoby] = new System.Windows.Forms.Label();
                uiEmployeesControls[nrOsoby].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiEmployeesControls[nrOsoby].Size = new System.Drawing.Size(340, 40);
                uiEmployeesControls[nrOsoby].Text = "";
            }

            //Usuwamy kolory i podświetlenie po kliknięciu głównego okna.
            EventGlobal.MainWindowClick += () =>
            {
                for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                {
                    this.ChangeColor(nrZmiany, (int)FunctionTypes.Default);
                    uiScheduleControls[nrZmiany].ClearSelected();
                }
            };
        }

        //Zmiana koloru kontroki z grafikiem.
        public void ChangeColor(int id, int color)
        {
            //Sprawdzamy, cy kolor jest poprawny.
            if (color != 0 && color != 1 && color != 2 && color != 99)
                throw new InvalidOperationException("Zmienna 'color' przyjęłą niedozwololną wartość " + color.ToString() + " .");

            //Sprawdzamy, czy id kontrolki jest poprawne.
            if (id < 0 || id >= uiScheduleControls.Count) throw new UIInvalidScheduleControlIdException("Wybrano niepoprawny numer kontrolki");

            //Wyświetlamy kolor w zależności od rodzaju funkcji, lub resetujemy kolor.
            switch(color)
            {
                case (int)FunctionTypes.Bez_Funkcji:
                    uiScheduleControls[id].BackColor = Color.Red;
                    break;

                case (int)FunctionTypes.Sala:
                    uiScheduleControls[id].BackColor = Color.Green;
                    break;

                case (int)FunctionTypes.Triaz:
                    uiScheduleControls[id].BackColor = Color.Blue;
                    break;

                case (int)FunctionTypes.Default:
                    uiScheduleControls[id].ResetBackColor();
                    break;
            }
        }

        //Czyścimy kontrolkę z danymi pracownika.
        public void ClearEmployeeData(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli tak, to resetujemy tekst.
            if (id < 0 || id >= uiEmployeesControls.Count) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            else  uiEmployeesControls[id].Text = ""; 
        }

        //Wyświetlamy dane wybranej zmiany.
        public void DisplayScheduleControl(Shift shift)
        {
            //Czyścimy kontrolkę.
            uiScheduleControls[shift.Id].Items.Clear();

            //Wyświetlamy pracowników
            if (shift.Present_employees.Count > 0)
            {

                //Próbujemy dodać pracowników do kontrolki.
                try
                {
                    for (int nrOsoby = 0; nrOsoby < shift.Present_employees.Count; nrOsoby++)
                    {
                        //Dopisujemy pracownika.
                        uiScheduleControls[shift.Id].Items.Add(shift.Present_employees[nrOsoby].Numer.ToString());
                    }

                    for (int nrOsoby = 0; nrOsoby < shift.Sala_employees.Count; nrOsoby++)
                    {
                        //Odznaczamy salę.
                        uiScheduleControls[shift.Id].ToSala(GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Sala_employees[nrOsoby].Numer.ToString()));
                    }

                    for (int nrOsoby = 0; nrOsoby < shift.Triaz_employees.Count; nrOsoby++)
                    {
                        //Odznaczamy salę.
                        uiScheduleControls[shift.Id].ToTriaz(GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Triaz_employees[nrOsoby].Numer.ToString()));
                    }

                }

                catch(Exception ex)
                {
                    throw new FormatException($"Kontrolka: {shift.Id} ma niepoprawne dane {ex.Message}.", ex);
                };
            }
        }

        //Zwracamy wszyskie elementy, w których zaznaczono pracownika.
        public IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds()
        {
            var result = new List<(int ShiftId, int EmployeeId)>();     //Zwracamy pracy element pracownik.

            //Sprawdzamy po kolei którzy pracownicy i które zmiany są wybrane.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                int selectedIndex = uiScheduleControls[shiftId].SelectedIndex;  //Sprawdzamy zaznaczony indeks w danej kontrolce.

                //Jeśli kontrolka ma zaznaczony element to pobieramy numer zaznaczonego pracownika.
                if (selectedIndex != -1)
                {
                    try
                    {
                        int employeeId = uiScheduleControls[shiftId].GetNumber(selectedIndex);
                        result.Add((shiftId, employeeId));
                    }

                    catch(Exception ex)
                    {
                        throw new FormatException($"Kontrolka: {shiftId} ma niepoprawne dane {ex.Message}.", ex);
                    };
                }
            }

            return result;
        }

        //Zwracamy zawartość kontrolki grafiku w postaci listy.
        public List<string> GetElementItemsByIdAsList(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne i jeśli tak, to zwracamy zawartość.
            if (id < 0 || id >= uiScheduleControls.Count) throw new UIInvalidScheduleControlIdException("Wybrano niepoprawny numer kontrolki");

            else return uiScheduleControls[id].Items.Cast<string>().ToList();
        }

        //Zwarcamy kontrolkę z danymi pracownika.
        public System.Windows.Forms.Label GetEmployeeControlById(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli tak, to zwracamy kontrolkę.
            if (id < 0 || id >= uiEmployeesControls.Count) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            else return uiEmployeesControls[id];
        }

        public ListBoxGrafik GetScheduleControlById(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne i jeśli tak, to zwracamy kontrolkę.
            if (id < 0 || id >= uiScheduleControls.Count) throw new UIInvalidScheduleControlIdException("Wybrano niepoprawny numer kontrolki");

            else return uiScheduleControls[id]; 
        }

        //Wyświetlanie informacji o pracowniku na etykiecie.
        public void UpdateEmployeeLabel(Employee employee)
        {
            if (employee != null)
            {
                //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli nie, to rzucamy wyjątek.
                if (employee.Numer - 1 < 0 || employee.Numer - 1 >= uiEmployeesControls.Count) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

                //Aktualizujemy pojedynczą etykietę.
                string employeeData = employee.Numer.ToString() + ". "
                                    + employee.Imie + " " + employee.Nazwisko + " "
                                    + employee.WymiarEtatu.ToString() + " "
                                    + employee.Zaleglosci.ToString();
                uiEmployeesControls[employee.Numer - 1].Text = employeeData;

                //Jeśli osoba jest stażystą i nie może być na triażu podczas zmiany to podświetlamy na pomarańczowo.
                if (employee.CzyTriazDzien &&  employee.CzyTriazNoc)
                    uiEmployeesControls[employee.Numer - 1].ForeColor = Color.Black;

                else
                    uiEmployeesControls[employee.Numer - 1].ForeColor = Color.Orange;
            }
        }
    }
}
